namespace DataFilter.Wpf.ViewModels;

/// <summary>
/// WPF type identity for the shared grid orchestrator (<see cref="DataFilter.PlatformShared.ViewModels.FilterableDataGridViewModel{T}"/>).
/// </summary>
public partial class FilterableDataGridViewModel<T> : DataFilter.PlatformShared.ViewModels.FilterableDataGridViewModel<T>, IFilterableDataGridViewModel<T>
{
}
