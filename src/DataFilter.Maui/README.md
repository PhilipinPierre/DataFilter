# DataFilter.Maui

Native MAUI specialization of DataFilter: filter bar chrome, popup services, and list/grid integration aligned with **DataFilter.PlatformShared**.

**Visual customization:** [CUSTOMIZATION.md — MAUI](../../CUSTOMIZATION.md#maui-datafiltermaui)

## NuGet integration

### Install the packages

```bash
dotnet add package DataFilter.Maui
dotnet add package DataFilter.Maui.PopupHost
```

### Target frameworks

`net9.0-android`, `net9.0-ios`, `net9.0-maccatalyst`, `net9.0-windows10.0.19041.0` (Windows)

### Dependencies (transitive)

`DataFilter.Core`, `DataFilter.Filtering.ExcelLike`, `DataFilter.PlatformShared`, `DataFilter.Localization`

### Quick start

```csharp
using DataFilter.Maui.Controls;
using DataFilter.PlatformShared.ColumnFilter;
using DataFilter.PlatformShared.ViewModels;

var vm = new FilterableDataGridViewModel<Employee>
{
    LocalDataSource = employees
};
await vm.RefreshDataAsync();

var grid = new FilterableDataGrid
{
    ViewModel = vm,
    ItemsSource = vm.FilteredItems,
    AreColumnFiltersEnabled = true,
    ColumnFilterTriggerMode = ColumnFilterTriggerMode.FilterButton,
};
```

Set **`AreColumnFiltersEnabled`** / **`ColumnFilterTriggerMode`** on **`FilterableDataGrid`** or **`ListViewFilterHeaderAdapter`** (see **`DataFilter.PlatformShared.ColumnFilter`**).

Optional active-filters bar — use **`FilterGridChromeView`** with **`ShowFilterBar="True"`** (see **DataFilter.PlatformShared** README).

Pipeline presets:

```csharp
await vm.ApplyFilterPipelineSnapshotAsync(vm.CreateFilterPipelineSnapshot());
```

## Localization

Popup UI texts come from **`DataFilter.Localization`**:

```csharp
using System.Globalization;
using DataFilter.Localization;

LocalizationManager.Instance.SetCulture(new CultureInfo("fr"));
```

Per-grid culture: **`IFilterableDataGridViewModel.CultureOverride`**.

## Theming

Filter UI follows **`FilterTheme.Current`** (PlatformShared). Popups subscribe to **`FilterTheme.CurrentChanged`**.

```csharp
using DataFilter.PlatformShared.Theming;

FilterTheme.Current = FilterTheme.Light.With(
    popupBackground: "#FFFFFF",
    primaryColor: "#512BD4");
```

| API | Role |
|-----|------|
| **`FilterPopupView.ApplyTheme(FilterTheme?)`** | Popup background, OK button, advanced toggle |
| **`FilterGridChromeView`** | Overlay uses `FilterTheme.Current.OverlayBackground` |
| **`FilterThemeApplier.ToMauiColor`** | `#RRGGBB` → `Microsoft.Maui.Graphics.Color` |

Combine with `Application.Current.UserAppTheme` for system light/dark. See [CUSTOMIZATION.md — MAUI](../../CUSTOMIZATION.md#maui-datafiltermaui).
