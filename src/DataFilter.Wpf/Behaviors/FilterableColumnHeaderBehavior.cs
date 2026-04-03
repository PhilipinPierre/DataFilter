using DataFilter.Wpf.Controls;
using DataFilter.Wpf.ViewModels;
using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace DataFilter.Wpf.Behaviors;

/// <summary>
/// Attaches a filter button to a ColumnHeader and manages the filter popup lifecycle.
/// Works with both DataGrid and GridView (ListView).
/// </summary>
public class FilterableColumnHeaderBehavior : Behavior<FrameworkElement>
{
    private ColumnFilterButton? _filterButton;
    private Popup? _filterPopup;
    private ColumnFilterViewModel? _viewModel;
    private bool _contentInjected;

    #region IsFilterable Attached Property

    public static readonly DependencyProperty IsFilterableProperty =
        DependencyProperty.RegisterAttached(
            "IsFilterable",
            typeof(bool),
            typeof(FilterableColumnHeaderBehavior),
            new PropertyMetadata(false, OnIsFilterableChanged));

    public static bool GetIsFilterable(DependencyObject obj) => (bool)obj.GetValue(IsFilterableProperty);
    public static void SetIsFilterable(DependencyObject obj, bool value) => obj.SetValue(IsFilterableProperty, value);

