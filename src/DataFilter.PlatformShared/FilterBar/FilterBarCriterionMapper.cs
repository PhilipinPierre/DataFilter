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
    /// Writes popup state onto a single criterion node (primary rule only).
    /// </summary>
    public static void ApplyStateToCriterion(CriterionPipelineNode node, string propertyName, ExcelFilterState state)
    {
        node.PropertyName = propertyName;
        if (!TryGetPrimaryRule(propertyName, state, out string? op, out object? value))
        {
            node.Operator = nameof(FilterOperator.Equals);
            node.Value = null;
            return;
        }

        node.Operator = op!;
        node.Value = value;
    }

    private static bool TryGetPrimaryRule(string propertyName, ExcelFilterState state, out string? op, out object? value)
    {
        op = null;
        value = null;
        var excel = new ExcelFilterDescriptor(propertyName, state);
        foreach (var d in excel.Descriptors)
        {
            if (d is FilterDescriptor fd)
            {
                op = fd.Operator.ToString();
                value = fd.Value;
                return true;
            }

            if (d is FilterGroup fg)
            {
                var first = fg.Descriptors.OfType<FilterDescriptor>().FirstOrDefault();
                if (first != null)
                {
                    op = first.Operator.ToString();
                    value = first.Value;
                    return true;
                }
            }
        }

        return false;
    }
}
