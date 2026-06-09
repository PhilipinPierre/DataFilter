using DataFilter.Localization;
using DataFilter.Maui.Behaviors;
using DataFilter.PlatformShared.ColumnFilter;
using DataFilter.PlatformShared.ViewModels;
using Microsoft.Maui.Controls;

namespace DataFilter.Maui.Attach;

internal static class ListViewFilterHeaderInteractions
{
    internal sealed class ColumnSpec
    {
        public required string Title { get; init; }
        public required string PropertyName { get; init; }
        public GridLength Width { get; init; } = GridLength.Star;
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

    internal static View BuildHeader(
        Page hostPage,
        IFilterableDataGridViewModel viewModel,
        IReadOnlyList<ColumnSpec> columns,
        Settings settings)
    {
        var grid = new Grid { Padding = new Thickness(10, 0, 10, 10), ColumnSpacing = 6 };
        foreach (var c in columns)
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = c.Width });

        var borderByProperty = new Dictionary<string, Border>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < columns.Count; i++)
        {
            var col = columns[i];
            if (!IsColumnFilteringEnabled(col, settings))
            {
                var label = new Label { Text = col.Title, VerticalOptions = LayoutOptions.Center, Padding = new Thickness(6, 2) };
                Grid.SetColumn(label, i);
                grid.Children.Add(label);
                continue;
            }

            var mode = GetEffectiveTriggerMode(col, settings);
            var columnKey = SanitizeForId(col.PropertyName);
            var headerCell = CreateHeaderCell(hostPage, viewModel, col, mode, columnKey, borderByProperty);
            Grid.SetColumn(headerCell, i);
            grid.Children.Add(headerCell);
        }

        void RefreshBorders()
        {
            foreach (var (propertyName, indicator) in borderByProperty)
            {
                var col = columns.First(c => string.Equals(c.PropertyName, propertyName, StringComparison.OrdinalIgnoreCase));
                var mode = GetEffectiveTriggerMode(col, settings);
                bool active = ColumnFilterHeaderOptions.ShowsFilteredColumnInnerIndicator(
                    mode,
                    ColumnFilterHeaderOptions.IsColumnFilterActive(viewModel, propertyName));
                indicator.IsVisible = active;
            }
        }

        viewModel.FilterDescriptorsChanged += (_, _) => RefreshBorders();
        RefreshBorders();

        return grid;
    }

    private static View CreateHeaderCell(
        Page hostPage,
        IFilterableDataGridViewModel viewModel,
        ColumnSpec col,
        ColumnFilterTriggerMode mode,
        string? columnKey,
        Dictionary<string, Border> borderByProperty)
    {
        if (ColumnFilterHeaderOptions.UsesAlwaysVisibleFilterButton(mode))
            return CreateFilterButton(hostPage, viewModel, col, columnKey);

        View content;
        if (!ColumnFilterHeaderOptions.HasHeaderFilterTrigger(mode))
        {
            content = new Label { Text = col.Title, VerticalOptions = LayoutOptions.Center, Padding = new Thickness(6, 2) };
        }
        else
        {
            var layout = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto },
                },
                Padding = new Thickness(6, 2),
                BackgroundColor = Colors.Transparent,
            };
            layout.Add(new Label { Text = col.Title, VerticalOptions = LayoutOptions.Center }, 0, 0);

            if (ColumnFilterHeaderOptions.UsesHoverRevealFilterButton(mode))
            {
                var hoverButton = CreateInlineFilterButton(hostPage, viewModel, col, columnKey);
                hoverButton.IsVisible = false;
                layout.Add(hoverButton, 1, 0);
                var enter = new PointerGestureRecognizer();
                enter.PointerEntered += (_, _) => hoverButton.IsVisible = true;
                enter.PointerExited += (_, _) => hoverButton.IsVisible = false;
                layout.GestureRecognizers.Add(enter);
            }

            AttachTriggers(hostPage, viewModel, col, mode, columnKey, layout);
            content = layout;
        }

        if (!ColumnFilterHeaderOptions.ShowsFilterStateOnHeaderBorder(mode))
            return content;

        return WrapWithFilterIndicator(content, col.PropertyName, borderByProperty);
    }

    private static Grid WrapWithFilterIndicator(
        View content,
        string propertyName,
        Dictionary<string, Border> indicatorByProperty)
    {
        var container = new Grid();
        container.Add(content);

        var indicator = new Border
        {
            Stroke = Colors.DodgerBlue,
            StrokeThickness = 2,
            IsVisible = false,
            InputTransparent = true,
        };
        container.Add(indicator);
        indicatorByProperty[propertyName] = indicator;
        return container;
    }

    private static Button CreateFilterButton(
        Page hostPage,
        IFilterableDataGridViewModel viewModel,
        ColumnSpec col,
        string? columnKey)
    {
        var btn = new Button
        {
            Text = $"{col.Title} \uD83D\uDD0D",
            BackgroundColor = Colors.Transparent,
            Padding = new Thickness(6, 2),
            HorizontalOptions = LayoutOptions.Start
        };
        if (!string.IsNullOrWhiteSpace(columnKey))
            btn.AutomationId = $"df-filter-btn-{columnKey}";

        btn.Clicked += async (_, _) => await ShowFilterPopupAsync(hostPage, viewModel, col.PropertyName, btn, columnKey);
        return btn;
    }

    private static Button CreateInlineFilterButton(
        Page hostPage,
        IFilterableDataGridViewModel viewModel,
        ColumnSpec col,
        string? columnKey)
    {
        var btn = new Button
        {
            Text = "\uD83D\uDD0D",
            BackgroundColor = Colors.Transparent,
            Padding = new Thickness(4, 0),
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Center,
        };
        if (!string.IsNullOrWhiteSpace(columnKey))
            btn.AutomationId = $"df-filter-btn-{columnKey}";

        btn.Clicked += async (_, _) => await ShowFilterPopupAsync(hostPage, viewModel, col.PropertyName, btn, columnKey);
        return btn;
    }

    private static void AttachTriggers(
        Page hostPage,
        IFilterableDataGridViewModel viewModel,
        ColumnSpec col,
        ColumnFilterTriggerMode mode,
        string? columnKey,
        View target)
    {
        switch (mode)
        {
            case ColumnFilterTriggerMode.HeaderLeftClick:
            {
                var tap = new TapGestureRecognizer();
                tap.Tapped += async (_, _) => await ShowFilterPopupAsync(hostPage, viewModel, col.PropertyName, target, columnKey);
                target.GestureRecognizers.Add(tap);
                break;
            }
            case ColumnFilterTriggerMode.HeaderRightClick:
            {
                var rightTap = new TapGestureRecognizer { Buttons = ButtonsMask.Secondary };
                rightTap.Tapped += async (_, _) => await ShowFilterPopupAsync(hostPage, viewModel, col.PropertyName, target, columnKey);
                target.GestureRecognizers.Add(rightTap);
                break;
            }
            case ColumnFilterTriggerMode.HeaderDoubleClick:
            {
                var doubleTap = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
                doubleTap.Tapped += async (_, _) => await ShowFilterPopupAsync(hostPage, viewModel, col.PropertyName, target, columnKey);
                target.GestureRecognizers.Add(doubleTap);
                break;
            }
            case ColumnFilterTriggerMode.HeaderMiddleClick:
            {
                MauiHeaderPointerHelpers.AttachMiddleClick(
                    target,
                    () => ShowFilterPopupAsync(hostPage, viewModel, col.PropertyName, target, columnKey));
                break;
            }
            case ColumnFilterTriggerMode.ShiftClick:
            {
                var tap = new TapGestureRecognizer { Buttons = ButtonsMask.Primary };
                tap.Tapped += async (_, _) =>
                {
                    if (!MauiHeaderPointerHelpers.IsShiftKeyDown())
                        return;

                    await ShowFilterPopupAsync(hostPage, viewModel, col.PropertyName, target, columnKey);
                };
                target.GestureRecognizers.Add(tap);
                break;
            }
            case ColumnFilterTriggerMode.CtrlClick:
            {
                var tap = new TapGestureRecognizer { Buttons = ButtonsMask.Primary };
                tap.Tapped += async (_, _) =>
                {
                    if (!MauiHeaderPointerHelpers.IsControlKeyDown())
                        return;

                    await ShowFilterPopupAsync(hostPage, viewModel, col.PropertyName, target, columnKey);
                };
                target.GestureRecognizers.Add(tap);
                break;
            }
            case ColumnFilterTriggerMode.ContextMenuFilter:
            {
                var menuTap = new TapGestureRecognizer { Buttons = ButtonsMask.Secondary };
                menuTap.Tapped += async (_, _) =>
                {
                    var action = await hostPage.DisplayActionSheet(
                        LocalizationManager.Instance["OpenColumnFilter"],
                        null,
                        null,
                        LocalizationManager.Instance["OpenColumnFilter"]);
                    if (action == LocalizationManager.Instance["OpenColumnFilter"])
                        await ShowFilterPopupAsync(hostPage, viewModel, col.PropertyName, target, columnKey);
                };
                target.GestureRecognizers.Add(menuTap);
                break;
            }
            case ColumnFilterTriggerMode.KeyboardShortcut:
                MauiHeaderPointerHelpers.AttachKeyboardShortcut(
                    target,
                    () => ShowFilterPopupAsync(hostPage, viewModel, col.PropertyName, target, columnKey));
                break;
            case ColumnFilterTriggerMode.HeaderLongPress:
            {
                IDispatcherTimer? timer = null;
                var down = new PointerGestureRecognizer();
                down.PointerPressed += (_, _) =>
                {
                    timer?.Stop();
                    timer = Application.Current?.Dispatcher.CreateTimer();
                    if (timer == null)
                        return;
                    timer.Interval = TimeSpan.FromMilliseconds(ColumnFilterHeaderChrome.LongPressDurationMs);
                    timer.Tick += async (_, _) =>
                    {
                        timer.Stop();
                        await ShowFilterPopupAsync(hostPage, viewModel, col.PropertyName, target, columnKey);
                    };
                    timer.Start();
                };
                down.PointerReleased += (_, _) => timer?.Stop();
                target.GestureRecognizers.Add(down);
                break;
            }
        }
    }

    private static async Task ShowFilterPopupAsync(
        Page hostPage,
        IFilterableDataGridViewModel viewModel,
        string propertyName,
        VisualElement anchor,
        string? columnKey)
    {
        var popupView = FilterHeaderBehavior.CreatePopup(viewModel, propertyName);
        var anchorRect = GetAbsoluteRect(anchor);
        var page = new FilterPopupPage(popupView, anchorRect, anchor.FlowDirection, columnKey);
        popupView.CloseRequested += async (_, __) =>
        {
            if (page.Navigation.ModalStack.Contains(page))
                await page.Navigation.PopModalAsync();
        };
        page.DismissRequested += async (_, __) =>
        {
            if (page.Navigation.ModalStack.Contains(page))
                await page.Navigation.PopModalAsync();
        };

        await hostPage.Navigation.PushModalAsync(page);
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

    private static Rect GetAbsoluteRect(VisualElement element)
    {
        double x = element.X;
        double y = element.Y;

        var parent = element.Parent as VisualElement;
        while (parent != null)
        {
            x += parent.X;
            y += parent.Y;
            parent = parent.Parent as VisualElement;
        }

        return new Rect(x, y, element.Width, element.Height);
    }
}
