using DataFilter.Core.Abstractions;
using DataFilter.Core.Enums;
using DataFilter.Core.Models;
using DataFilter.Core.Services;

namespace DataFilter.Filtering.ExcelLike.Services;

/// <summary>
/// Converts compiled pipeline descriptors to <see cref="ExcelFilterDescriptor"/> so column filter popups and
/// grid filter state stay aligned with the filter context after JSON presets or pipeline apply.
/// </summary>
public static class FilterDescriptorToExcelConverter
{
    /// <summary>
    /// Converts each top-level <see cref="FilterDescriptor"/> to an <see cref="ExcelFilterDescriptor"/>.
    /// AND groups over a single property are merged; the root OR wrapper from <see cref="FilterPipelineCompiler"/> is left unchanged.
    /// </summary>
    public static IReadOnlyList<IFilterDescriptor> ConvertCompiledPipeline(IReadOnlyList<IFilterDescriptor> compiled)
    {
        var result = new List<IFilterDescriptor>();
        foreach (var d in compiled)
            ConvertTopLevel(d, result);
        return result;
    }

    private static void ConvertTopLevel(IFilterDescriptor d, List<IFilterDescriptor> result)
    {
        switch (d)
        {
            case FilterDescriptor fd:
                result.Add(ExcelFilterStateFromFilterDescriptor.ToExcelFilterDescriptor(fd));
                break;
            case FilterGroup fg:
                if (fg.LogicalOperator == LogicalOperator.Or
                    && string.Equals(fg.PropertyName, FilterPipelineCompiler.PipelineRootGroupPropertyName, StringComparison.Ordinal))
                {
                    result.Add(fg);
                }
                else if (fg.LogicalOperator == LogicalOperator.And)
                {
                    if (ExcelFilterStateFromFilterDescriptor.TryMergeAndFiltersToExcelDescriptor(fg, out var excel) && excel != null)
                        result.Add(excel);
                    else
                        result.Add(fg);
                }
                else
                {
                    result.Add(fg);
                }
                break;
            default:
                result.Add(d);
                break;
        }
    }
}
