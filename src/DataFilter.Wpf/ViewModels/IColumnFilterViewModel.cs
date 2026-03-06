using DataFilter.Filtering.ExcelLike.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace DataFilter.Wpf.ViewModels;

/// <summary>
/// Defines the contract for the column filter popup view model.
/// </summary>
public interface IColumnFilterViewModel : INotifyPropertyChanged
{
    /// <summary>
    /// Gets or sets the search text in the filter popup.
    /// </summary>
    string SearchText { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether "Select All" is checked.
    /// </summary>
    bool? SelectAll { get; set; }

    /// <summary>
    /// Gets the collection of values to display in the UI.
    /// </summary>
    ObservableCollection<FilterValueItem> FilterValues { get; }

    /// <summary>
    /// Gets the underlying filter state.
    /// </summary>
    ExcelFilterState FilterState { get; }

    /// <summary>
    /// Gets the command executed to apply the filter.
    /// </summary>
    ICommand ApplyCommand { get; }

    /// <summary>
    /// Gets the command executed to clear the filter.
    /// </summary>
    ICommand ClearCommand { get; }

    /// <summary>
    /// Gets the command to sort data in ascending order.
    /// </summary>
    ICommand SortAscendingCommand { get; }

    /// <summary>
    /// Gets the command to sort data in descending order.
    /// </summary>
    ICommand SortDescendingCommand { get; }

    /// <summary>
    /// Initializes the view model with distinct values.
    /// </summary>
    void Initialize(System.Collections.Generic.IEnumerable<object> distinctValues);

    /// <summary>
    /// Indicates if data is currently loading asynchronously.
    /// </summary>
    bool IsLoading { get; }
}
