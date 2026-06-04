using DataFilter.Core.Abstractions;
using DataFilter.Core.Enums;
using DataFilter.Core.Models;
using DataFilter.Core.Pipeline;
using DataFilter.Filtering.ExcelLike.Models;

namespace DataFilter.PlatformShared.FilterBar;

/// <summary>
/// Maps between <see cref="ExcelFilterState"/> and <see cref="CriterionPipelineNode"/>.
/// </summary>
public static class FilterBarCriterionMapper
{
    /// <summary>
    /// Loads pipeline criterion fields into an <see cref="ExcelFilterState"/> via the Excel descriptor rules.
    /// </summary>
    public static ExcelFilterState StateFromCriterion(CriterionPipelineNode node)
    {
        if (string.IsNullOrEmpty(node.Operator) || string.IsNullOrEmpty(node.PropertyName))
            return new ExcelFilterState();

        var op = (FilterOperator)Enum.Parse(typeof(FilterOperator), node.Operator, ignoreCase: true);
        var fd = new FilterDescriptor(node.PropertyName, op, node.Value);
        return Filtering.ExcelLike.Services.ExcelFilterStateFromFilterDescriptor.ToExcelFilterDescriptor(fd).State;
    }

    /// <summary>
    /// Picks popup state for bar edit: node snapshot when it carries a rule, otherwise column context state.
    /// </summary>
    public static ExcelFilterState ResolveStateForEdit(CriterionPipelineNode node, ExcelFilterState? columnState)
    {
        ExcelFilterState fromNode = StateFromCriterion(node);
        if (columnState != null && !CriterionHasStoredRule(node))
            return columnState;
        return fromNode;
    }

    /// <summary>
    /// True when the pipeline node carries a real operator/value (not an empty placeholder).
    /// </summary>
    public static bool CriterionHasStoredRule(CriterionPipelineNode node) =>
        !string.IsNullOrEmpty(node.Operator)
        && !(string.Equals(node.Operator, nameof(FilterOperator.Equals), StringComparison.OrdinalIgnoreCase) && node.Value == null);

    /// <summary>
    /// Writes popup state onto a single criterion node (primary compiled rule).
    /// </summary>
    public static void ApplyStateToCriterion(CriterionPipelineNode node, string propertyName, ExcelFilterState state)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            throw new ArgumentException("PropertyName is required.", nameof(propertyName));

        node.PropertyName = propertyName;
        if (TryGetPrimaryRule(propertyName, state, out string? op, out object? value))
        {
            node.Operator = op!;
            node.Value = value;
            return;
        }

        if (state.CustomOperator != null)
        {
            node.Operator = state.CustomOperator.Value.ToString();
            node.Value = state.CustomOperator == FilterOperator.Between
                ? new RangeValue(state.CustomValue1, state.CustomValue2)
                : state.CustomValue1;
            return;
        }

        if (!state.SelectAll && state.SelectedValues.Count > 0)
        {
            node.Operator = nameof(FilterOperator.In);
            node.Value = state.SelectedValues.ToList();
            return;
        }

        node.Operator = nameof(FilterOperator.Equals);
        node.Value = null;
    }

    private static bool TryGetPrimaryRule(string propertyName, ExcelFilterState state, out string? op, out object? value)
    {
        op = null;
        value = null;
        var excel = new ExcelFilterDescriptor(propertyName, state);
        IFilterDescriptor? primary = GetPrimaryCompiledRule(excel);
        if (primary == null)
            return false;

        op = primary.Operator.ToString();
        value = primary.Value;
        return true;
    }

    private static IFilterDescriptor? GetPrimaryCompiledRule(IFilterGroup group)
    {
        foreach (IFilterDescriptor d in group.Descriptors)
        {
            switch (d)
            {
                case FilterGroup fg:
                    IFilterDescriptor? nested = GetPrimaryCompiledRule(fg);
                    if (nested != null)
                        return nested;
                    break;
                default:
                    return d;
            }
        }

        return null;
    }
}
