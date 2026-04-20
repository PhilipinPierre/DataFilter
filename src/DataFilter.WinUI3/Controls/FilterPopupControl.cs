using DataFilter.PlatformShared.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using DataFilter.Core.Enums;
using System.Collections.ObjectModel;

namespace DataFilter.WinUI3.Controls;

public sealed class FilterPopupControl : UserControl
{
    public ColumnFilterViewModel? ViewModel { get; private set; }
    private bool _isInitialized;

    public FilterPopupControl()
    {
        Background = (Brush)Application.Current.Resources["ApplicationPageBackgroundThemeBrush"];
        BorderBrush = (Brush)Application.Current.Resources["SurfaceStrokeColorDefaultBrush"];
        BorderThickness = new Thickness(1);
        Padding = new Thickness(8);
        Width = 250;
        MinHeight = 300;
        MaxHeight = 500;

        var root = new Grid();
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Sort
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Search
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Advanced
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // List
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Buttons

        // 1. Sorting Section
        var sortPanel = new StackPanel { Spacing = 2, Margin = new Thickness(0, 0, 0, 8) };
        sortPanel.Children.Add(CreateButton("Sort A to Z", "SortAscendingCommand"));
        sortPanel.Children.Add(CreateButton("Sort Z to A", "SortDescendingCommand"));
        sortPanel.Children.Add(CreateButton("Add Sort A to Z", "AddSubSortAscendingCommand"));
        sortPanel.Children.Add(CreateButton("Add Sort Z to A", "AddSubSortDescendingCommand"));
        sortPanel.Children.Add(new MenuFlyoutSeparator());
        Grid.SetRow(sortPanel, 0);
        root.Children.Add(sortPanel);

        // 2. Search Section
        var searchPanel = new StackPanel { Spacing = 4, Margin = new Thickness(0, 0, 0, 8) };
        var searchBox = new TextBox { PlaceholderText = "Search...", Margin = new Thickness(0, 0, 0, 4) };
        searchBox.SetBinding(TextBox.TextProperty, new Binding { Path = new PropertyPath("SearchText"), Mode = BindingMode.TwoWay });
        searchPanel.Children.Add(searchBox);

        var addToExisting = new CheckBox { Content = "Add selection to filter", FontSize = 12 };
        addToExisting.SetBinding(CheckBox.IsCheckedProperty, new Binding { Path = new PropertyPath("AddToExistingFilter"), Mode = BindingMode.TwoWay });
        searchPanel.Children.Add(addToExisting);

        var accMode = new ComboBox { HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(20, 0, 0, 0), FontSize = 11 };
        accMode.ItemsSource = Enum.GetValues<AccumulationMode>();
        accMode.SetBinding(ComboBox.SelectedItemProperty, new Binding { Path = new PropertyPath("AccumulationMode"), Mode = BindingMode.TwoWay });
        accMode.SetBinding(UIElement.VisibilityProperty, new Binding { Path = new PropertyPath("AddToExistingFilter"), Converter = new BoolToVisibilityConverter() });
        searchPanel.Children.Add(accMode);

        Grid.SetRow(searchPanel, 1);
        root.Children.Add(searchPanel);

        // 3. Advanced Filter (Expander)
        var advancedExpander = new Expander { Header = "Advanced Filter", HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0, 0, 0, 8) };
        advancedExpander.SetBinding(Expander.IsExpandedProperty, new Binding { Path = new PropertyPath("IsCustomFilterExpanded"), Mode = BindingMode.TwoWay });
        
        var advancedContent = new StackPanel { Spacing = 4, Padding = new Thickness(4) };
        advancedContent.Children.Add(new TextBlock { Text = "Operator", FontSize = 10, Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"] });
        var opCombo = new ComboBox { HorizontalAlignment = HorizontalAlignment.Stretch };
        opCombo.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Path = new PropertyPath("AvailableOperators") });
        opCombo.SetBinding(ComboBox.SelectedItemProperty, new Binding { Path = new PropertyPath("SelectedCustomOperator"), Mode = BindingMode.TwoWay });
        advancedContent.Children.Add(opCombo);

