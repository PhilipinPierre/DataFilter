using DataFilter.PlatformShared.ViewModels;
using DataFilter.Core.Enums;
using System.Collections.ObjectModel;
using DataFilter.Filtering.ExcelLike.Models;

namespace DataFilter.Maui.Controls;

public sealed class FilterPopupView : ContentView
{
    public ColumnFilterViewModel? ViewModel { get; private set; }

    private readonly VerticalStackLayout _itemsLayout;
    private readonly VerticalStackLayout _advancedLayout;

    public FilterPopupView()
    {
        Padding = 10;
        BackgroundColor = Colors.White;
        WidthRequest = 280;
        MinimumHeightRequest = 350;

        _itemsLayout = new VerticalStackLayout { Spacing = 2 };
        _advancedLayout = new VerticalStackLayout { Spacing = 4, IsVisible = false };

        var root = new VerticalStackLayout { Spacing = 10 };

        // 1. Sorting
        var sortGrid = new Grid { ColumnDefinitions = { new ColumnDefinition(), new ColumnDefinition() }, RowDefinitions = { new RowDefinition(), new RowDefinition() }, RowSpacing = 4, ColumnSpacing = 4 };
        sortGrid.Add(CreateSortButton("Sort A-Z", "SortAscendingCommand"), 0, 0);
        sortGrid.Add(CreateSortButton("Sort Z-A", "SortDescendingCommand"), 1, 0);
        sortGrid.Add(CreateSortButton("Add A-Z", "AddSubSortAscendingCommand"), 0, 1);
        sortGrid.Add(CreateSortButton("Add Z-A", "AddSubSortDescendingCommand"), 1, 1);
        root.Add(sortGrid);
        root.Add(new BoxView { HeightRequest = 1, Color = Colors.LightGray });

        // 2. Search & Accumulation
        var search = new Entry { Placeholder = "Search...", Margin = new Thickness(0, 0, 0, 4) };
        search.SetBinding(Entry.TextProperty, new Binding("SearchText", BindingMode.TwoWay));
        root.Add(search);

        var accPanel = new HorizontalStackLayout { Spacing = 8 };
        var addToFilter = new CheckBox();
        addToFilter.SetBinding(CheckBox.IsCheckedProperty, new Binding("AddToExistingFilter", BindingMode.TwoWay));
        accPanel.Add(addToFilter);
        accPanel.Add(new Label { Text = "Add to filter", VerticalTextAlignment = TextAlignment.Center, FontSize = 12 });
        
        var modePicker = new Picker { FontSize = 11, WidthRequest = 100 };
        modePicker.ItemsSource = Enum.GetValues<AccumulationMode>();
        modePicker.SetBinding(Picker.SelectedItemProperty, new Binding("AccumulationMode", BindingMode.TwoWay));
        modePicker.SetBinding(VisualElement.IsVisibleProperty, new Binding("AddToExistingFilter"));
        accPanel.Add(modePicker);
        root.Add(accPanel);

        // 3. Advanced Filter
        var advancedToggle = new Button { Text = "Advanced Filter", FontSize = 12, BackgroundColor = Colors.Transparent, TextColor = Colors.Blue, Padding = 0, HeightRequest = 30 };
        advancedToggle.Clicked += (s, e) => _advancedLayout.IsVisible = !_advancedLayout.IsVisible;
        root.Add(advancedToggle);

        _advancedLayout.Add(new Label { Text = "Operator", FontSize = 10, TextColor = Colors.Gray });
        var opPicker = new Picker();
        opPicker.SetBinding(Picker.ItemsSourceProperty, new Binding("AvailableOperators"));
        opPicker.SetBinding(Picker.SelectedItemProperty, new Binding("SelectedCustomOperator", BindingMode.TwoWay));
        _advancedLayout.Add(opPicker);

        _advancedLayout.Add(new Label { Text = "Value", FontSize = 10, TextColor = Colors.Gray });
        var val1 = new Entry();
        val1.SetBinding(Entry.TextProperty, new Binding("CustomValue1", BindingMode.TwoWay));
        _advancedLayout.Add(val1);

        root.Add(_advancedLayout);
        root.Add(new BoxView { HeightRequest = 1, Color = Colors.LightGray });

        // 4. List Section
        var scroll = new ScrollView { Content = _itemsLayout, HeightRequest = 200 };
        root.Add(scroll);

        // 5. Actions
        var actions = new Grid { ColumnDefinitions = { new ColumnDefinition(), new ColumnDefinition() }, ColumnSpacing = 10 };
        var ok = new Button { Text = "OK" };
        ok.SetBinding(Button.CommandProperty, new Binding("ApplyCommand"));
        actions.Add(ok, 0, 0);

        var clear = new Button { Text = "Clear" };
        clear.SetBinding(Button.CommandProperty, new Binding("ClearCommand"));
        actions.Add(clear, 1, 0);
        root.Add(actions);

        Content = root;
    }

    private Button CreateSortButton(string text, string commandPath)
    {
        var btn = new Button { Text = text, FontSize = 10, HeightRequest = 35, Padding = 2 };
        btn.SetBinding(Button.CommandProperty, new Binding(commandPath));
        return btn;
    }

    public void Bind(ColumnFilterViewModel vm)
    {
        ViewModel = vm;
        BindingContext = vm;
        ViewModel.FilterValues.CollectionChanged += (s, e) => UpdateItemsList();
        UpdateItemsList();
    }

    private void UpdateItemsList()
    {
        _itemsLayout.Children.Clear();
        if (ViewModel == null) return;

        var selectAll = new HorizontalStackLayout { Spacing = 8 };
        var cbAll = new CheckBox();
        cbAll.SetBinding(CheckBox.IsCheckedProperty, new Binding("SelectAll", BindingMode.TwoWay));
        selectAll.Add(cbAll);
        selectAll.Add(new Label { Text = "(Select All)", VerticalTextAlignment = TextAlignment.Center });
        _itemsLayout.Add(selectAll);

        foreach (var item in ViewModel.FilterValues)
        {
            AddFilterItemView(item, 0);
        }
    }

    private void AddFilterItemView(FilterValueItem item, int indent)
    {
        var panel = new HorizontalStackLayout { Spacing = 8, Margin = new Thickness(indent * 20, 0, 0, 0) };
        var cb = new CheckBox();
        cb.SetBinding(CheckBox.IsCheckedProperty, new Binding("IsSelected", BindingMode.TwoWay, source: item));
        panel.Add(cb);
        panel.Add(new Label { Text = item.DisplayText, VerticalTextAlignment = TextAlignment.Center });
        _itemsLayout.Add(panel);

        foreach (var child in item.Children)
        {
            AddFilterItemView(child, indent + 1);
        }
    }
}
