using DataFilter.PlatformShared.ViewModels;
using DataFilter.Core.Enums;
using System.Collections.ObjectModel;
using DataFilter.Filtering.ExcelLike.Models;
using System.ComponentModel;
using Microsoft.Maui.ApplicationModel;
using DataFilter.Localization;

namespace DataFilter.Maui.Controls;

public sealed class FilterPopupView : ContentView
{
    public ColumnFilterViewModel? ViewModel { get; private set; }
    public event EventHandler? CloseRequested;

    private readonly ObservableCollection<FlatFilterItem> _flatItems = new();
    private readonly CollectionView _itemsView;
    private readonly VerticalStackLayout _advancedLayout;
    private ObservableCollection<FilterValueItem>? _subscribedFilterValues;
    private readonly Label _addToFilterLabel;
    private readonly Button _advancedToggle;
    private readonly Label _operatorLabel;
    private readonly Label _valueLabel;
    private Label? _selectAllLabel;
    private readonly Entry _searchEntry;
    private readonly Button _okButton;
    private readonly Button _clearButton;
    private readonly Picker _modePicker;
    private readonly Picker _opPicker;
    private readonly ObservableCollection<LocalizedItem> _localizedAccModes = new();
    private readonly ObservableCollection<LocalizedItem> _localizedOperators = new();

    public FilterPopupView()
    {
        Padding = 10;
        // Follow the app theme (dark/light) instead of forcing white.
        this.SetAppThemeColor(BackgroundColorProperty, Colors.White, Color.FromArgb("#1f1f1f"));
        WidthRequest = 280;
        MinimumHeightRequest = 350;

        _itemsView = CreateItemsView();
        _advancedLayout = new VerticalStackLayout { Spacing = 4, IsVisible = false };

        var root = new VerticalStackLayout { Spacing = 10 };

        // 1. Sorting
        var sortGrid = new Grid { ColumnDefinitions = { new ColumnDefinition(), new ColumnDefinition() }, RowDefinitions = { new RowDefinition(), new RowDefinition() }, RowSpacing = 4, ColumnSpacing = 4 };
        sortGrid.Add(CreateSortButton("Sort A-Z", "SortAscendingCommand"), 0, 0);
        sortGrid.Add(CreateSortButton("Sort Z-A", "SortDescendingCommand"), 1, 0);
        sortGrid.Add(CreateSortButton("Add A-Z", "AddSubSortAscendingCommand"), 0, 1);
        sortGrid.Add(CreateSortButton("Add Z-A", "AddSubSortDescendingCommand"), 1, 1);
        root.Add(sortGrid);
        root.Add(new BoxView { HeightRequest = 1 });

        // 2. Search & Accumulation
        _searchEntry = new Entry { Margin = new Thickness(0, 0, 0, 4) };
        _searchEntry.SetBinding(Entry.TextProperty, new Binding("SearchText", BindingMode.TwoWay));
        root.Add(_searchEntry);

        var accPanel = new HorizontalStackLayout { Spacing = 8 };
        var addToFilter = new CheckBox();
        addToFilter.SetBinding(CheckBox.IsCheckedProperty, new Binding("AddToExistingFilter", BindingMode.TwoWay));
        accPanel.Add(addToFilter);
        _addToFilterLabel = new Label { VerticalTextAlignment = TextAlignment.Center, FontSize = 12 };
        accPanel.Add(_addToFilterLabel);
        
        _modePicker = new Picker { FontSize = 11, WidthRequest = 140, ItemDisplayBinding = new Binding(nameof(LocalizedItem.Text)) };
        _modePicker.ItemsSource = _localizedAccModes;
        _modePicker.SelectedIndexChanged += (_, _) =>
        {
            if (ViewModel == null) return;
            if (_modePicker.SelectedItem is LocalizedItem { Value: AccumulationMode mode })
                ViewModel.AccumulationMode = mode;
        };
        _modePicker.SetBinding(VisualElement.IsVisibleProperty, new Binding("AddToExistingFilter"));
        accPanel.Add(_modePicker);
        root.Add(accPanel);

        // 3. Advanced Filter
        _advancedToggle = new Button { FontSize = 12, BackgroundColor = Colors.Transparent, Padding = 0, HeightRequest = 30 };
        _advancedToggle.SetAppThemeColor(Button.TextColorProperty, Colors.Blue, Color.FromArgb("#ac99ea"));
        _advancedToggle.Clicked += (s, e) => _advancedLayout.IsVisible = !_advancedLayout.IsVisible;
        root.Add(_advancedToggle);

        _operatorLabel = new Label { FontSize = 10 };
        _advancedLayout.Add(_operatorLabel);
        _opPicker = new Picker { ItemDisplayBinding = new Binding(nameof(LocalizedItem.Text)) };
        _opPicker.ItemsSource = _localizedOperators;
        _opPicker.SelectedIndexChanged += (_, _) =>
        {
            if (ViewModel == null) return;
            if (_opPicker.SelectedItem is LocalizedItem { Value: FilterOperator op })
                ViewModel.SelectedCustomOperator = op;
        };
        _advancedLayout.Add(_opPicker);

        _valueLabel = new Label { FontSize = 10 };
        _advancedLayout.Add(_valueLabel);
        var val1 = new Entry();
        val1.SetBinding(Entry.TextProperty, new Binding("CustomValue1", BindingMode.TwoWay));
        _advancedLayout.Add(val1);

        root.Add(_advancedLayout);
        root.Add(new BoxView { HeightRequest = 1 });

        // 4. List Section
        _itemsView.HeightRequest = 200;
        root.Add(_itemsView);

        // 5. Actions
        var actions = new Grid { ColumnDefinitions = { new ColumnDefinition(), new ColumnDefinition() }, ColumnSpacing = 10 };
        _okButton = new Button();
        _okButton.SetBinding(Button.CommandProperty, new Binding("ApplyCommand"));
        actions.Add(_okButton, 0, 0);

        _clearButton = new Button();
        _clearButton.SetBinding(Button.CommandProperty, new Binding("ClearCommand"));
        actions.Add(_clearButton, 1, 0);
        root.Add(actions);

        Content = root;

        LocalizationManager.Instance.CultureChanged += (_, _) => ApplyLocalization();
        ApplyLocalization();
    }

