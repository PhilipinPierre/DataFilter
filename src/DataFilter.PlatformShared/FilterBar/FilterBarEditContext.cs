using DataFilter.Filtering.ExcelLike.Models;

namespace DataFilter.PlatformShared.FilterBar;

/// <summary>
/// When set on <see cref="ViewModels.ColumnFilterViewModel"/>, Apply/Clear update a pipeline node instead of the whole column.
/// </summary>
public sealed class FilterBarEditContext
{
    public required string NodeId { get; init; }

    public bool RemoveNodeOnClear { get; init; } = true;

    public bool IsNew { get; init; }

    /// <summary>
    /// Applies popup state to the pipeline criterion and refreshes the grid.
    /// </summary>
    public Func<ExcelFilterState, Task>? ApplyToPipelineAsync { get; init; }

    /// <summary>
    /// Removes the criterion node from the pipeline.
    /// </summary>
    public Func<Task>? RemoveFromPipelineAsync { get; init; }
}
