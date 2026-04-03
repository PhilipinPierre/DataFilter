using DataFilter.Core.Engine;
using DataFilter.Filtering.ExcelLike.Models;
using DataFilter.Wpf.Resources;

namespace DataFilter.Wpf.ViewModels;

/// <summary>
/// Supplies localized UI strings (e.g. blanks label); behavior is in <see cref="PlatformShared.ViewModels.ColumnFilterViewModel"/>.
/// </summary>
public partial class ColumnFilterViewModel : PlatformShared.ViewModels.ColumnFilterViewModel, IColumnFilterViewModel
{
    public ColumnFilterViewModel(
        Func<string, System.Threading.Tasks.Task<IEnumerable<object>>> distinctValuesProvider,
        Action<ExcelFilterState> onApply,
        Action onClear,
        Action<bool>? onSort = null,
        Action<bool>? onAddSubSort = null,
        Type? propertyType = null,
        IFilterEvaluator? filterEvaluator = null)
        : base(distinctValuesProvider, onApply, onClear, onSort, onAddSubSort, propertyType, filterEvaluator, FilterResources.Blanks)
    {
    }

    /// <summary>For XAML design-time (<c>d:DesignInstance</c>).</summary>
    public ColumnFilterViewModel()
        : base()
    {
    }
}
