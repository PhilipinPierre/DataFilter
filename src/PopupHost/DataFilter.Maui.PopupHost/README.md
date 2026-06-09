# DataFilter.Maui.PopupHost

Hosts and positions the MAUI filter popup (modal overlay page, LTR/RTL anchoring, page clamping).

**Visual customization:** [CUSTOMIZATION.md — MAUI](../../../CUSTOMIZATION.md#maui-datafiltermaui)

## NuGet integration

### Install the packages

```bash
dotnet add package DataFilter.Maui
dotnet add package DataFilter.Maui.PopupHost
```

### Target frameworks

Same as **`DataFilter.Maui`** (`net9.0-android`, `net9.0-ios`, `net9.0-maccatalyst`, `net9.0-windows10.0.19041.0`)

### Dependencies

- `DataFilter.Maui` (transitive: PlatformShared, Core, ExcelLike, Localization)

### Quick start

Use header behaviors and popup services from **`DataFilter.Maui`**; PopupHost provides overlay positioning when opening column filter popups from custom layouts. See **`DataFilter.Maui.Demo`** for attach and local-filter scenarios.

```csharp
// Typical flow: PlatformShared ViewModel + MAUI header behavior opens PopupHost overlay
var vm = new FilterableDataGridViewModel<Employee> { LocalDataSource = items };
await vm.RefreshDataAsync();
```

## What this package contains

Popup **hosting and positioning** only. UI controls remain in **`DataFilter.Maui`**.

## Theming

Popup surfaces follow **`FilterTheme.Current`** on **`FilterPopupView`**. See [CUSTOMIZATION.md — MAUI](../../../CUSTOMIZATION.md#maui-datafiltermaui).