    private sealed class FlatFilterItem
    {
        public FlatFilterItem(FilterValueItem item, int indent)
        {
            Item = item;
            Margin = new Thickness(indent * 20, 0, 0, 0);
        }

        public FilterValueItem Item { get; }
        public Thickness Margin { get; }
    }

    private CollectionView CreateItemsView()
    {
        var selectAllHeader = new HorizontalStackLayout { Spacing = 8, Padding = new Thickness(0, 0, 0, 6) };
        var cbAll = new CheckBox();
        cbAll.SetBinding(CheckBox.IsCheckedProperty, new Binding("SelectAll", BindingMode.TwoWay));
        selectAllHeader.Add(cbAll);
        _selectAllLabel = new Label { VerticalTextAlignment = TextAlignment.Center };
        selectAllHeader.Add(_selectAllLabel);

        return new CollectionView
        {
            ItemsSource = _flatItems,
            Header = selectAllHeader,
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical) { ItemSpacing = 2 },
            ItemTemplate = new DataTemplate(() =>
            {
                var row = new HorizontalStackLayout { Spacing = 8 };
                row.SetBinding(MarginProperty, new Binding(nameof(FlatFilterItem.Margin)));

                var cb = new CheckBox();
                cb.SetBinding(CheckBox.IsCheckedProperty, new Binding("Item.IsSelected", BindingMode.TwoWay));

                var label = new Label { VerticalTextAlignment = TextAlignment.Center };
                label.SetBinding(Label.TextProperty, new Binding("Item.DisplayText"));

                row.Add(cb);
                row.Add(label);

                return row;
            })
        };
    }

    private Button CreateSortButton(string text, string commandPath)
    {
        var btn = new Button { Text = text, FontSize = 10, HeightRequest = 35, Padding = 2 };
        btn.SetBinding(Button.CommandProperty, new Binding(commandPath));
        return btn;
    }

    public void Bind(ColumnFilterViewModel vm)
    {
        if (ViewModel != null)
        {
            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            ViewModel.OnApply -= ViewModel_OnApply;
            ViewModel.OnClear -= ViewModel_OnClear;
            UnsubscribeFilterValues();
        }

        ViewModel = vm;
        BindingContext = vm;
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        ViewModel.OnApply += ViewModel_OnApply;
        ViewModel.OnClear += ViewModel_OnClear;
        SubscribeFilterValues(vm.FilterValues);
        UpdateItemsList();
        BuildLocalizedLists();
        ApplyLocalization();

        // Ensure distinct values are loaded on first display (SearchText starts empty so it won't auto-trigger).
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (ViewModel?.SearchCommand != null)
                await ViewModel.SearchCommand.ExecuteAsync(string.Empty);
        });
    }

    private void ViewModel_OnApply(object? sender, EventArgs e) => CloseRequested?.Invoke(this, EventArgs.Empty);
    private void ViewModel_OnClear(object? sender, EventArgs e) => CloseRequested?.Invoke(this, EventArgs.Empty);

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ColumnFilterViewModel.FilterValues) && ViewModel != null)
        {
            SubscribeFilterValues(ViewModel.FilterValues);
            UpdateItemsList();
        }
    }

    private void SubscribeFilterValues(ObservableCollection<FilterValueItem> values)
    {
        if (ReferenceEquals(_subscribedFilterValues, values))
            return;

        UnsubscribeFilterValues();
        _subscribedFilterValues = values;
        _subscribedFilterValues.CollectionChanged += FilterValues_CollectionChanged;
    }

    private void UnsubscribeFilterValues()
    {
        if (_subscribedFilterValues != null)
            _subscribedFilterValues.CollectionChanged -= FilterValues_CollectionChanged;
        _subscribedFilterValues = null;
    }

    private void FilterValues_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => UpdateItemsList();

    private void UpdateItemsList()
    {
        if (ViewModel == null) return;
        _flatItems.Clear();
        foreach (var item in ViewModel.FilterValues)
            Flatten(item, 0);
    }

    private void Flatten(FilterValueItem item, int indent)
    {
        _flatItems.Add(new FlatFilterItem(item, indent));
        foreach (var child in item.Children)
            Flatten(child, indent + 1);
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
        _searchEntry.Placeholder = LocalizationManager.Instance["SearchPlaceholder"];
        _addToFilterLabel.Text = LocalizationManager.Instance["AddToFilter"];
        _advancedToggle.Text = LocalizationManager.Instance["AdvancedFilter"];
        _operatorLabel.Text = LocalizationManager.Instance["OperatorText"];
        _valueLabel.Text = LocalizationManager.Instance["ValueText"];
        if (_selectAllLabel != null)
            _selectAllLabel.Text = LocalizationManager.Instance["SelectAll"];
        _okButton.Text = LocalizationManager.Instance["Ok"];
        _clearButton.Text = LocalizationManager.Instance["Clear"];

        BuildLocalizedLists();
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
