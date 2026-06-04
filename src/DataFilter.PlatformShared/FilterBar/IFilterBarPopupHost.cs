using DataFilter.PlatformShared.ViewModels;

namespace DataFilter.PlatformShared.FilterBar;

/// <summary>
/// Host capability to open the column filter popup from the filter bar.
/// </summary>
public interface IFilterBarPopupHost
{
    /// <summary>
    /// Shows the filter popup for the given edit request.
    /// </summary>
    Task ShowColumnFilterAsync(FilterBarEditRequest request, CancellationToken cancellationToken = default);
}
