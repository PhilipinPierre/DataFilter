using DataFilter.PlatformShared.ViewModels;
using DataFilter.Core.Enums;
using System.Collections.ObjectModel;
using DataFilter.Filtering.ExcelLike.Models;
using System.ComponentModel;
using Microsoft.Maui.ApplicationModel;
using DataFilter.Localization;
using DataFilter.PlatformShared.Theming;
using DataFilter.Maui.Theming;

namespace DataFilter.Maui.Controls;

public sealed class FilterPopupView : ContentView
{
    public ColumnFilterViewModel? ViewModel { get; private set; }
    public event EventHandler? CloseRequested;

    /// <summary>Raised when the user cancels without applying (bar draft removal).</summary>
    public event EventHandler? CancelRequested;

    private readonly ObservableCollection<FlatFilterItem> _flatItems = new();
    private readonly CollectionView _itemsView;
    private readonly VerticalStackLayout _advancedLayout;
    private ObservableCollection<FilterValueItem>? _subscribedFilterValues;
    private readonly Label _addToFilterLabel;
    private readonly Button _advancedToggle;
    private readonly Label _operatorLabel;
    private readonly Label _valueLabel;
    private readonly Label _toLabel;
    private readonly VerticalStackLayout _customValue2Layout;
    private Label? _selectAllLabel;
    private readonly Entry _searchEntry;
    private readonly Button _okButton;
    private readonly Button _cancelButton;
    private readonly Button _clearButton;
    private readonly Picker _modePicker;
    private readonly Picker _opPicker;
    private readonly ObservableCollection<LocalizedItem> _localizedAccModes = new();
    private readonly ObservableCollection<LocalizedItem> _localizedOperators = new();

    public FilterPopupView()
    {
        Padding = 10;
        WidthRequest = 280;
        MinimumHeightRequest = 350;
        ApplyTheme();
        FilterTheme.CurrentChanged += OnGlobalThemeChanged;

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

        _clearButton = new Button();
        _clearButton.SetBinding(Button.CommandProperty, new Binding("ClearCommand"));
        root.Add(_clearButton);

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

        var val1Text = new Entry();
        val1Text.SetBinding(Entry.TextProperty, new Binding("CustomValue1", BindingMode.TwoWay));
        val1Text.SetBinding(VisualElement.IsVisibleProperty, new Binding("DataType", converter: new FilterDataTypeToBoolConverter(), converterParameter: "Text"));
        _advancedLayout.Add(val1Text);

        var val1Date = new DatePicker();
        val1Date.SetBinding(DatePicker.DateProperty, new Binding("CustomValue1", BindingMode.TwoWay, converter: new StringToDateConverter()));
        val1Date.SetBinding(VisualElement.IsVisibleProperty, new Binding("DataType", converter: new FilterDataTypeToBoolConverter(), converterParameter: "Date"));
        _advancedLayout.Add(val1Date);

        var val1Time = new TimePicker();
        val1Time.SetBinding(TimePicker.TimeProperty, new Binding("CustomValue1", BindingMode.TwoWay, converter: new StringToTimeConverter()));
        val1Time.SetBinding(VisualElement.IsVisibleProperty, new Binding("DataType", converter: new FilterDataTypeToBoolConverter(), converterParameter: "Time"));
        _advancedLayout.Add(val1Time);

        _customValue2Layout = new VerticalStackLayout { Spacing = 4, IsVisible = false };
        _customValue2Layout.SetBinding(VisualElement.IsVisibleProperty, new Binding("SelectedCustomOperator", converter: new OperatorBetweenVisibilityConverter()));
        _toLabel = new Label { FontSize = 10 };
        _customValue2Layout.Add(_toLabel);

        var val2Text = new Entry();
        val2Text.SetBinding(Entry.TextProperty, new Binding("CustomValue2", BindingMode.TwoWay));
        val2Text.SetBinding(VisualElement.IsVisibleProperty, new Binding("DataType", converter: new FilterDataTypeToBoolConverter(), converterParameter: "Text"));
        _customValue2Layout.Add(val2Text);

        var val2Date = new DatePicker();
        val2Date.SetBinding(DatePicker.DateProperty, new Binding("CustomValue2", BindingMode.TwoWay, converter: new StringToDateConverter()));
        val2Date.SetBinding(VisualElement.IsVisibleProperty, new Binding("DataType", converter: new FilterDataTypeToBoolConverter(), converterParameter: "Date"));
        _customValue2Layout.Add(val2Date);

        var val2Time = new TimePicker();
        val2Time.SetBinding(TimePicker.TimeProperty, new Binding("CustomValue2", BindingMode.TwoWay, converter: new StringToTimeConverter()));
        val2Time.SetBinding(VisualElement.IsVisibleProperty, new Binding("DataType", converter: new FilterDataTypeToBoolConverter(), converterParameter: "Time"));
        _customValue2Layout.Add(val2Time);

        _advancedLayout.Add(_customValue2Layout);

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

        _cancelButton = new Button();
        _cancelButton.Clicked += (_, _) => CancelRequested?.Invoke(this, EventArgs.Empty);
        actions.Add(_cancelButton, 1, 0);
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

    /// <summary>Applies <paramref name="theme"/> (or <see cref="FilterTheme.Current"/> when null).</summary>
    public void ApplyTheme(FilterTheme? theme = null)
    {
        theme ??= FilterTheme.Current;
        BackgroundColor = FilterThemeApplier.ToMauiColor(theme.PopupBackground);
        _advancedToggle.TextColor = FilterThemeApplier.ToMauiColor(theme.PrimaryColor);
        _okButton.BackgroundColor = FilterThemeApplier.ToMauiColor(theme.PrimaryColor);
        _okButton.TextColor = FilterThemeApplier.ToMauiColor(theme.PrimaryButtonForeground);
    }

    private void OnGlobalThemeChanged(object? sender, EventArgs e)
    {
        if (Handler == null)
            return;
        MainThread.BeginInvokeOnMainThread(() => ApplyTheme());
    }

    private void ApplyLocalization()
    {
        _searchEntry.Placeholder = LocalizationManager.Instance["SearchPlaceholder"];
        _addToFilterLabel.Text = LocalizationManager.Instance["AddToFilter"];
        _advancedToggle.Text = LocalizationManager.Instance["AdvancedFilter"];
        _operatorLabel.Text = LocalizationManager.Instance["OperatorText"];
        _valueLabel.Text = LocalizationManager.Instance["ValueText"];
        _toLabel.Text = LocalizationManager.Instance["ToText"];
        if (_selectAllLabel != null)
            _selectAllLabel.Text = LocalizationManager.Instance["SelectAll"];
        _okButton.Text = LocalizationManager.Instance["Ok"];
        _cancelButton.Text = LocalizationManager.Instance["Cancel"];
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

    private sealed class OperatorBetweenVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture) =>
            value is FilterOperator op && op == FilterOperator.Between;

        public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
