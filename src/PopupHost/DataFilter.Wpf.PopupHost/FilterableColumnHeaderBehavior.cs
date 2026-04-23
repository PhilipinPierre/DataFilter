using System.ComponentModel;
using DataFilter.Localization;
using DataFilter.Wpf.Controls;
using DataFilter.Wpf.ViewModels;
using Microsoft.Xaml.Behaviors;
using ColumnFilterVm = DataFilter.Wpf.ViewModels.ColumnFilterViewModel;
using GridFilterVm = DataFilter.Wpf.ViewModels.IFilterableDataGridViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Automation;

namespace DataFilter.Wpf.Behaviors;

/// <summary>
/// Attaches a filter button to a ColumnHeader and manages the filter popup lifecycle.
/// Works with both DataGrid and GridView (ListView).
/// </summary>
public class FilterableColumnHeaderBehavior : Behavior<FrameworkElement>
{
    private ColumnFilterButton? _filterButton;
    private Popup? _filterPopup;
    private ColumnFilterVm? _viewModel;
    private bool _contentInjected;
    private GridFilterVm? _filterParentSubscriptions;
    private System.Globalization.CultureInfo? _cultureBeforePopup;

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
        UnsubscribeParentFilterEvents();
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
            // First-class attach point: allow binding the VM directly on an existing DataGrid/GridView.
            // This avoids visual-tree fragility and enables a one-line opt-in on the grid.
            var attachedVm = FilterableGridAttach.GetViewModel(parent);
            if (attachedVm is GridFilterVm attachedGridVm)
            {
                ParentViewModel = attachedGridVm;
                return;
            }

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

            // If the client uses a plain ListView+GridView, allow attaching the VM on the GridView itself.
            if (parent is ListView lv2 && lv2.View is GridView gv)
            {
                var gvVm = FilterableGridAttach.GetViewModel(gv);
                if (gvVm is GridFilterVm vm2)
                {
                    ParentViewModel = vm2;
                    return;
                }
            }

            if (parent is FrameworkElement fe && fe.DataContext is GridFilterVm vm)
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

        if (_viewModel != null || string.IsNullOrEmpty(PropertyName) || ParentViewModel is not GridFilterVm parentVm)
            return;

        _viewModel = new ColumnFilterVm(
            async (searchText) => await parentVm.GetDistinctValuesAsync(PropertyName, searchText),
            (state) => parentVm.ApplyColumnFilter(PropertyName, state),
            () => parentVm.ClearColumnFilter(PropertyName),
            (isDesc) => parentVm.ApplySort(PropertyName, isDesc),
            (isDesc) => parentVm.AddSubSort(PropertyName, isDesc),
            parentVm.GetPropertyType(PropertyName)
        );

        BuildHeaderContent();

        if (ParentViewModel is GridFilterVm pVm && !string.IsNullOrEmpty(PropertyName))
        {
            pVm.FilterableProperties.Add(PropertyName);
        }

        var existingState = parentVm.GetColumnFilterState(PropertyName);
        if (existingState != null)
        {
            await _viewModel.LoadStateAsync(existingState);
        }

