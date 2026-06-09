using DataFilter.Localization;
using DataFilter.PlatformShared.ColumnFilter;
using DataFilter.PlatformShared.ViewModels;
using DataFilter.WinUI3.Behaviors;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using Windows.System;
namespace DataFilter.WinUI3.Attach;

internal static class ListViewFilterHeaderInteractions
{
    private static readonly List<WeakReference<Flyout>> OpenFlyouts = new();

    private static void CloseOpenFlyouts()
    {
        foreach (var reference in OpenFlyouts.ToArray())
        {
            if (reference.TryGetTarget(out var flyout))
                flyout.Hide();
        }

        OpenFlyouts.Clear();
    }

    private static void TrackFlyout(Flyout flyout)
    {
        OpenFlyouts.RemoveAll(r => !r.TryGetTarget(out _));
        OpenFlyouts.Add(new WeakReference<Flyout>(flyout));
        flyout.Closed += (_, _) =>
        {
            OpenFlyouts.RemoveAll(r => r.TryGetTarget(out var f) && ReferenceEquals(f, flyout));
        };
    }

    internal sealed class ColumnSpec
    {
        public required string Title { get; init; }
        public required string PropertyName { get; init; }
        public double Width { get; init; } = 150;
        public bool IsFilterable { get; init; } = true;
        public ColumnFilterTriggerMode TriggerMode { get; init; } = ColumnFilterTriggerMode.Inherit;
    }

    internal sealed class Settings
    {
        public bool AreColumnFiltersEnabled { get; init; } = true;
        public ColumnFilterTriggerMode ColumnFilterTriggerMode { get; init; } = ColumnFilterTriggerMode.FilterButton;
    }

    internal static bool IsColumnFilteringEnabled(ColumnSpec column, Settings settings) =>
        ColumnFilterHeaderOptions.IsFilteringEnabled(settings.AreColumnFiltersEnabled, column.IsFilterable);

    internal static ColumnFilterTriggerMode GetEffectiveTriggerMode(ColumnSpec column, Settings settings) =>
        ColumnFilterHeaderOptions.ResolveTriggerMode(settings.ColumnFilterTriggerMode, column.TriggerMode);

    internal static UIElement BuildHeader(IFilterableDataGridViewModel viewModel, IReadOnlyList<ColumnSpec> columns, Settings settings)
    {
        var headerScroll = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled
        };

