using System.Collections;
using DataFilter.Core.Enums;
using DataFilter.Core.Models;
using DataFilter.Filtering.ExcelLike.Models;

namespace DataFilter.Filtering.ExcelLike.Services;

/// <summary>
/// Maps generic <see cref="FilterDescriptor"/> rules to <see cref="ExcelFilterDescriptor"/> for Excel-style column UI.
/// </summary>
public static class ExcelFilterStateFromFilterDescriptor
{
    /// <summary>
    /// Builds an Excel column descriptor from a single compiled criterion.
    /// </summary>
    public static ExcelFilterDescriptor ToExcelFilterDescriptor(FilterDescriptor fd)
    {
        var state = new ExcelFilterState();
        if (fd.Operator == FilterOperator.In)
            ApplyInRuleToState(state, fd);
        else
            ApplyCustomRuleToState(state, fd);
        return new ExcelFilterDescriptor(fd.PropertyName, state);
    }

    /// <summary>
    /// When all children are <see cref="FilterDescriptor"/> for the same property and combined with AND,
    /// merges them into one <see cref="ExcelFilterDescriptor"/> (custom + additional criteria, or In list).
    /// </summary>
    public static bool TryMergeAndFiltersToExcelDescriptor(FilterGroup group, out ExcelFilterDescriptor? excel)
    {
        excel = null;
        if (group.LogicalOperator != LogicalOperator.And)
            return false;

        var directCriteria = new List<FilterDescriptor>();
        FilterGroup? orGroup = null;
        foreach (var d in group.Descriptors)
        {
            switch (d)
            {
                case FilterDescriptor fd:
                    directCriteria.Add(fd);
                    break;
                case FilterGroup fg when fg.LogicalOperator == LogicalOperator.Or:
                    orGroup = fg;
                    break;
                default:
                    return false;
            }
        }

        if (directCriteria.Count == 0 && orGroup == null)
            return false;

        var allPropertyNames = new List<string>();
        allPropertyNames.AddRange(directCriteria.Select(c => c.PropertyName));
        if (orGroup != null)
            allPropertyNames.AddRange(orGroup.Descriptors.OfType<FilterDescriptor>().Select(c => c.PropertyName));

        if (allPropertyNames.Distinct(StringComparer.OrdinalIgnoreCase).Count() != 1)
            return false;

        string propertyName = allPropertyNames[0];
        var state = new ExcelFilterState();
        var customs = directCriteria.Where(d => d.Operator != FilterOperator.In).ToList();
        var inRule = directCriteria.FirstOrDefault(d => d.Operator == FilterOperator.In);

        if (customs.Count > 0)
        {
            ApplyCustomRuleToState(state, customs[0]);
            for (int i = 1; i < customs.Count; i++)
            {
                var fd = customs[i];
                if (fd.Operator == FilterOperator.Between && fd.Value is RangeValue rv)
                {
                    state.AdditionalCustomCriteria.Add(new ExcelFilterAdditionalCriterion
                    {
                        Operator = fd.Operator,
                        Value1 = rv.Min,
                        Value2 = rv.Max
                    });
                }
                else
                {
                    state.AdditionalCustomCriteria.Add(new ExcelFilterAdditionalCriterion
                    {
                        Operator = fd.Operator,
                        Value1 = fd.Value,
                        Value2 = null
                    });
                }
            }
        }

        if (orGroup != null)
        {
            foreach (var child in orGroup.Descriptors.OfType<FilterDescriptor>())
            {
                if (child.Operator == FilterOperator.StartsWith && child.Value is string p && !string.IsNullOrEmpty(p))
                {
                    state.OrSearchPatterns.Add(p);
                }
                else if (child.Operator == FilterOperator.In)
                {
                    if (child.Value is IEnumerable enumerable && child.Value is not string)
                    {
                        foreach (var v in enumerable)
                            state.OrSelectedValues.Add(v);
                    }
                }
                else
                {
                    return false;
                }
            }

            state.SelectAll = true;
            state.SelectedValues.Clear();
        }
        else if (inRule != null && customs.Count == 0)
        {
            ApplyInRuleToState(state, inRule);
        }
        else if (customs.Count == 0)
        {
            return false;
        }

        excel = new ExcelFilterDescriptor(propertyName, state);
        return true;
    }

    public static void ApplyCustomRuleToState(ExcelFilterState state, FilterDescriptor fd)
    {
        state.CustomOperator = fd.Operator;
        state.SelectedValues.Clear();
        state.SelectAll = true;

        if (fd.Operator == FilterOperator.Between && fd.Value is RangeValue rv)
        {
            state.CustomValue1 = rv.Min;
            state.CustomValue2 = rv.Max;
        }
        else
        {
            state.CustomValue1 = fd.Value;
            state.CustomValue2 = null;
        }
    }

    public static void ApplyInRuleToState(ExcelFilterState state, FilterDescriptor fd)
    {
        state.CustomOperator = null;
        state.CustomValue1 = null;
        state.CustomValue2 = null;
        state.SelectedValues.Clear();

        if (fd.Value is IEnumerable enumerable && fd.Value is not string)
        {
            foreach (var v in enumerable)
                state.SelectedValues.Add(v);
        }

        state.SelectAll = false;
    }
}
