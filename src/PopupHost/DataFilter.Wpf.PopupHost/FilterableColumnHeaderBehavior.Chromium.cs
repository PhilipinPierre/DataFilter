using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using DataFilter.Localization;
using DataFilter.PlatformShared.ColumnFilter;
using ColumnFilterVm = DataFilter.Wpf.ViewModels.ColumnFilterViewModel;

namespace DataFilter.Wpf.Behaviors;

public partial class FilterableColumnHeaderBehavior
{
    private void ApplyTriggerModeToHeader(DockPanel dockPanel)
    {
        DetachHeaderTriggerHandlers();
        ApplyNativeSortPolicy();
        ApplyHeaderFilterBorder();

        var triggerMode = GetEffectiveTriggerMode();
        if (!ColumnFilterHeaderOptions.HasHeaderFilterTrigger(triggerMode))
        {
            if (_filterButton != null)
                _filterButton.Visibility = Visibility.Collapsed;
            return;
        }

        if (ColumnFilterHeaderOptions.UsesFilterButtonChrome(triggerMode))
        {
            EnsureFilterButton(dockPanel);
            _filterButton!.Visibility = triggerMode == ColumnFilterTriggerMode.HoverRevealButton
                ? Visibility.Collapsed
                : Visibility.Visible;
            if (triggerMode == ColumnFilterTriggerMode.HoverRevealButton)
            {
                AssociatedObject.MouseEnter += OnHeaderMouseEnter;
                AssociatedObject.MouseLeave += OnHeaderMouseLeave;
            }

            return;
        }

        if (_filterButton != null)
            _filterButton.Visibility = Visibility.Collapsed;

        switch (triggerMode)
        {
            case ColumnFilterTriggerMode.HeaderRightClick:
                AssociatedObject.PreviewMouseRightButtonUp += OnHeaderRightClick;
                break;
            case ColumnFilterTriggerMode.HeaderLeftClick:
                AssociatedObject.PreviewMouseLeftButtonDown += OnHeaderLeftClick;
                break;
            case ColumnFilterTriggerMode.HeaderDoubleClick:
                if (AssociatedObject is Control headerControl)
                    headerControl.MouseDoubleClick += OnHeaderDoubleClick;
                break;
            case ColumnFilterTriggerMode.HeaderMiddleClick:
                AssociatedObject.PreviewMouseDown += OnHeaderMiddleClick;
                break;
            case ColumnFilterTriggerMode.ShiftClick:
            case ColumnFilterTriggerMode.CtrlClick:
                AssociatedObject.PreviewMouseLeftButtonDown += OnHeaderModifierClick;
                break;
            case ColumnFilterTriggerMode.ContextMenuFilter:
                EnsureHeaderContextMenu();
                break;
            case ColumnFilterTriggerMode.KeyboardShortcut:
                AssociatedObject.Focusable = true;
                AssociatedObject.KeyDown += OnHeaderKeyDown;
                break;
            case ColumnFilterTriggerMode.HeaderLongPress:
                AssociatedObject.PreviewMouseLeftButtonDown += OnLongPressMouseDown;
                AssociatedObject.PreviewMouseLeftButtonUp += OnLongPressMouseUp;
                break;
        }
    }

    private void EnsureFilterButton(DockPanel dockPanel)
    {
        if (_filterButton == null)
        {
            _filterButton = new Controls.ColumnFilterButton
            {
                Margin = new Thickness(4, 0, 0, 0),
                MinWidth = 14,
                MinHeight = 14,
                VerticalAlignment = VerticalAlignment.Center,
                Cursor = Cursors.Hand
            };
            _filterButton.Click += OnFilterButtonClick;
        }

        if (!string.IsNullOrWhiteSpace(PropertyName))
            AutomationProperties.SetAutomationId(_filterButton, $"df-filter-btn-{PropertyName}");

        _filterButton.SetBinding(
            Controls.ColumnFilterButton.IsActiveProperty,
            new System.Windows.Data.Binding(nameof(ColumnFilterVm.IsFilterActive)) { Source = _viewModel });

        if (!dockPanel.Children.Contains(_filterButton))
        {
            DockPanel.SetDock(_filterButton, Dock.Right);
            dockPanel.Children.Insert(0, _filterButton);
        }
    }

    private void EnsureHeaderContextMenu()
    {
        _headerContextMenu ??= new ContextMenu();
        if (_headerContextMenu.Items.Count == 0)
        {
            var item = new MenuItem { Header = LocalizationManager.Instance["OpenColumnFilter"] };
            item.Click += async (_, _) => await ToggleFilterPopupAsync();
            _headerContextMenu.Items.Add(item);
        }

        AssociatedObject.ContextMenu = _headerContextMenu;
    }

    private void ApplyHeaderFilterBorder()
    {
        if (AssociatedObject is Control control)
        {
            control.ClearValue(Control.BorderThicknessProperty);
            control.ClearValue(Control.BorderBrushProperty);
        }

        bool showIndicator = ColumnFilterHeaderOptions.ShowsFilteredColumnInnerIndicator(
            GetEffectiveTriggerMode(),
            _viewModel?.IsFilterActive == true);

        if (!showIndicator || AssociatedObject is not FrameworkElement element)
        {
            RemoveFilterStateIndicator();
            return;
        }

        EnsureFilterStateIndicator(element);
    }