    private static void OnIsFilterableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FrameworkElement header && (bool)e.NewValue)
        {
            var behaviors = Interaction.GetBehaviors(header);
            if (!behaviors.OfType<FilterableColumnHeaderBehavior>().Any())
            {
                behaviors.Add(new FilterableColumnHeaderBehavior());
            }
        }
    }

    #endregion

    #region PropertyPath Attached Property

    public static readonly DependencyProperty PropertyPathProperty =
        DependencyProperty.RegisterAttached(
            "PropertyPath",
            typeof(string),
            typeof(FilterableColumnHeaderBehavior),
            new PropertyMetadata(null));

    public static string GetPropertyPath(DependencyObject obj) => (string)obj.GetValue(PropertyPathProperty);
    public static void SetPropertyPath(DependencyObject obj, string value) => obj.SetValue(PropertyPathProperty, value);

    #endregion

    #region ParentViewModel Dependency Property

    public static readonly DependencyProperty ParentViewModelProperty =
        DependencyProperty.Register(
            nameof(ParentViewModel),
            typeof(object),
            typeof(FilterableColumnHeaderBehavior),
            new PropertyMetadata(null, async (d, e) => await((FilterableColumnHeaderBehavior)d).TryInitializeAsync()));

    public object? ParentViewModel
    {
        get => GetValue(ParentViewModelProperty);
        set => SetValue(ParentViewModelProperty, value);
    }

    #endregion

    #region PropertyName Dependency Property

    public static readonly DependencyProperty PropertyNameProperty =
        DependencyProperty.Register(
            nameof(PropertyName),
            typeof(string),
            typeof(FilterableColumnHeaderBehavior),
            new PropertyMetadata(null, async (d, e) => await((FilterableColumnHeaderBehavior)d).TryInitializeAsync()));

    public string? PropertyName
    {
        get => (string?)GetValue(PropertyNameProperty);
        set => SetValue(PropertyNameProperty, value);
    }

    #endregion

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.Loaded += OnAssociatedObjectLoaded;
        AssociatedObject.DataContextChanged += OnDataContextChanged;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.Loaded -= OnAssociatedObjectLoaded;
        AssociatedObject.DataContextChanged -= OnDataContextChanged;
        if (_filterButton != null)
        {
            _filterButton.Click -= OnFilterButtonClick;
        }

        if (_filterPopup != null)
        {
            _filterPopup.Opened -= OnFilterPopupOpened;
            _filterPopup.Closed -= OnFilterPopupClosed;
            var window = Window.GetWindow(AssociatedObject);
            if (window != null)
                window.PreviewMouseLeftButtonDown -= OnWindowMouseDown;
        }

    }

    private async void OnAssociatedObjectLoaded(object sender, RoutedEventArgs e)
    {
        TryResolvePropertyName();
        TryResolveParentViewModel();
        await TryInitializeAsync();
    }

    private async void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        TryResolvePropertyName();
        TryResolveParentViewModel();
        await TryInitializeAsync();
    }

    private void TryResolvePropertyName()
    {
        if (!string.IsNullOrEmpty(PropertyName)) return;

        // Implementation for DataGrid
        if (AssociatedObject is DataGridColumnHeader dgHeader && dgHeader.Column != null)
        {
            // Check if column explicitly disabled filtering
            var isFilterableObj = dgHeader.Column.ReadLocalValue(IsFilterableProperty);
            if (isFilterableObj is bool isF && !isF)
            {
                PropertyName = string.Empty; // Abort
                return;
            }

            // Check explicitly defined PropertyPath
            var explicitPath = GetPropertyPath(dgHeader.Column);
            if (!string.IsNullOrEmpty(explicitPath))
            {
                PropertyName = explicitPath;
                return;
            }

            // Fallback to binding
            if (dgHeader.Column is DataGridBoundColumn dgCol && dgCol.Binding is Binding b)
            {
                PropertyName = b.Path.Path;
            }
        }
        // Implementation for GridView (ListView)
        else if (AssociatedObject is GridViewColumnHeader gvHeader && gvHeader.Column != null)
        {
            var isFilterableObj = gvHeader.Column.ReadLocalValue(IsFilterableProperty);
            if (isFilterableObj is bool isF && !isF)
            {
                PropertyName = string.Empty; // Abort
                return;
            }

            var explicitPath = GetPropertyPath(gvHeader.Column);
            if (!string.IsNullOrEmpty(explicitPath))
            {
                PropertyName = explicitPath;
                return;
            }

            if (gvHeader.Column.DisplayMemberBinding is Binding b)
            {
                PropertyName = b.Path.Path;
            }
        }
    }

    private void TryResolveParentViewModel()
    {
        if (ParentViewModel != null) return;

        var parent = VisualTreeHelper.GetParent(AssociatedObject);
        while (parent != null)
        {
            if (parent is DataFilter.Wpf.Controls.FilterableDataGrid fg)
            {
                ParentViewModel = fg.ViewModel;
                if (ParentViewModel != null) return;
            }

            if (parent is ListView lv && lv.View is DataFilter.Wpf.Controls.FilterableGridView fv)
            {
                ParentViewModel = fv.ViewModel;
                if (ParentViewModel != null) return;
            }

            if (parent is FrameworkElement fe && fe.DataContext is IFilterableDataGridViewModel vm)
            {
                ParentViewModel = vm;
                return;
            }

            parent = VisualTreeHelper.GetParent(parent);
        }
    }

    private async Task TryInitializeAsync()
    {
        TryResolvePropertyName();

        if (_viewModel != null || string.IsNullOrEmpty(PropertyName) || ParentViewModel is not IFilterableDataGridViewModel parentVm)
            return;

        _viewModel = new ColumnFilterViewModel(
            async (searchText) => await parentVm.GetDistinctValuesAsync(PropertyName, searchText),
            (state) => parentVm.ApplyColumnFilter(PropertyName, state),
            () => parentVm.ClearColumnFilter(PropertyName),
            (isDesc) => parentVm.ApplySort(PropertyName, isDesc),
            (isDesc) => parentVm.AddSubSort(PropertyName, isDesc),
            parentVm.GetPropertyType(PropertyName)
        );

        BuildHeaderContent();

        if (ParentViewModel is IFilterableDataGridViewModel pVm && !string.IsNullOrEmpty(PropertyName))
        {
            pVm.FilterableProperties.Add(PropertyName);
        }

        var existingState = parentVm.GetColumnFilterState(PropertyName);
        if (existingState != null)
        {
            await _viewModel.LoadStateAsync(existingState);
        }
    }

    private void BuildHeaderContent()
    {
        if (_contentInjected || _viewModel == null) return;
        _contentInjected = true;

        ContentControl? header = AssociatedObject as ContentControl;
        if (header == null) return;

        var existingContent = header.Content;
        var dockPanel = new DockPanel
        {
            LastChildFill = true,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        _filterButton = new ColumnFilterButton
        {
            Margin = new Thickness(4, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Cursor = Cursors.Hand
        };
        _filterButton.Click += OnFilterButtonClick;

        // Bind IsActive
        _filterButton.SetBinding(ColumnFilterButton.IsActiveProperty, new Binding(nameof(ColumnFilterViewModel.IsFilterActive)) { Source = _viewModel });

        DockPanel.SetDock(_filterButton, Dock.Right);
        dockPanel.Children.Add(_filterButton);

        if (existingContent is UIElement element)
        {
            // Move existing element to dockpanel
            header.Content = null;
            dockPanel.Children.Add(element);
        }
        else
        {
            var textBlock = new TextBlock
            {
                Text = existingContent?.ToString() ?? string.Empty,
                VerticalAlignment = VerticalAlignment.Center
            };
            dockPanel.Children.Add(textBlock);
        }

        header.Content = dockPanel;
    }

    /// <summary>
    /// Opens/closes the filter popup. Matches v1.0.0 timing: empty search then <see cref="Popup.IsOpen"/> = true
    /// in the same turn (no deferred open / no await before show). That ordering is required for opening
    /// another column’s popup after filtering a first column; async + deferred open regressed that scenario.
    /// State sync remains in <see cref="TryInitializeAsync"/> (and parent-driven updates while the popup is open).
    /// </summary>
    private void OnFilterButtonClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        if (_viewModel == null) return;

        // Keep post-refactor robustness: headers/parent can change when the grid re-templates after a filter.
        TryResolvePropertyName();
        TryResolveParentViewModel();

        if (_filterPopup == null)
            BuildPopup();

        if (_filterPopup!.IsOpen)
        {
            _filterPopup.IsOpen = false;
        }
        else
        {
            _ = _viewModel.SearchCommand.ExecuteAsync(string.Empty);
            _filterPopup.IsOpen = true;
        }
    }

    private void BuildPopup()
    {
        _filterPopup = new Popup
        {
            StaysOpen = true,
            AllowsTransparency = true,
            PlacementTarget = _filterButton,
            Placement = PlacementMode.Bottom,
            PopupAnimation = PopupAnimation.Fade
        };

        var filterControl = new FilterPopup { DataContext = _viewModel };
        _filterPopup.Child = filterControl;

        _viewModel!.OnApply += (_, _) => _filterPopup.IsOpen = false;
        _viewModel.OnClear += (_, _) => _filterPopup.IsOpen = false;

        // Subscribe once per open, unsubscribe on Closed — otherwise handlers stack on every open
        // while Apply/Clear never removed them, breaking subsequent filter button clicks.
        _filterPopup.Opened += OnFilterPopupOpened;
        _filterPopup.Closed += OnFilterPopupClosed;
    }

    private void OnFilterPopupOpened(object? sender, EventArgs e)
    {
        var window = Window.GetWindow(AssociatedObject);
        if (window != null)
            window.PreviewMouseLeftButtonDown += OnWindowMouseDown;
    }

    private void OnFilterPopupClosed(object? sender, EventArgs e)
    {
        var window = Window.GetWindow(AssociatedObject);
        if (window != null)
            window.PreviewMouseLeftButtonDown -= OnWindowMouseDown;
    }

    private void OnWindowMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (_filterPopup is not { IsOpen: true, Child: UIElement child })
            return;

        // Before first layout, RenderSize can be (0,0). Rect with zero area contains no points,
        // so every position looks "outside" and we would close the popup immediately after open.
        double w = child.RenderSize.Width;
        double h = child.RenderSize.Height;
        if (child is FrameworkElement fe)
        {
            if (w <= 0) w = fe.ActualWidth;
            if (h <= 0) h = fe.ActualHeight;
        }

        if (w <= 0 || h <= 0)
            return;

        if (!new Rect(0, 0, w, h).Contains(e.GetPosition(child)))
            _filterPopup.IsOpen = false;
    }

    // FindFilterableParent is no longer needed but kept empty for safety, or removed entirely.
}
