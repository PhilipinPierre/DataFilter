using DataFilter.Wpf.Behaviors;
using DataFilter.Wpf.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DataFilter.Wpf.Controls;

/// <summary>
/// A GridView that supports Excel-like column filtering on ListView columns.
/// Automatically applies <see cref="FilterableColumnHeaderBehavior"/> to each column header.
/// </summary>
public class FilterableGridView : GridView
{
    public FilterableGridView()
    {
        var style = new Style(typeof(GridViewColumnHeader));
        style.Setters.Add(new Setter(FilterableColumnHeaderBehavior.IsFilterableProperty, true));

        ColumnHeaderContainerStyle = style;
    }

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(IFilterableDataGridViewModel), typeof(FilterableGridView), new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the filtering orchestrator.
    /// </summary>
    public IFilterableDataGridViewModel? ViewModel
    {
        get => (IFilterableDataGridViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }
}