    private void EnsureFilterStateIndicator(FrameworkElement element)
    {
        if (_filterStateIndicatorAdorner != null)
        {
            _filterStateIndicatorAdorner.InvalidateVisual();
            return;
        }

        var layer = AdornerLayer.GetAdornerLayer(element);
        if (layer == null)
            return;

        _filterStateIndicatorAdorner = new FilterStateIndicatorAdorner(element);
        layer.Add(_filterStateIndicatorAdorner);
    }

    private void RemoveFilterStateIndicator()
    {
        if (_filterStateIndicatorAdorner == null)
            return;

        var layer = AdornerLayer.GetAdornerLayer(_filterStateIndicatorAdorner.AdornedElement);
        layer?.Remove(_filterStateIndicatorAdorner);
        _filterStateIndicatorAdorner = null;
    }

    private void OnHeaderMouseEnter(object sender, MouseEventArgs e)
    {
        if (_filterButton != null && GetEffectiveTriggerMode() == ColumnFilterTriggerMode.HoverRevealButton)
            _filterButton.Visibility = Visibility.Visible;
    }

    private void OnHeaderMouseLeave(object sender, MouseEventArgs e)
    {
        if (_filterButton != null && GetEffectiveTriggerMode() == ColumnFilterTriggerMode.HoverRevealButton && _filterPopup is not { IsOpen: true })
            _filterButton.Visibility = Visibility.Collapsed;
    }

    private async void OnHeaderModifierClick(object sender, MouseButtonEventArgs e)
    {
        var mode = GetEffectiveTriggerMode();
        if (mode == ColumnFilterTriggerMode.ShiftClick && (Keyboard.Modifiers & ModifierKeys.Shift) == 0)
            return;
        if (mode == ColumnFilterTriggerMode.CtrlClick && (Keyboard.Modifiers & ModifierKeys.Control) == 0)
            return;
        if (mode is not (ColumnFilterTriggerMode.ShiftClick or ColumnFilterTriggerMode.CtrlClick))
            return;

        e.Handled = true;
        await ToggleFilterPopupAsync();
    }

    private async void OnHeaderKeyDown(object sender, KeyEventArgs e)
    {
        if (GetEffectiveTriggerMode() != ColumnFilterTriggerMode.KeyboardShortcut)
            return;

        if (e.Key == Key.Down && (Keyboard.Modifiers & ModifierKeys.Alt) != 0)
        {
            e.Handled = true;
            await ToggleFilterPopupAsync();
        }
    }

    private void OnLongPressMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (GetEffectiveTriggerMode() != ColumnFilterTriggerMode.HeaderLongPress)
            return;

        _longPressTimer?.Stop();
        _longPressTimer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(ColumnFilterHeaderChrome.LongPressDurationMs)
        };
        _longPressTimer.Tick += async (_, _) =>
        {
            _longPressTimer?.Stop();
            await ToggleFilterPopupAsync();
        };
        _longPressTimer.Start();
    }

    private void OnLongPressMouseUp(object sender, MouseButtonEventArgs e)
    {
        _longPressTimer?.Stop();
    }

    private void DetachHeaderTriggerHandlers()
    {
        RemoveFilterStateIndicator();
        AssociatedObject.PreviewMouseRightButtonUp -= OnHeaderRightClick;
        AssociatedObject.PreviewMouseLeftButtonDown -= OnHeaderLeftClick;
        AssociatedObject.PreviewMouseLeftButtonDown -= OnHeaderModifierClick;
        AssociatedObject.PreviewMouseLeftButtonDown -= OnLongPressMouseDown;
        AssociatedObject.PreviewMouseLeftButtonUp -= OnLongPressMouseUp;
        if (AssociatedObject is Control headerControl)
            headerControl.MouseDoubleClick -= OnHeaderDoubleClick;
        AssociatedObject.PreviewMouseDown -= OnHeaderMiddleClick;
        AssociatedObject.KeyDown -= OnHeaderKeyDown;
        AssociatedObject.MouseEnter -= OnHeaderMouseEnter;
        AssociatedObject.MouseLeave -= OnHeaderMouseLeave;
        AssociatedObject.ContextMenu = null;
        _longPressTimer?.Stop();
    }

    private sealed class FilterStateIndicatorAdorner : Adorner
    {
        private static readonly Pen IndicatorPen = CreateIndicatorPen();

        public FilterStateIndicatorAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            IsHitTestVisible = false;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var size = AdornedElement.RenderSize;
            if (size.Width <= 0 || size.Height <= 0)
                return;

            const double inset = 1;
            var rect = new Rect(inset, inset, Math.Max(0, size.Width - inset * 2), Math.Max(0, size.Height - inset * 2));
            drawingContext.DrawRectangle(null, IndicatorPen, rect);
        }

        private static Pen CreateIndicatorPen()
        {
            var pen = new Pen(Brushes.DodgerBlue, 2);
            pen.Freeze();
            return pen;
        }
    }
}
