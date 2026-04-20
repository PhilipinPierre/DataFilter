using DataFilter.PlatformShared.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DataFilter.WinUI3.Controls;

public sealed partial class FilterableDataGrid : ListView
{
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            nameof(ViewModel),
            typeof(IFilterableDataGridViewModel),
            typeof(FilterableDataGrid),
            new PropertyMetadata(null));

    public IFilterableDataGridViewModel? ViewModel
    {
        get => (IFilterableDataGridViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public FilterableDataGrid()
    {
        var headerScroll = new ScrollViewer { HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, VerticalScrollBarVisibility = ScrollBarVisibility.Disabled };
        var headerGrid = new Grid { Padding = new Thickness(10, 0, 10, 10) };
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        
        headerGrid.Children.Add(CreateHeaderButton("Id", 0));
        headerGrid.Children.Add(CreateHeaderButton("Name", 1));
        headerGrid.Children.Add(CreateHeaderButton("Department", 2));
        headerGrid.Children.Add(CreateHeaderButton("Country", 3));

        headerScroll.Content = headerGrid;
        Header = headerScroll;
    }

    private Button CreateHeaderButton(string text, int col)
    {
        var btn = new Button { 
            Content = text + " 🔍", 
            HorizontalAlignment = HorizontalAlignment.Left,
            Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent),
            Padding = new Thickness(5)
        };
        Grid.SetColumn(btn, col);
        btn.Click += (s, e) => {
            if (ViewModel != null) 
            {
                var popup = DataFilter.WinUI3.Behaviors.FilterHeaderBehavior.CreatePopup(ViewModel, text);
                var flyout = new Flyout { Content = popup };
                if (popup.ViewModel != null)
                {
                    popup.ViewModel.OnApply += (_, __) => flyout.Hide();
                    popup.ViewModel.OnClear += (_, __) => flyout.Hide();
                }
                flyout.ShowAt(btn);
            }
        };
        return btn;
    }
}