        SubscribeParentFilterEvents(parentVm);
    }

    private void SubscribeParentFilterEvents(GridFilterVm parentVm)
    {
        UnsubscribeParentFilterEvents();
        _filterParentSubscriptions = parentVm;
        parentVm.FilterDescriptorsChanged += OnFilterDescriptorsChanged;
        if (parentVm is INotifyPropertyChanged npc)
            npc.PropertyChanged += OnParentGridPropertyChanged;
    }

    private void UnsubscribeParentFilterEvents()
    {
        if (_filterParentSubscriptions is { } vm)
        {
            vm.FilterDescriptorsChanged -= OnFilterDescriptorsChanged;
            if (vm is INotifyPropertyChanged npc)
                npc.PropertyChanged -= OnParentGridPropertyChanged;
            _filterParentSubscriptions = null;
        }
    }

    private async void OnFilterDescriptorsChanged(object? sender, DataFilter.PlatformShared.ViewModels.FilterDescriptorsChangedEventArgs e)
    {
        if (_viewModel == null || string.IsNullOrEmpty(PropertyName) || ParentViewModel is not GridFilterVm parentVm)
            return;

        if (e.AffectedPropertyName != null && !string.Equals(e.AffectedPropertyName, PropertyName, StringComparison.OrdinalIgnoreCase))
        {
            _viewModel.RaiseFilterActiveChanged();
            return;
        }

        await SyncColumnFilterFromParentAsync(parentVm);
    }

    private async System.Threading.Tasks.Task SyncColumnFilterFromParentAsync(GridFilterVm parentVm)
    {
        if (_viewModel == null || string.IsNullOrEmpty(PropertyName))
            return;

        await _viewModel.SearchCommand.ExecuteAsync(string.Empty);
        var state = parentVm.GetColumnFilterState(PropertyName);
        if (state != null)
            await _viewModel.LoadStateAsync(state);
        else
            await _viewModel.SyncFromClearedContextAsync();
    }

    private void OnParentGridPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not ("FilteredItems" or "LocalDataSource"))
            return;
        _viewModel?.RaiseFilterActiveChanged();
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
        if (!string.IsNullOrWhiteSpace(PropertyName))
        {
            AutomationProperties.SetAutomationId(_filterButton, $"df-filter-btn-{PropertyName}");
        }
        _filterButton.Click += OnFilterButtonClick;

        // Bind IsActive
        _filterButton.SetBinding(ColumnFilterButton.IsActiveProperty, new Binding(nameof(ColumnFilterVm.IsFilterActive)) { Source = _viewModel });

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
    /// Opens/closes the filter popup. Loads distinct values first, then restores selection from the parent
    /// grid context so reopening a filtered column matches the applied filter. No deferred <c>IsOpen</c> or
    /// forced close before open (those broke opening another column’s popup after filtering a first one).
    /// </summary>
    private async void OnFilterButtonClick(object sender, RoutedEventArgs e)
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
            // Apply a per-grid culture override for the popup lifetime (if provided).
            if (ParentViewModel is GridFilterVm parentVmWithCulture && parentVmWithCulture.CultureOverride != null)
            {
                _cultureBeforePopup = LocalizationManager.Instance.Culture;
                LocalizationManager.Instance.SetCulture(parentVmWithCulture.CultureOverride);
            }

            await _viewModel.SearchCommand.ExecuteAsync(string.Empty);
            if (ParentViewModel is GridFilterVm parentVm && !string.IsNullOrEmpty(PropertyName))
            {
                var state = parentVm.GetColumnFilterState(PropertyName);
                if (state != null)
                    await _viewModel.LoadStateAsync(state);
            }

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
            Placement = PlacementMode.Custom,
            CustomPopupPlacementCallback = PlaceFilterPopup,
            PopupAnimation = PopupAnimation.Fade
        };

        var filterControl = new FilterPopup { DataContext = _viewModel };
        if (!string.IsNullOrWhiteSpace(PropertyName))
        {
            AutomationProperties.SetAutomationId(filterControl, $"df-filter-popup-{PropertyName}");
        }
        _filterPopup.Child = filterControl;

        _viewModel!.OnApply += (_, _) => _filterPopup.IsOpen = false;
        _viewModel.OnClear += (_, _) => _filterPopup.IsOpen = false;

        // Subscribe once per open, unsubscribe on Closed — otherwise handlers stack on every open
        // while Apply/Clear never removed them, breaking subsequent filter button clicks.
        _filterPopup.Opened += OnFilterPopupOpened;
        _filterPopup.Closed += OnFilterPopupClosed;
    }

    private CustomPopupPlacement[] PlaceFilterPopup(Size popupSize, Size targetSize, Point offset)
    {
        if (_filterButton == null)
            return new[] { new CustomPopupPlacement(new Point(0, targetSize.Height), PopupPrimaryAxis.Horizontal) };

        bool isRtl = _filterButton.FlowDirection == FlowDirection.RightToLeft;
        // Default anchor rule:
        // - LTR: popup top-left at button bottom-right
        // - RTL: popup top-right at button bottom-left (=> top-left at -popupWidth, bottom)
        var desiredRelative = new Point(isRtl ? -popupSize.Width : targetSize.Width, targetSize.Height);

        // Clamp to keep as visible as possible within the window (goal: stay visible in the app window).
        // Callback expects offsets in DIPs relative to target.
        var window = Window.GetWindow(_filterButton);
        if (window == null || window.ActualWidth <= 0 || window.ActualHeight <= 0)
            return new[] { new CustomPopupPlacement(desiredRelative, PopupPrimaryAxis.Horizontal) };

        const double margin = 8;
        var targetScreen = _filterButton.PointToScreen(new Point(0, 0));
        var desiredScreen = new Point(targetScreen.X + desiredRelative.X, targetScreen.Y + desiredRelative.Y);

        var windowTopLeft = new Point(window.Left, window.Top);
        var windowBottomRight = new Point(window.Left + window.ActualWidth, window.Top + window.ActualHeight);

        var minX = windowTopLeft.X + margin;
        var maxX = Math.Max(minX, windowBottomRight.X - popupSize.Width - margin);
        var minY = windowTopLeft.Y + margin;
        var maxY = Math.Max(minY, windowBottomRight.Y - popupSize.Height - margin);

        var clampedX = desiredScreen.X < minX ? minX : (desiredScreen.X > maxX ? maxX : desiredScreen.X);
        var clampedY = desiredScreen.Y < minY ? minY : (desiredScreen.Y > maxY ? maxY : desiredScreen.Y);

        var adjustedRelative = new Point(clampedX - targetScreen.X, clampedY - targetScreen.Y);
        return new[] { new CustomPopupPlacement(adjustedRelative, PopupPrimaryAxis.Horizontal) };
    }

    private void OnFilterPopupOpened(object? sender, EventArgs e)
    {
        // Ensure the popup cannot exceed the current monitor working area. Without this, the measured
        // popup size can be larger than small CI desktops (e.g. 1024x720), and placement clamping
        // alone cannot keep the window inside the working area.
        if (_filterPopup is { Child: FrameworkElement child } && _filterButton != null)
        {
            try
            {
                var anchor = _filterButton.PointToScreen(new Point(0, 0));
                var screen = System.Windows.Forms.Screen.FromPoint(new System.Drawing.Point((int)anchor.X, (int)anchor.Y));
                var wa = screen.WorkingArea;
                const double margin = 8;

                child.MaxWidth = Math.Max(200, wa.Width - (margin * 2));
                child.MaxHeight = Math.Max(200, wa.Height - (margin * 2));

                // If the control is explicitly sized larger than allowed, shrink it.
                if (!double.IsNaN(child.Width))
                    child.Width = Math.Min(child.Width, child.MaxWidth);
                if (!double.IsNaN(child.Height))
                    child.Height = Math.Min(child.Height, child.MaxHeight);
            }
            catch
            {
                // Best-effort: never fail popup open due to environment / interop edge cases.
            }
        }

        var window = Window.GetWindow(AssociatedObject);
        if (window != null)
            window.PreviewMouseLeftButtonDown += OnWindowMouseDown;
    }

    private void OnFilterPopupClosed(object? sender, EventArgs e)
    {
        var window = Window.GetWindow(AssociatedObject);
        if (window != null)
            window.PreviewMouseLeftButtonDown -= OnWindowMouseDown;

        if (_cultureBeforePopup != null)
        {
            LocalizationManager.Instance.SetCulture(_cultureBeforePopup);
            _cultureBeforePopup = null;
        }
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
