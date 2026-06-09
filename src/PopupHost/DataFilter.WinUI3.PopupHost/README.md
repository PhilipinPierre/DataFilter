# DataFilter.WinUI3.PopupHost

Hosts and positions the WinUI 3 filter popup (open/close lifecycle, LTR/RTL anchoring).

**Visual customization:** [CUSTOMIZATION.md — WinUI 3](../../../CUSTOMIZATION.md#winui-3-datafilterwinui3)

## NuGet integration

### Install the packages

```bash
dotnet add package DataFilter.WinUI3
dotnet add package DataFilter.WinUI3.PopupHost
```

Requires **Windows App SDK**.

### Target framework

`net8.0-windows10.0.19041.0` (aligned with **`DataFilter.WinUI3`**)

### Dependencies

- `DataFilter.WinUI3` (transitive: PlatformShared, Core, ExcelLike, Localization)

### Quick start

Use **`FilterHeaderBehavior`** from **`DataFilter.WinUI3`** with an **`IFilterableDataGridViewModel`**; PopupHost handles overlay placement when the filter button is clicked.

```csharp
var vm = new FilterableDataGridViewModel<Employee> { LocalDataSource = items };
await vm.RefreshDataAsync();
// Wire FilterableDataGrid or custom grid + FilterHeaderBehavior (see WinUI3 demo)
```

## What this package contains

Popup **hosting and positioning** only. UI controls remain in **`DataFilter.WinUI3`**.

## Theming

Default: WinUI system theme. Optional **`FilterPopupControl.ApplyTheme()`** for explicit **`FilterTheme`** colors — [CUSTOMIZATION.md — WinUI 3](../../../CUSTOMIZATION.md#winui-3-datafilterwinui3).
