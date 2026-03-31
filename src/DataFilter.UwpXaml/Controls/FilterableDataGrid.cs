using System.ComponentModel;
using DataFilter.PlatformShared.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DataFilter.UwpXaml.Controls;

public sealed class FilterableDataGrid : ListView
{
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            nameof(ViewModel),
            typeof(IFilterableDataGridViewModel),
            typeof(FilterableDataGrid),
            new PropertyMetadata(null, OnViewModelChanged));
    
    public FilterableDataGrid()
    {
        this.DefaultStyleKey = typeof(ListView);
    }

    public IFilterableDataGridViewModel? ViewModel
    {
        get => (IFilterableDataGridViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FilterableDataGrid control)
        {
            if (e.OldValue is IFilterableDataGridViewModel oldVm)
            {
                oldVm.PropertyChanged -= control.OnViewModelPropertyChanged;
            }

            if (e.NewValue is IFilterableDataGridViewModel newVm)
            {
                newVm.PropertyChanged += control.OnViewModelPropertyChanged;
                control.UpdateItemsSource();
            }
            else
            {
                control.ItemsSource = null;
            }
        }
    }

    private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IFilterableDataGridViewModel.FilteredItems))
        {
            UpdateItemsSource();
        }
    }

    private void UpdateItemsSource()
    {
        if (ViewModel == null) return;

        // Ensure we update on the UI thread to avoid exceptions
        var _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
        {
            this.ItemsSource = ViewModel.FilteredItems;
        });
    }
}
