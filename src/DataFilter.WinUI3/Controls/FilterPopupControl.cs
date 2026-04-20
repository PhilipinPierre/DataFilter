using DataFilter.PlatformShared.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using DataFilter.Core.Enums;
using System.Collections.ObjectModel;
using DataFilter.Localization;

namespace DataFilter.WinUI3.Controls;

public sealed class FilterPopupControl : UserControl
{
    public ColumnFilterViewModel? ViewModel { get; private set; }
    private bool _isInitialized;

    private readonly StackPanel _sortPanel = new() { Spacing = 2, Margin = new Thickness(0, 0, 0, 8) };
    private readonly TextBox _searchBox = new() { Margin = new Thickness(0, 0, 0, 4) };
    private readonly CheckBox _addToExisting = new() { FontSize = 12 };
    private readonly Expander _advancedExpander = new() { HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0, 0, 0, 8) };
    private readonly TextBlock _advancedOperatorLabel = new() { FontSize = 10 };
    private readonly TextBlock _advancedValueLabel = new() { FontSize = 10 };
    private readonly TextBlock _advancedToLabel = new() { FontSize = 10, Margin = new Thickness(0, 4, 0, 0) };
    private readonly Button _okBtn = new() { HorizontalAlignment = HorizontalAlignment.Stretch };
    private readonly Button _clearBtn = new() { HorizontalAlignment = HorizontalAlignment.Stretch };
    private readonly ComboBox _accMode = new() { HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(20, 0, 0, 0), FontSize = 11 };
    private readonly ComboBox _opCombo = new() { HorizontalAlignment = HorizontalAlignment.Stretch };
    private readonly ObservableCollection<LocalizedItem> _localizedOperators = new();
    private readonly ObservableCollection<LocalizedItem> _localizedAccModes = new();

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
        _sortPanel.Children.Add(CreateButton(LocalizationManager.Instance["SortAscending"], "SortAscendingCommand"));
        _sortPanel.Children.Add(CreateButton(LocalizationManager.Instance["SortDescending"], "SortDescendingCommand"));
        _sortPanel.Children.Add(CreateButton(LocalizationManager.Instance["AddSubSortAscending"], "AddSubSortAscendingCommand"));
        _sortPanel.Children.Add(CreateButton(LocalizationManager.Instance["AddSubSortDescending"], "AddSubSortDescendingCommand"));
        _sortPanel.Children.Add(new MenuFlyoutSeparator());
        Grid.SetRow(_sortPanel, 0);
        root.Children.Add(_sortPanel);

        // 2. Search Section
        var searchPanel = new StackPanel { Spacing = 4, Margin = new Thickness(0, 0, 0, 8) };
        _searchBox.PlaceholderText = LocalizationManager.Instance["SearchPlaceholder"];
        _searchBox.SetBinding(TextBox.TextProperty, new Binding { Path = new PropertyPath("SearchText"), Mode = BindingMode.TwoWay });
        searchPanel.Children.Add(_searchBox);

        _addToExisting.Content = LocalizationManager.Instance["AddToFilter"];
        _addToExisting.SetBinding(CheckBox.IsCheckedProperty, new Binding { Path = new PropertyPath("AddToExistingFilter"), Mode = BindingMode.TwoWay });
        searchPanel.Children.Add(_addToExisting);

        _accMode.ItemsSource = _localizedAccModes;
        _accMode.DisplayMemberPath = nameof(LocalizedItem.Text);
        _accMode.SelectedValuePath = nameof(LocalizedItem.Value);
        _accMode.SetBinding(ComboBox.SelectedValueProperty, new Binding { Path = new PropertyPath("AccumulationMode"), Mode = BindingMode.TwoWay });
        _accMode.SetBinding(UIElement.VisibilityProperty, new Binding { Path = new PropertyPath("AddToExistingFilter"), Converter = new BoolToVisibilityConverter() });
        searchPanel.Children.Add(_accMode);

        Grid.SetRow(searchPanel, 1);
        root.Children.Add(searchPanel);

        // 3. Advanced Filter (Expander)
        _advancedExpander.Header = LocalizationManager.Instance["AdvancedFilter"];
        _advancedExpander.SetBinding(Expander.IsExpandedProperty, new Binding { Path = new PropertyPath("IsCustomFilterExpanded"), Mode = BindingMode.TwoWay });
        
        var advancedContent = new StackPanel { Spacing = 4, Padding = new Thickness(4) };
        _advancedOperatorLabel.Text = LocalizationManager.Instance["OperatorText"];
        _advancedOperatorLabel.Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"];
        advancedContent.Children.Add(_advancedOperatorLabel);

        _opCombo.ItemsSource = _localizedOperators;
        _opCombo.DisplayMemberPath = nameof(LocalizedItem.Text);
        _opCombo.SelectedValuePath = nameof(LocalizedItem.Value);
        _opCombo.SetBinding(ComboBox.SelectedValueProperty, new Binding { Path = new PropertyPath("SelectedCustomOperator"), Mode = BindingMode.TwoWay });
        advancedContent.Children.Add(_opCombo);

        _advancedValueLabel.Text = LocalizationManager.Instance["ValueText"];
        _advancedValueLabel.Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"];
        advancedContent.Children.Add(_advancedValueLabel);
        var val1 = new TextBox();
        val1.SetBinding(TextBox.TextProperty, new Binding { Path = new PropertyPath("CustomValue1"), Mode = BindingMode.TwoWay });
        advancedContent.Children.Add(val1);

        var val2Panel = new StackPanel();
        val2Panel.SetBinding(UIElement.VisibilityProperty, new Binding { Path = new PropertyPath("SelectedCustomOperator"), Converter = new OperatorToVisibilityConverter(), ConverterParameter = FilterOperator.Between });
        _advancedToLabel.Text = LocalizationManager.Instance["ToText"];
        _advancedToLabel.Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"];
        val2Panel.Children.Add(_advancedToLabel);
        var val2 = new TextBox();
        val2.SetBinding(TextBox.TextProperty, new Binding { Path = new PropertyPath("CustomValue2"), Mode = BindingMode.TwoWay });
        val2Panel.Children.Add(val2);
        advancedContent.Children.Add(val2Panel);

        _advancedExpander.Content = advancedContent;
        Grid.SetRow(_advancedExpander, 2);
        root.Children.Add(_advancedExpander);

        // 4. List Section (TreeView)
        var tree = new TreeView { Margin = new Thickness(0, 0, 0, 8) };
        tree.ItemTemplate = CreateTreeViewTemplate();
        tree.SetBinding(TreeView.ItemsSourceProperty, new Binding { Path = new PropertyPath("FilterValues") });
        Grid.SetRow(tree, 3);
        root.Children.Add(tree);

        // 5. Actions Section
        var actions = new Grid { ColumnDefinitions = { new ColumnDefinition(), new ColumnDefinition { Width = new GridLength(8) }, new ColumnDefinition() } };
        _okBtn.Content = LocalizationManager.Instance["Ok"];
        _okBtn.SetBinding(Button.CommandProperty, new Binding { Path = new PropertyPath("ApplyCommand") });
        Grid.SetColumn(_okBtn, 0);
        actions.Children.Add(_okBtn);

        _clearBtn.Content = LocalizationManager.Instance["Clear"];
        _clearBtn.SetBinding(Button.CommandProperty, new Binding { Path = new PropertyPath("ClearCommand") });
        Grid.SetColumn(_clearBtn, 2);
        actions.Children.Add(_clearBtn);

        Grid.SetRow(actions, 4);
        root.Children.Add(actions);

        Content = root;

        LocalizationManager.Instance.CultureChanged += (_, _) => ApplyLocalization();
        ApplyLocalization();
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

        BuildLocalizedLists();
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

    private void BuildLocalizedLists()
    {
        _localizedAccModes.Clear();
        _localizedAccModes.Add(new LocalizedItem(AccumulationMode.Union, LocalizationManager.Instance["ModeUnion"]));
        _localizedAccModes.Add(new LocalizedItem(AccumulationMode.Intersection, LocalizationManager.Instance["ModeIntersection"]));

        _localizedOperators.Clear();
        if (ViewModel == null) return;
        foreach (var op in ViewModel.AvailableOperators)
            _localizedOperators.Add(new LocalizedItem(op, LocalizationManager.Instance[$"FilterOperator_{op}"]));
    }

    private void ApplyLocalization()
    {
        _searchBox.PlaceholderText = LocalizationManager.Instance["SearchPlaceholder"];
        _addToExisting.Content = LocalizationManager.Instance["AddToFilter"];
        _advancedExpander.Header = LocalizationManager.Instance["AdvancedFilter"];
        _advancedOperatorLabel.Text = LocalizationManager.Instance["OperatorText"];
        _advancedValueLabel.Text = LocalizationManager.Instance["ValueText"];
        _advancedToLabel.Text = LocalizationManager.Instance["ToText"];
        _okBtn.Content = LocalizationManager.Instance["Ok"];
        _clearBtn.Content = LocalizationManager.Instance["Clear"];

        // Rebuild displayed enum lists with localized labels
        BuildLocalizedLists();

        // Sort button texts
        if (_sortPanel.Children.Count >= 4)
        {
            if (_sortPanel.Children[0] is Button b0) b0.Content = LocalizationManager.Instance["SortAscending"];
            if (_sortPanel.Children[1] is Button b1) b1.Content = LocalizationManager.Instance["SortDescending"];
            if (_sortPanel.Children[2] is Button b2) b2.Content = LocalizationManager.Instance["AddSubSortAscending"];
            if (_sortPanel.Children[3] is Button b3) b3.Content = LocalizationManager.Instance["AddSubSortDescending"];
        }
    }

    private sealed class LocalizedItem
    {
        public LocalizedItem(object value, string text)
        {
            Value = value;
            Text = text;
        }

        public object Value { get; }
        public string Text { get; }
        public override string ToString() => Text;
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
