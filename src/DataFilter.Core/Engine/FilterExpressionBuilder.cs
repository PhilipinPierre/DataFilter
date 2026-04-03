using DataFilter.Core.Abstractions;
using DataFilter.Core.Enums;
using System.Linq.Expressions;
using System.Reflection;

namespace DataFilter.Core.Engine;

/// <summary>
/// Builds LINQ expression trees from filter descriptors.
/// </summary>
public static class FilterExpressionBuilder
{
    private static readonly MethodInfo StringContainsMethod =
        typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string), typeof(StringComparison) })!;
    private static readonly MethodInfo StringStartsWithMethod =
        typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string), typeof(StringComparison) })!;
    private static readonly MethodInfo StringEndsWithMethod =
        typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string), typeof(StringComparison) })!;

    /// <summary>
    /// Builds an expression representing the specified filter descriptor.
    /// </summary>
    /// <typeparam name="T">The type of the item to filter.</typeparam>
    /// <param name="descriptor">The filter descriptor.</param>
    /// <returns>An expression that evaluates the filter.</returns>
    public static Expression<Func<T, bool>> BuildExpression<T>(IFilterDescriptor descriptor)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var body = BuildBody(parameter, descriptor);

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    /// <summary>
    /// Builds a compiled Func representing the specified filter descriptor.
    /// </summary>
    /// <param name="targetType">The type of the item to filter.</param>
    /// <param name="descriptor">The filter descriptor.</param>
    /// <returns>A Func evaluating the filter against an object.</returns>
    public static Func<object, bool> BuildFunc(Type targetType, IFilterDescriptor descriptor)
    {
        var objParameter = Expression.Parameter(typeof(object), "obj");
        var castParameter = Expression.Convert(objParameter, targetType);

        var body = BuildBody(castParameter, descriptor);

        var lambda = Expression.Lambda<Func<object, bool>>(body, objParameter);
        return lambda.Compile();
    }

    /// <summary>
    /// Builds the body of the expression for a sequence of filter descriptors combined with a logical operator.
    /// </summary>
    public static Expression<Func<T, bool>> BuildExpression<T>(IReadOnlyList<IFilterDescriptor> descriptors, LogicalOperator logicalOperator = LogicalOperator.And)
    {
        if (descriptors == null || descriptors.Count == 0)
        {
            return x => true;
        }

        var parameter = Expression.Parameter(typeof(T), "x");

        Expression combinedBody = null!;

        foreach (var descriptor in descriptors)
        {
            var body = BuildBody(parameter, descriptor);

            if (combinedBody == null)
            {
                combinedBody = body;
            }
            else
            {
                combinedBody = logicalOperator == LogicalOperator.And
                    ? Expression.AndAlso(combinedBody, body)
                    : Expression.OrElse(combinedBody, body);
            }
        }

        return Expression.Lambda<Func<T, bool>>(combinedBody, parameter);
    }

    /// <summary>
    /// Builds a compiled predicate for a sequence of descriptors, for a known item type (boxed as <see cref="object"/>).
    /// </summary>
    public static Func<object, bool> BuildCombinedFunc(Type itemType, IReadOnlyList<IFilterDescriptor> descriptors, LogicalOperator logicalOperator = LogicalOperator.And)
    {
        if (descriptors == null || descriptors.Count == 0)
        {
            return _ => true;
        }

        var objParameter = Expression.Parameter(typeof(object), "obj");
        var castParameter = Expression.Convert(objParameter, itemType);

        Expression? combinedBody = null;

        foreach (var descriptor in descriptors)
        {
            var body = BuildBody(castParameter, descriptor);

            combinedBody = combinedBody == null
                ? body
                : logicalOperator == LogicalOperator.And
                    ? Expression.AndAlso(combinedBody, body)
                    : Expression.OrElse(combinedBody, body);
        }

        var lambda = Expression.Lambda<Func<object, bool>>(combinedBody!, objParameter);
        return lambda.Compile();
    }

    private static Expression BuildBody(Expression parameter, IFilterDescriptor descriptor)
    {
        if (descriptor is IFilterGroup group)
        {
            Expression groupBody = null!;
            foreach (var childDescriptor in group.Descriptors)
            {
                var body = BuildBody(parameter, childDescriptor);
                if (groupBody == null)
                {
                    groupBody = body;
                }
                else
                {
                    groupBody = group.LogicalOperator == LogicalOperator.And
                        ? Expression.AndAlso(groupBody, body)
                        : Expression.OrElse(groupBody, body);
                }
            }
            return groupBody ?? Expression.Constant(true);
        }

        var memberExpression = GetMemberExpression(parameter, descriptor.PropertyName);
        var value = descriptor.Value;

        return BuildOperationExpression(memberExpression, descriptor.Operator, value);
    }

    private static Expression GetMemberExpression(Expression parameter, string propertyName)
    {
        Expression current = parameter;
        foreach (var property in propertyName.Split('.'))
        {
            current = Expression.PropertyOrField(current, property);
        }
        return current;
    }

    private static Expression BuildOperationExpression(Expression left, FilterOperator filterOperator, object? rightValue)
    {
        switch (filterOperator)
        {
            case FilterOperator.IsNull:
                if (left.Type.IsValueType && Nullable.GetUnderlyingType(left.Type) == null)
                    return Expression.Constant(false);
                return Expression.Equal(left, Expression.Constant(null));

            case FilterOperator.IsNotNull:
                if (left.Type.IsValueType && Nullable.GetUnderlyingType(left.Type) == null)
                    return Expression.Constant(true);
                return Expression.NotEqual(left, Expression.Constant(null));
        }

        if (filterOperator == FilterOperator.Between)
        {
            return BuildBetweenExpression(left, rightValue);
        }

        if (filterOperator == FilterOperator.In || filterOperator == FilterOperator.NotIn)
        {
            return BuildInExpression(left, rightValue, filterOperator == FilterOperator.NotIn);
        }

        // Handle normal binary operations
        var right = GetConstantExpression(rightValue, left.Type);

        switch (filterOperator)
        {
            case FilterOperator.Equals:
                return Expression.Equal(left, right);
            case FilterOperator.NotEquals:
                return Expression.NotEqual(left, right);
            case FilterOperator.GreaterThan:
                return Expression.GreaterThan(left, right);
            case FilterOperator.GreaterThanOrEqual:
                return Expression.GreaterThanOrEqual(left, right);
            case FilterOperator.LessThan:
                return Expression.LessThan(left, right);
            case FilterOperator.LessThanOrEqual:
                return Expression.LessThanOrEqual(left, right);
            case FilterOperator.Contains:
                return BuildStringCallExpression(left, StringContainsMethod, right);
            case FilterOperator.NotContains:
                return Expression.Not(BuildStringCallExpression(left, StringContainsMethod, right));
            case FilterOperator.StartsWith:
                return BuildStringCallExpression(left, StringStartsWithMethod, right);
            case FilterOperator.EndsWith:
                return BuildStringCallExpression(left, StringEndsWithMethod, right);
            default:
                throw new NotSupportedException($"Operator {filterOperator} is not supported.");
        }
    }

    private static Expression BuildInExpression(Expression left, object? value, bool invert)
    {
        if (value is not System.Collections.IEnumerable enumerable || value is string)
        {
            throw new ArgumentException($"Value must be an IEnumerable for In/NotIn operations.");
        }

        var unboxMethod = typeof(Enumerable).GetMethod("Cast")!.MakeGenericMethod(typeof(object));
        var castedEnumerable = (IEnumerable<object>)unboxMethod.Invoke(null, new[] { value })!;

        var valuesList = castedEnumerable.ToList();

        if (valuesList.Count == 0)
        {
            return Expression.Constant(invert);
        }

        Expression combinedBody = null!;

        foreach (var val in valuesList)
        {
            var right = GetConstantExpression(val, left.Type);
            var equal = Expression.Equal(left, right);

            combinedBody = combinedBody == null
                ? equal
                : Expression.OrElse(combinedBody, equal);
        }

        if (invert)
        {
            return Expression.Not(combinedBody);
        }

        return combinedBody;
    }

    private static Expression GetConstantExpression(object? value, Type targetType)
    {
        if (value == null)
        {
            return Expression.Constant(null, targetType);
        }

        var nonNullableType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        object convertedValue;
        if (nonNullableType.IsEnum)
        {
            convertedValue = Enum.Parse(nonNullableType, value.ToString()!);
        }
        else
        {
            convertedValue = Convert.ChangeType(value, nonNullableType);
        }

        return Expression.Constant(convertedValue, targetType);
    }

    private static Expression EnsureString(Expression expression)
    {
        if (expression.Type == typeof(string))
            return expression;

        var toStringMethod = typeof(object).GetMethod(nameof(object.ToString));
        return Expression.Call(expression, toStringMethod!);
    }

    private static Expression BuildStringCallExpression(Expression left, MethodInfo method, Expression right)
    {
        var stringLeft = EnsureString(left);
        var stringRight = EnsureString(right);
        var comparison = Expression.Constant(StringComparison.OrdinalIgnoreCase);
        return Expression.Call(stringLeft, method, stringRight, comparison);
    }

    private static Expression BuildBetweenExpression(Expression left, object? value)
    {
        if (value is not Models.RangeValue range)
        {
            throw new ArgumentException("Value must be a RangeValue for the Between operator.");
        }

        var minExpr = GetConstantExpression(range.Min, left.Type);
        var maxExpr = GetConstantExpression(range.Max, left.Type);

        var greaterThanOrEqual = Expression.GreaterThanOrEqual(left, minExpr);
        var lessThanOrEqual = Expression.LessThanOrEqual(left, maxExpr);
        return Expression.AndAlso(greaterThanOrEqual, lessThanOrEqual);
    }
}
