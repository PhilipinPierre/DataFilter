using System.ComponentModel;
using System.Linq;
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
using DataFilter.PlatformShared.ColumnFilter;

namespace DataFilter.Wpf.Behaviors;

/// <summary>
/// Attaches a filter button to a ColumnHeader and manages the filter popup lifecycle.
/// Works with both DataGrid and GridView (ListView).
/// </summary>
public partial class FilterableColumnHeaderBehavior : Behavior<FrameworkElement>
{
    private ColumnFilterButton? _filterButton;
    private Popup? _filterPopup;
    private ContextMenu? _headerContextMenu;
    private System.Windows.Threading.DispatcherTimer? _longPressTimer;
    private ColumnFilterVm? _viewModel;
    private bool _contentInjected;
    private GridFilterVm? _filterParentSubscriptions;
    private System.Globalization.CultureInfo? _cultureBeforePopup;
    private object? _trackedColumn;
    private int _columnResolveAttempts;
    private int _initResolveAttempts;
    private DataTemplate? _savedHeaderContentTemplate;
    private DataTemplateSelector? _savedHeaderContentTemplateSelector;
    private FilterStateIndicatorAdorner? _filterStateIndicatorAdorner;
    private bool _isPointerOverHeader;

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

    #region ColumnFilterTriggerMode Attached Property

    public static readonly DependencyProperty ColumnFilterTriggerModeProperty =
        DependencyProperty.RegisterAttached(
            "ColumnFilterTriggerMode",
            typeof(ColumnFilterTriggerMode),
            typeof(FilterableColumnHeaderBehavior),
            new PropertyMetadata(ColumnFilterTriggerMode.Inherit));

    public static ColumnFilterTriggerMode GetColumnFilterTriggerMode(DependencyObject obj) =>
        (ColumnFilterTriggerMode)obj.GetValue(ColumnFilterTriggerModeProperty);

    public static void SetColumnFilterTriggerMode(DependencyObject obj, ColumnFilterTriggerMode value) =>
        obj.SetValue(ColumnFilterTriggerModeProperty, value);

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

