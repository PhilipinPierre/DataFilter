using DataFilter.Core.Abstractions;
using DataFilter.Core.Engine;
using DataFilter.Core.Enums;
using DataFilter.Core.Models;

namespace DataFilter.Filtering.ExcelLike.Models;

/// <summary>
/// A specialized filter descriptor that generates its logic based on an Excel-like filter state.
/// This descriptor acts as a group to handle both manual selection and contextual filters.
/// </summary>
public class ExcelFilterDescriptor : IFilterGroup
{
    private class SimpleDescriptor : IFilterDescriptor
    {
        public string PropertyName { get; set; } = string.Empty;
        public FilterOperator Operator { get; set; }
        public object? Value { get; set; }
        public bool IsMatch(object item) => false; // Not used when part of a group
    }

    /// <summary>
    /// Gets the current Excel-like filter state used by this descriptor.
    /// </summary>
    public ExcelFilterState State { get; }

    /// <inheritdoc />
    public string PropertyName { get; }

    /// <inheritdoc />
    public FilterOperator Operator => FilterOperator.In; // Default for list selection

    /// <inheritdoc />
    public object? Value => State.SelectedValues.ToList();

    /// <inheritdoc />
    public LogicalOperator LogicalOperator => LogicalOperator.And;

    /// <inheritdoc />
    public IReadOnlyList<IFilterDescriptor> Descriptors
    {
        get
        {
            var list = new List<IFilterDescriptor>();

            // 1. Contextual Filter (takes precedence or is combined)
            if (State.CustomOperator != null)
            {
                object? val = State.CustomValue1;
                if (State.CustomOperator == FilterOperator.Between)
                {
                    val = new RangeValue(State.CustomValue1, State.CustomValue2);
                }

                list.Add(new SimpleDescriptor
                {
                    PropertyName = this.PropertyName,
                    Operator = State.CustomOperator.Value,
                    Value = val
                });

                foreach (ExcelFilterAdditionalCriterion extra in State.AdditionalCustomCriteria)
                {
                    object? extraVal = extra.Value1;
                    if (extra.Operator == FilterOperator.Between)
                    {
                        extraVal = new RangeValue(extra.Value1, extra.Value2);
                    }

                    list.Add(new SimpleDescriptor
                    {
                        PropertyName = this.PropertyName,
                        Operator = extra.Operator,
                        Value = extraVal
                    });
                }
            }

            // 2. Manual Selection (if not Select All)
            // In Excel, if you use a custom filter, it usually resets the list selection.
            // But we can support combining them if needed. 
            // For now, if CustomOperator is None, use manual selection.
            else if (!State.SelectAll || State.DistinctValues.Count != State.SelectedValues.Count)
            {
                list.Add(new SimpleDescriptor
                {
                    PropertyName = this.PropertyName,
                    Operator = FilterOperator.In,
                    Value = State.SelectedValues.ToList()
                });
            }

            return list;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExcelFilterDescriptor"/> class.
    /// </summary>
    /// <param name="propertyName">The name of the property being filtered.</param>
    /// <param name="state">The Excel-like filter state.</param>
    public ExcelFilterDescriptor(string propertyName, ExcelFilterState state)
    {
        PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        State = state ?? throw new ArgumentNullException(nameof(state));
    }

    /// <inheritdoc />
    public bool IsMatch(object item)
    {
        if (item == null) return false;

        var descriptors = Descriptors;
        if (descriptors.Count == 0) return true;

        var func = FilterExpressionBuilder.BuildFunc(item.GetType(), this);
        return func(item);
    }
}
