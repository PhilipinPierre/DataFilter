namespace DataFilter.Wpf.ViewModels;

/// <summary>
/// WPF contract surface; implementation is shared via <see cref="DataFilter.PlatformShared.ViewModels.IFilterableDataGridViewModel"/>.
/// </summary>
public interface IFilterableDataGridViewModel : DataFilter.PlatformShared.ViewModels.IFilterableDataGridViewModel
{
}

/// <summary>
/// WPF generic contract surface; implementation is shared via <see cref="DataFilter.PlatformShared.ViewModels.IFilterableDataGridViewModel{T}"/>.
/// </summary>
public interface IFilterableDataGridViewModel<T> : DataFilter.PlatformShared.ViewModels.IFilterableDataGridViewModel<T>, IFilterableDataGridViewModel
{
}
