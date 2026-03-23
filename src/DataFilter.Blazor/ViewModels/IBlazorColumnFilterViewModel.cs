using DataFilter.Filtering.ExcelLike.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace DataFilter.Blazor.ViewModels;

/// <summary>
/// Defines the contract for the Blazor column filter popup view model.
/// </summary>
public interface IBlazorColumnFilterViewModel : INotifyPropertyChanged
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
    /// Gets the command to add a sub-sort in ascending order.
    /// </summary>
    ICommand AddSubSortAscendingCommand { get; }

    /// <summary>
    /// Gets the command to add a sub-sort in descending order.
    /// </summary>
    ICommand AddSubSortDescendingCommand { get; }

    /// <summary>
    /// Indicates whether the filter is actively filtering data.
    /// </summary>
    bool IsFilterActive { get; }

    /// <summary>
    /// Gets or sets a value indicating whether to add the current selection to the existing filter.
    /// </summary>
    bool AddToExistingFilter { get; set; }

    /// <summary>
    /// Gets or sets the mode used to merge new criteria.
    /// </summary>
    DataFilter.Core.Enums.AccumulationMode AccumulationMode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the custom filter section is expanded.
    /// </summary>
    bool IsCustomFilterExpanded { get; set; }

    /// <summary>
    /// Gets or sets the currently selected custom operator.
    /// </summary>
    DataFilter.Core.Enums.FilterOperator? SelectedCustomOperator { get; set; }

    /// <summary>
    /// Gets the list of available custom operators.
    /// </summary>
    System.Collections.ObjectModel.ObservableCollection<DataFilter.Core.Enums.FilterOperator> AvailableOperators { get; }

    /// <summary>
    /// Gets or sets the first value for custom filtering.
    /// </summary>
    string CustomValue1 { get; set; }

    /// <summary>
    /// Gets or sets the second value for custom filtering.
    /// </summary>
    string CustomValue2 { get; set; }

    /// <summary>
    /// Initializes the view model with distinct values asynchronously.
    /// </summary>
    System.Threading.Tasks.Task InitializeAsync(System.Collections.Generic.IEnumerable<object> distinctValues);

    /// <summary>
    /// Loads an existing filter state into this view model asynchronously.
    /// </summary>
    System.Threading.Tasks.Task LoadStateAsync(ExcelFilterState state);

    /// <summary>
    /// Indicates if data is currently loading asynchronously.
    /// </summary>
    bool IsLoading { get; }
}
