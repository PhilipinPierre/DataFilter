using DataFilter.Core.Enums;

namespace DataFilter.Core.Engine;

/// <summary>
/// Defines the contract for evaluating filter operators against values.
/// </summary>
public interface IFilterEvaluator
{
    /// <summary>
    /// Evaluates a filter operator against a value.
    /// </summary>
    /// <param name="itemValue">The value to test.</param>
    /// <param name="op">The operator to apply.</param>
    /// <param name="v1">The first comparison value.</param>
    /// <param name="v2">The second comparison value (used for Between).</param>
    /// <returns>True if the condition is met; otherwise, false.</returns>
    bool EvaluateOperator(object? itemValue, FilterOperator op, object? v1, object? v2 = null);
}