        advancedContent.Children.Add(new TextBlock { Text = "Value", FontSize = 10, Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"] });
        var val1 = new TextBox();
        val1.SetBinding(TextBox.TextProperty, new Binding { Path = new PropertyPath("CustomValue1"), Mode = BindingMode.TwoWay });
        advancedContent.Children.Add(val1);

        var val2Panel = new StackPanel();
        val2Panel.SetBinding(UIElement.VisibilityProperty, new Binding { Path = new PropertyPath("SelectedCustomOperator"), Converter = new OperatorToVisibilityConverter(), ConverterParameter = FilterOperator.Between });
        val2Panel.Children.Add(new TextBlock { Text = "And", FontSize = 10, Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"], Margin = new Thickness(0, 4, 0, 0) });
        var val2 = new TextBox();
        val2.SetBinding(TextBox.TextProperty, new Binding { Path = new PropertyPath("CustomValue2"), Mode = BindingMode.TwoWay });
        val2Panel.Children.Add(val2);
        advancedContent.Children.Add(val2Panel);

        advancedExpander.Content = advancedContent;
        Grid.SetRow(advancedExpander, 2);
        root.Children.Add(advancedExpander);

        // 4. List Section (TreeView)
        var tree = new TreeView { Margin = new Thickness(0, 0, 0, 8) };
        tree.ItemTemplate = CreateTreeViewTemplate();
        tree.SetBinding(TreeView.ItemsSourceProperty, new Binding { Path = new PropertyPath("FilterValues") });
        Grid.SetRow(tree, 3);
        root.Children.Add(tree);

        // 5. Actions Section
        var actions = new Grid { ColumnDefinitions = { new ColumnDefinition(), new ColumnDefinition { Width = new GridLength(8) }, new ColumnDefinition() } };
        var okBtn = new Button { Content = "OK", HorizontalAlignment = HorizontalAlignment.Stretch };
        okBtn.SetBinding(Button.CommandProperty, new Binding { Path = new PropertyPath("ApplyCommand") });
        Grid.SetColumn(okBtn, 0);
        actions.Children.Add(okBtn);

        var clearBtn = new Button { Content = "Clear", HorizontalAlignment = HorizontalAlignment.Stretch };
        clearBtn.SetBinding(Button.CommandProperty, new Binding { Path = new PropertyPath("ClearCommand") });
        Grid.SetColumn(clearBtn, 2);
        actions.Children.Add(clearBtn);

        Grid.SetRow(actions, 4);
        root.Children.Add(actions);

        Content = root;
    }

    private Button CreateButton(string text, string commandPath)
    {
        var btn = new Button { Content = text, HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = HorizontalAlignment.Left, Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent), BorderThickness = new Thickness(0) };
        btn.SetBinding(Button.CommandProperty, new Binding { Path = new PropertyPath(commandPath) });
        return btn;
    }

    private DataTemplate CreateTreeViewTemplate()
    {
        var xaml = @"
            <DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                          xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
                <CheckBox Content=""{Binding DisplayText}"" 
                          IsChecked=""{Binding IsSelected, Mode=TwoWay}"" 
                          IsThreeState=""True"" />
            </DataTemplate>";
        return (DataTemplate)Microsoft.UI.Xaml.Markup.XamlReader.Load(xaml);
    }

    public void Bind(ColumnFilterViewModel vm)
    {
        ViewModel = vm;
        DataContext = vm;

        Loaded -= FilterPopupControl_Loaded;
        Loaded += FilterPopupControl_Loaded;
    }

    private async void FilterPopupControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (_isInitialized) return;
        _isInitialized = true;

        var vm = ViewModel;
        if (vm == null) return;

        // Ensure distinct values are loaded on first open (empty search).
        if (vm.SearchCommand.CanExecute(vm.SearchText))
        {
            await vm.SearchCommand.ExecuteAsync(vm.SearchText ?? string.Empty);
        }
    }
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) => (bool)value ? Visibility.Visible : Visibility.Collapsed;
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class OperatorToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is FilterOperator op && parameter is FilterOperator target) return op == target ? Visibility.Visible : Visibility.Collapsed;
        return Visibility.Collapsed;
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