        var grid = new Grid { Padding = new Thickness(10, 0, 10, 10) };
        foreach (var c in columns)
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(c.Width) });

        var borderByProperty = new Dictionary<string, Border>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < columns.Count; i++)
        {
            var col = columns[i];
            if (!IsColumnFilteringEnabled(col, settings))
            {
                var label = new TextBlock { Text = col.Title, VerticalAlignment = VerticalAlignment.Center, Padding = new Thickness(5) };
                Grid.SetColumn(label, i);
                grid.Children.Add(label);
                continue;
            }

            var mode = GetEffectiveTriggerMode(col, settings);
            var columnKey = SanitizeForId(col.PropertyName);
            var headerCell = CreateHeaderCell(viewModel, col, mode, columnKey, borderByProperty);
            Grid.SetColumn(headerCell, i);
            grid.Children.Add(headerCell);
        }

        void RefreshBorders()
        {
            foreach (var (propertyName, indicator) in borderByProperty)
            {
                var mode = GetEffectiveTriggerMode(
                    columns.First(c => string.Equals(c.PropertyName, propertyName, StringComparison.OrdinalIgnoreCase)),
                    settings);
                bool active = ColumnFilterHeaderOptions.ShowsFilteredColumnInnerIndicator(
                    mode,
                    ColumnFilterHeaderOptions.IsColumnFilterActive(viewModel, propertyName));
                indicator.Visibility = active ? Visibility.Visible : Visibility.Collapsed;
            }

            grid.InvalidateMeasure();
        }

        viewModel.FilterDescriptorsChanged += (_, _) => RefreshBorders();
        RefreshBorders();

        headerScroll.Content = grid;
        return headerScroll;
    }

    private static FrameworkElement CreateHeaderCell(
        IFilterableDataGridViewModel viewModel,
        ColumnSpec col,
        ColumnFilterTriggerMode mode,
        string? columnKey,
        Dictionary<string, Border> borderByProperty)
    {
        if (ColumnFilterHeaderOptions.UsesAlwaysVisibleFilterButton(mode))
            return CreateFilterButton(viewModel, col, columnKey);

        FrameworkElement content;
        if (!ColumnFilterHeaderOptions.HasHeaderFilterTrigger(mode))
        {
            content = new TextBlock { Text = col.Title, VerticalAlignment = VerticalAlignment.Center };
        }
        else
        {
            var host = new Grid { Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent) };
            host.Children.Add(new TextBlock { Text = col.Title, VerticalAlignment = VerticalAlignment.Center });

            if (ColumnFilterHeaderOptions.UsesHoverRevealFilterButton(mode))
            {
                var hoverReveal = new HoverRevealButtonController(
                    CreateInlineFilterButton(viewModel, col, columnKey, wireDefaultClick: false),
                    host);
                host.Children.Add(hoverReveal.Button);
                hoverReveal.Attach();
                hoverReveal.Button.Click += (_, _) =>
                    ShowFilterFlyout(viewModel, col.PropertyName, host, columnKey, hoverReveal);
            }

            AttachTriggers(viewModel, col, mode, columnKey, host);
            content = host;
        }

        if (!ColumnFilterHeaderOptions.ShowsFilterStateOnHeaderBorder(mode))
            return WrapContent(content, new Thickness(5));

        return WrapWithFilterIndicator(WrapContent(content, new Thickness(4)), col.PropertyName, borderByProperty);
    }

    private static Border WrapContent(FrameworkElement content, Thickness padding)
    {
        var border = new Border { Padding = padding, Child = content };
        return border;
    }

    private static Grid WrapWithFilterIndicator(
        FrameworkElement content,
        string propertyName,
        Dictionary<string, Border> indicatorByProperty)
    {
        var container = new Grid();
        container.Children.Add(content);

        var indicator = new Border
        {
            BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.DodgerBlue),
            BorderThickness = new Thickness(2),
            IsHitTestVisible = false,
            Visibility = Visibility.Collapsed,
        };
        container.Children.Add(indicator);
        indicatorByProperty[propertyName] = indicator;
        return container;
    }

    private static Button CreateFilterButton(IFilterableDataGridViewModel viewModel, ColumnSpec col, string? columnKey)
    {
        var btn = new Button
        {
            Content = $"{col.Title} \uD83D\uDD0D",
            HorizontalAlignment = HorizontalAlignment.Left,
            Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
            Padding = new Thickness(5)
        };
        if (!string.IsNullOrWhiteSpace(columnKey))
            AutomationProperties.SetAutomationId(btn, $"df-filter-btn-{columnKey}");

        btn.Click += (_, _) => ShowFilterFlyout(viewModel, col.PropertyName, btn, columnKey);
        return btn;
    }

    private static Button CreateInlineFilterButton(
        IFilterableDataGridViewModel viewModel,
        ColumnSpec col,
        string? columnKey,
        bool wireDefaultClick = true)
    {
        var btn = new Button
        {
            Content = "\uD83D\uDD0D",
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
            Padding = new Thickness(4),
            MinWidth = 24,
            MinHeight = 20,
        };
        if (!string.IsNullOrWhiteSpace(columnKey))
            AutomationProperties.SetAutomationId(btn, $"df-filter-btn-{columnKey}");

        if (wireDefaultClick)
            btn.Click += (_, _) => ShowFilterFlyout(viewModel, col.PropertyName, btn, columnKey);

        return btn;
    }

    private static void AttachTriggers(
        IFilterableDataGridViewModel viewModel,
        ColumnSpec col,
        ColumnFilterTriggerMode mode,
        string? columnKey,
        Grid host)
    {
        switch (mode)
        {
            case ColumnFilterTriggerMode.HeaderLeftClick:
                host.PointerPressed += (_, e) => OnHeaderLeftPointerPressed(viewModel, col, host, columnKey, e);
                break;
            case ColumnFilterTriggerMode.HeaderRightClick:
                host.RightTapped += (_, _) => ShowFilterFlyout(viewModel, col.PropertyName, host, columnKey);
                break;
            case ColumnFilterTriggerMode.HeaderDoubleClick:
                host.DoubleTapped += (_, _) => ShowFilterFlyout(viewModel, col.PropertyName, host, columnKey);
                break;
            case ColumnFilterTriggerMode.HeaderMiddleClick:
                host.PointerPressed += (_, e) => OnHeaderMiddlePointerPressed(viewModel, col, host, columnKey, e);
                break;
            case ColumnFilterTriggerMode.ShiftClick:
                host.PointerPressed += (_, e) => OnModifierPointerPressed(viewModel, col, host, columnKey, e, shift: true, ctrl: false);
                break;
            case ColumnFilterTriggerMode.CtrlClick:
                host.PointerPressed += (_, e) => OnModifierPointerPressed(viewModel, col, host, columnKey, e, shift: false, ctrl: true);
                break;
            case ColumnFilterTriggerMode.ContextMenuFilter:
            {
                var menu = new MenuFlyout();
                var item = new MenuFlyoutItem { Text = LocalizationManager.Instance["OpenColumnFilter"] };
                item.Click += (_, _) => ShowFilterFlyout(viewModel, col.PropertyName, host, columnKey);
                menu.Items.Add(item);
                host.ContextFlyout = menu;
                break;
            }
            case ColumnFilterTriggerMode.KeyboardShortcut:
                host.IsTabStop = true;
                host.KeyDown += (_, e) =>
                {
                    if (e.Key == VirtualKey.Down && Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Menu).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
                    {
                        ShowFilterFlyout(viewModel, col.PropertyName, host, columnKey);
                        e.Handled = true;
                    }
                };
                break;
            case ColumnFilterTriggerMode.HeaderLongPress:
            {
                var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(ColumnFilterHeaderChrome.LongPressDurationMs) };
                timer.Tick += (_, _) =>
                {
                    timer.Stop();
                    ShowFilterFlyout(viewModel, col.PropertyName, host, columnKey);
                };
                host.PointerPressed += (_, _) => timer.Start();
                host.PointerReleased += (_, _) => timer.Stop();
                host.PointerCanceled += (_, _) => timer.Stop();
                break;
            }
            case ColumnFilterTriggerMode.HoverRevealButton:
                break;
        }
    }

    private static void OnHeaderLeftPointerPressed(
        IFilterableDataGridViewModel viewModel,
        ColumnSpec col,
        FrameworkElement anchor,
        string? columnKey,
        PointerRoutedEventArgs e)
    {
        var props = e.GetCurrentPoint(anchor).Properties;
        if (props.IsRightButtonPressed || props.IsMiddleButtonPressed)
            return;

        ShowFilterFlyout(viewModel, col.PropertyName, anchor, columnKey);
        e.Handled = true;
    }

    private static void OnHeaderMiddlePointerPressed(
        IFilterableDataGridViewModel viewModel,
        ColumnSpec col,
        FrameworkElement anchor,
        string? columnKey,
        PointerRoutedEventArgs e)
    {
        if (!e.GetCurrentPoint(anchor).Properties.IsMiddleButtonPressed)
            return;

        ShowFilterFlyout(viewModel, col.PropertyName, anchor, columnKey);
        e.Handled = true;
    }

    private static void OnModifierPointerPressed(
        IFilterableDataGridViewModel viewModel,
        ColumnSpec col,
        FrameworkElement anchor,
        string? columnKey,
        PointerRoutedEventArgs e,
        bool shift,
        bool ctrl)
    {
        var props = e.GetCurrentPoint(anchor).Properties;
        if (!props.IsLeftButtonPressed)
            return;

        bool shiftDown = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
        bool ctrlDown = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
        if (shift && !shiftDown || ctrl && !ctrlDown)
            return;

        ShowFilterFlyout(viewModel, col.PropertyName, anchor, columnKey);
        e.Handled = true;
    }

    private static void ShowFilterFlyout(
        IFilterableDataGridViewModel viewModel,
        string propertyName,
        FrameworkElement anchor,
        string? columnKey,
        HoverRevealButtonController? hoverReveal = null)
    {
        var popup = FilterHeaderBehavior.CreatePopup(viewModel, propertyName);
        if (!string.IsNullOrWhiteSpace(columnKey))
            AutomationProperties.SetAutomationId(popup, $"df-filter-popup-{columnKey}");

        CloseOpenFlyouts();

        var flyout = new Flyout
        {
            Content = popup,
            LightDismissOverlayMode = LightDismissOverlayMode.On,
        };
        if (popup.ViewModel != null)
        {
            popup.ViewModel.OnApply += (_, __) => flyout.Hide();
            popup.ViewModel.OnClear += (_, __) => flyout.Hide();
            popup.CancelRequested += (_, __) => flyout.Hide();
        }

        TrackFlyout(flyout);
        flyout.Opened += (_, _) => hoverReveal?.OnFlyoutOpened();
        flyout.Closed += (_, _) => hoverReveal?.OnFlyoutClosed();

        bool isRtl = anchor.FlowDirection == FlowDirection.RightToLeft;
        var desired = new Point(isRtl ? -popup.Width : anchor.ActualWidth, anchor.ActualHeight);
        flyout.ShowAt(anchor, new FlyoutShowOptions
        {
            Placement = FlyoutPlacementMode.Bottom,
            Position = desired
        });
    }

    private sealed class HoverRevealButtonController
    {
        public Button Button { get; }
        private readonly Grid _host;
        private bool _isPointerOverHost;
        private bool _isFlyoutOpen;

        public HoverRevealButtonController(Button button, Grid host)
        {
            Button = button;
            _host = host;
            Button.Visibility = Visibility.Collapsed;
        }

        public void Attach()
        {
            _host.PointerEntered += (_, _) =>
            {
                _isPointerOverHost = true;
                UpdateVisibility();
            };
            _host.PointerExited += (_, _) =>
            {
                _isPointerOverHost = false;
                UpdateVisibility();
            };
        }

        public void OnFlyoutOpened()
        {
            _isFlyoutOpen = true;
            UpdateVisibility();
        }

        public void OnFlyoutClosed()
        {
            _isFlyoutOpen = false;
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            Button.Visibility = _isPointerOverHost || _isFlyoutOpen
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }

    private static string? SanitizeForId(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        raw = raw.Trim();
        Span<char> buf = stackalloc char[raw.Length];
        int n = 0;
        foreach (var ch in raw)
        {
            if ((ch >= 'a' && ch <= 'z') ||
                (ch >= 'A' && ch <= 'Z') ||
                (ch >= '0' && ch <= '9') ||
                ch == '_' || ch == '-')
            {
                buf[n++] = ch;
            }
        }

        return n == 0 ? null : new string(buf[..n]);
    }
}