        // IsFilterable is often applied via ColumnHeaderStyle after the header has already Loaded
        // (e.g. auto-generated columns before FilterableDataGrid.Loaded in the Local demo).
        if (AssociatedObject.IsLoaded)
            _ = HandleColumnContextChangedAsync();
    }

    protected override void OnDetaching()
    {
        UnsubscribeParentFilterEvents();
        base.OnDetaching();
        AssociatedObject.Loaded -= OnAssociatedObjectLoaded;
        AssociatedObject.DataContextChanged -= OnDataContextChanged;
        DetachHeaderTriggerHandlers();
        if (_filterButton != null)
            _filterButton.Click -= OnFilterButtonClick;

        if (_filterPopup != null)
        {
            _filterPopup.Opened -= OnFilterPopupOpened;
            _filterPopup.Closed -= OnFilterPopupClosed;
            FilterColumnPopupTracker.OnPopupClosed(this);
        }

    }

    private async void OnAssociatedObjectLoaded(object sender, RoutedEventArgs e) =>
        await HandleColumnContextChangedAsync();

    private async void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        await HandleColumnContextChangedAsync();
    }

    private async Task HandleColumnContextChangedAsync()
    {
        var column = GetAssociatedColumn();
        if (column == null)
        {
            if (_columnResolveAttempts++ < 12)
            {
                AssociatedObject.Dispatcher.BeginInvoke(
                    () => _ = HandleColumnContextChangedAsync(),
                    System.Windows.Threading.DispatcherPriority.Loaded);
            }

            return;
        }

        _columnResolveAttempts = 0;

        if (ReferenceEquals(column, _trackedColumn) && _viewModel != null)
            return;

        _trackedColumn = column;
        await ResetAndInitializeForColumnAsync();
    }

    private object? GetAssociatedColumn() =>
        AssociatedObject switch
        {
            DataGridColumnHeader dgHeader => dgHeader.Column,
            GridViewColumnHeader gvHeader => gvHeader.Column,
            _ => null
        };

    private async Task ResetAndInitializeForColumnAsync()
    {
        UnsubscribeParentFilterEvents();
        _viewModel = null;
        PropertyName = null;

        TryResolvePropertyName();
        TryResolveParentViewModel();
        await TryInitializeAsync();

        if (_viewModel == null && !string.IsNullOrEmpty(PropertyName) && _initResolveAttempts++ < 12)
        {
            AssociatedObject.Dispatcher.BeginInvoke(
                () => _ = HandleColumnContextChangedAsync(),
                System.Windows.Threading.DispatcherPriority.DataBind);
            return;
        }

        if (_viewModel != null)
            _initResolveAttempts = 0;

        if (_contentInjected && AssociatedObject is ContentControl header && header.Content is DockPanel dock)
            UpdateDockPanelForCurrentColumn(dock);
    }

    private DependencyObject? FindFilterGridHost()
    {
        var parent = VisualTreeHelper.GetParent(AssociatedObject);
        while (parent != null)
        {
            if (parent is DataGrid or ListView or GridView)
                return parent;

            parent = VisualTreeHelper.GetParent(parent);
        }

        return null;
    }

    private bool IsColumnFilteringEnabled() =>
        ColumnFilterHeaderSettings.IsColumnFilteringEnabled(
            GetAssociatedColumn() as DependencyObject,
            AssociatedObject,
            FindFilterGridHost());

    private ColumnFilterTriggerMode GetEffectiveTriggerMode() =>
        ColumnFilterHeaderSettings.GetEffectiveTriggerMode(
            GetAssociatedColumn() as DependencyObject,
            FindFilterGridHost());

    private void TryResolvePropertyName()
    {
        if (!IsColumnFilteringEnabled())
        {
            PropertyName = string.Empty;
            return;
        }

        // Implementation for DataGrid
        if (AssociatedObject is DataGridColumnHeader dgHeader && dgHeader.Column != null)
        {
            // Check if column explicitly disabled filtering
            var isFilterableObj = dgHeader.Column.ReadLocalValue(IsFilterableProperty);
            if (isFilterableObj is bool isF && isFilterableObj != DependencyProperty.UnsetValue && !isF)
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
            if (isFilterableObj is bool isF && isFilterableObj != DependencyProperty.UnsetValue && !isF)
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

        if (_viewModel is INotifyPropertyChanged inpc)
        {
            inpc.PropertyChanged -= OnColumnFilterVmPropertyChanged;
            inpc.PropertyChanged += OnColumnFilterVmPropertyChanged;
        }

        SubscribeParentFilterEvents(parentVm);
    }

    private void OnColumnFilterVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ColumnFilterVm.IsFilterActive))
            ApplyHeaderFilterBorder();
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
        ApplyHeaderFilterBorder();
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
        if (_viewModel == null || !IsColumnFilteringEnabled()) return;

        if (_contentInjected && AssociatedObject is ContentControl existingHeader && existingHeader.Content is DockPanel existingDock)
        {
            UpdateDockPanelForCurrentColumn(existingDock);
            return;
        }

        if (_contentInjected) return;
        _contentInjected = true;

        ContentControl? header = AssociatedObject as ContentControl;
        if (header == null) return;

        _savedHeaderContentTemplate = header.ContentTemplate;
        _savedHeaderContentTemplateSelector = header.ContentTemplateSelector;
        _trackedColumn = GetAssociatedColumn();

        var dockPanel = new DockPanel
        {
            LastChildFill = true,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        ApplyTriggerModeToHeader(dockPanel);

        var contentPresenter = CreateHeaderContentPresenter();
        dockPanel.Children.Add(contentPresenter);

        // Prevent the column header ContentTemplate from binding to the DockPanel (shows type name).
        header.ContentTemplate = null;
        header.ContentTemplateSelector = null;
        header.Content = dockPanel;
    }

    private void ApplyNativeSortPolicy()
    {
        if (GetAssociatedColumn() is DataGridColumn dgColumn)
        {
            dgColumn.CanUserSort = !ColumnFilterHeaderOptions.SuppressesNativeColumnSort(GetEffectiveTriggerMode());
        }
    }

    private ContentPresenter CreateHeaderContentPresenter()
    {
        var contentPresenter = new ContentPresenter
        {
            ContentTemplate = _savedHeaderContentTemplate,
            ContentTemplateSelector = _savedHeaderContentTemplateSelector,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        ApplyHeaderContentBinding(contentPresenter);
        return contentPresenter;
    }

    private void ApplyHeaderContentBinding(ContentPresenter contentPresenter)
    {
        BindingOperations.ClearBinding(contentPresenter, ContentPresenter.ContentProperty);

        if (AssociatedObject is DataGridColumnHeader)
        {
            BindingOperations.SetBinding(
                contentPresenter,
                ContentPresenter.ContentProperty,
                new Binding
                {
                    RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataGridColumnHeader), 1),
                    Path = new PropertyPath("Column.Header"),
                    Mode = BindingMode.OneWay,
                });
            return;
        }

        if (AssociatedObject is GridViewColumnHeader)
        {
            BindingOperations.SetBinding(
                contentPresenter,
                ContentPresenter.ContentProperty,
                new Binding
                {
                    RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(GridViewColumnHeader), 1),
                    Path = new PropertyPath("Column.Header"),
                    Mode = BindingMode.OneWay,
                });
        }
    }

    private void UpdateDockPanelForCurrentColumn(DockPanel dockPanel)
    {
        if (_viewModel == null)
            return;

        var contentPresenter = dockPanel.Children.OfType<ContentPresenter>().FirstOrDefault();
        if (contentPresenter != null)
            ApplyHeaderContentBinding(contentPresenter);

        ApplyTriggerModeToHeader(dockPanel);
    }

    internal static void RefreshHeaderChrome(FrameworkElement header)
    {
        foreach (var behavior in Interaction.GetBehaviors(header).OfType<FilterableColumnHeaderBehavior>())
            behavior.RefreshHeaderChrome();
    }

    private void RefreshHeaderChrome()
    {
        _isPointerOverHeader = AssociatedObject.IsMouseOver;

        if (_filterPopup is { IsOpen: true })
            _filterPopup.IsOpen = false;

        ApplyNativeSortPolicy();

        if (_contentInjected && AssociatedObject is ContentControl { Content: DockPanel dockPanel })
        {
            if (!IsColumnFilteringEnabled())
            {
                DetachHeaderTriggerHandlers();
                if (_filterButton != null)
                    _filterButton.Visibility = Visibility.Collapsed;
                ApplyHeaderFilterBorder();
                return;
            }

            UpdateDockPanelForCurrentColumn(dockPanel);
            return;
        }

        if (_viewModel != null && IsColumnFilteringEnabled())
            BuildHeaderContent();
    }

    /// <summary>
    /// Opens/closes the filter popup. Loads distinct values first, then restores selection from the parent
    /// grid context so reopening a filtered column matches the applied filter. No deferred <c>IsOpen</c> or
    /// forced close before open (those broke opening another column’s popup after filtering a first one).
    /// </summary>
    private async void OnFilterButtonClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        await ToggleFilterPopupAsync();
    }

    private async void OnHeaderRightClick(object sender, MouseButtonEventArgs e)
    {
        if (GetEffectiveTriggerMode() != ColumnFilterTriggerMode.HeaderRightClick)
            return;

        e.Handled = true;
        await ToggleFilterPopupAsync();
    }

    private async void OnHeaderLeftClick(object sender, MouseButtonEventArgs e)
    {
        if (GetEffectiveTriggerMode() != ColumnFilterTriggerMode.HeaderLeftClick)
            return;

        e.Handled = true;
        await ToggleFilterPopupAsync();
    }

    private async void OnHeaderDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (GetEffectiveTriggerMode() != ColumnFilterTriggerMode.HeaderDoubleClick)
            return;

        e.Handled = true;
        await ToggleFilterPopupAsync();
    }

    private async void OnHeaderMiddleClick(object sender, MouseButtonEventArgs e)
    {
        if (GetEffectiveTriggerMode() != ColumnFilterTriggerMode.HeaderMiddleClick
            || e.ChangedButton != MouseButton.Middle)
        {
            return;
        }

        e.Handled = true;
        await ToggleFilterPopupAsync();
    }

    private async Task ToggleFilterPopupAsync()
    {
        if (_viewModel == null || !IsColumnFilteringEnabled())
            return;

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

            var gridHost = FindFilterGridHost();
            if (gridHost != null)
            {
                ColumnFilterPopupCoordinator.Instance.NotifyOpened(
                    gridHost,
                    this,
                    CloseFilterPopup);
            }

            _filterPopup.PlacementTarget = GetPopupPlacementTarget();
            _filterPopup.IsOpen = true;
        }
    }

    internal void CloseFilterPopup()
    {
        if (_filterPopup is { IsOpen: true })
            _filterPopup.IsOpen = false;
    }

    internal FrameworkElement? AssociatedHeader => AssociatedObject;

    internal UIElement? TryGetOpenPopupChild() =>
        _filterPopup is { IsOpen: true, Child: UIElement child } ? child : null;

    private FrameworkElement GetPopupPlacementTarget()
    {
        var mode = GetEffectiveTriggerMode();
        if (ColumnFilterHeaderOptions.UsesFilterButtonChrome(mode) && _filterButton is { Visibility: Visibility.Visible })
            return _filterButton;

        return AssociatedObject;
    }

    private void BuildPopup()
    {
        _filterPopup = new Popup
        {
            StaysOpen = true,
            AllowsTransparency = true,
            PlacementTarget = GetPopupPlacementTarget(),
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

        filterControl.CancelRequested += (_, _) => _filterPopup.IsOpen = false;
        _viewModel!.OnApply += (_, _) => _filterPopup.IsOpen = false;
        _viewModel.OnClear += (_, _) => _filterPopup.IsOpen = false;

        // Subscribe once per open, unsubscribe on Closed — otherwise handlers stack on every open
        // while Apply/Clear never removed them, breaking subsequent filter button clicks.
        _filterPopup.Opened += OnFilterPopupOpened;
        _filterPopup.Closed += OnFilterPopupClosed;
    }

    private CustomPopupPlacement[] PlaceFilterPopup(Size popupSize, Size targetSize, Point offset)
    {
        var placementTarget = GetPopupPlacementTarget();
        if (placementTarget == null)
            return new[] { new CustomPopupPlacement(new Point(0, targetSize.Height), PopupPrimaryAxis.Horizontal) };

        bool isRtl = placementTarget.FlowDirection == FlowDirection.RightToLeft;
        // Default anchor rule:
        // - LTR: popup top-left at button bottom-right
        // - RTL: popup top-right at button bottom-left (=> top-left at -popupWidth, bottom)
        var desiredRelative = new Point(isRtl ? -popupSize.Width : targetSize.Width, targetSize.Height);

        // Clamp to keep as visible as possible within the window (goal: stay visible in the app window).
        // Callback expects offsets in DIPs relative to target.
        var window = Window.GetWindow(placementTarget);
        if (window == null || window.ActualWidth <= 0 || window.ActualHeight <= 0)
            return new[] { new CustomPopupPlacement(desiredRelative, PopupPrimaryAxis.Horizontal) };

        const double margin = 8;
        // Work purely in DIPs. The callback expects offsets in DIPs relative to target, so mixing
        // screen pixels (PointToScreen) with Window.Left/Top (DIPs) can push the popup offscreen.
        var targetInWindow = placementTarget.TranslatePoint(new Point(0, 0), window);
        var desiredInWindow = new Point(targetInWindow.X + desiredRelative.X, targetInWindow.Y + desiredRelative.Y);

        var minX = margin;
        var maxX = Math.Max(minX, window.ActualWidth - popupSize.Width - margin);
        var minY = margin;
        var maxY = Math.Max(minY, window.ActualHeight - popupSize.Height - margin);

        var clampedX = desiredInWindow.X < minX ? minX : (desiredInWindow.X > maxX ? maxX : desiredInWindow.X);
        var clampedY = desiredInWindow.Y < minY ? minY : (desiredInWindow.Y > maxY ? maxY : desiredInWindow.Y);

        var adjustedRelative = new Point(clampedX - targetInWindow.X, clampedY - targetInWindow.Y);
        return new[] { new CustomPopupPlacement(adjustedRelative, PopupPrimaryAxis.Horizontal) };
    }

    private void OnFilterPopupOpened(object? sender, EventArgs e)
    {
        // Ensure the popup cannot exceed the current monitor working area. Without this, the measured
        // popup size can be larger than small CI desktops (e.g. 1024x720), and placement clamping
        // alone cannot keep the window inside the working area.
        if (_filterPopup is { Child: FrameworkElement child } && GetPopupPlacementTarget() is { } placementTarget)
        {
            try
            {
                const double margin = 8;

                // Use WPF's WorkArea to avoid WinForms dependency in this project.
                // Note: SystemParameters.WorkArea is in DIPs.
                var wa = SystemParameters.WorkArea;
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

        FilterColumnPopupTracker.OnPopupOpened(this);
    }

    private void OnFilterPopupClosed(object? sender, EventArgs e)
    {
        FilterColumnPopupTracker.OnPopupClosed(this);

        var gridHost = FindFilterGridHost();
        if (gridHost != null)
            ColumnFilterPopupCoordinator.Instance.NotifyClosed(gridHost, this);

        if (_cultureBeforePopup != null)
        {
            LocalizationManager.Instance.SetCulture(_cultureBeforePopup);
            _cultureBeforePopup = null;
        }

        ApplyHoverRevealButtonVisibility();
    }
}
