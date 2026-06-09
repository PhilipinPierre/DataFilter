# DataFilter.Wpf.PopupHost

Hosts and positions the WPF filter popup, and provides ready-made filterable grid/list controls.

**Visual customization:** [CUSTOMIZATION.md — WPF](../../../CUSTOMIZATION.md#wpf-datafilterwpf)

## NuGet integration

### Install the package

```bash
dotnet add package DataFilter.Wpf
dotnet add package DataFilter.Wpf.PopupHost
```

Install **`DataFilter.Wpf`** first (themes, popup UI, filter bar). This package adds grid/list hosting and attach behaviors.

### Target frameworks

`net8.0-windows`, `net9.0-windows`

### Dependencies

- `DataFilter.Wpf` (transitive: Core, ExcelLike, PlatformShared, Localization)

### Quick start — built-in filterable grid

```xml
xmlns:controls="clr-namespace:DataFilter.Wpf.Controls;assembly=DataFilter.Wpf.PopupHost"
```

```xml
<controls:FilterableDataGrid ItemsSource="{Binding GridViewModel.FilteredItems}"
                             ViewModel="{Binding GridViewModel}"
                             FilterContext="{Binding GridViewModel.Context}"
                             AutoGenerateColumns="True" />
```

### Quick start — attach to existing DataGrid / ListView

```xml
xmlns:behaviors="clr-namespace:DataFilter.Wpf;assembly=DataFilter.Wpf.PopupHost"

<DataGrid behaviors:FilterableColumnHeaderBehavior.IsFilterable="True" ... />
```

```csharp
behaviors:FilterableColumnHeaderBehavior.ViewModel="{Binding GridViewModel}"
```

## What this package contains

- **`FilterableDataGrid`**, **`FilterableGridView`** — grids with filter header integration.
- **`FilterableColumnHeaderBehavior`** — make existing columns filterable.
- Popup hosting: open/close lifecycle, LTR/RTL anchoring, window clamping.

Popup **content** (checkbox list, operators) remains in **`DataFilter.Wpf`** (`FilterPopup`, themes).

## Theming

Header buttons and popups inherit WPF ResourceDictionary keys from **`DataFilter.Wpf`** (`FilterLightTheme` / `FilterDarkTheme`). See [CUSTOMIZATION.md — WPF](../../../CUSTOMIZATION.md#wpf-datafilterwpf) and **`FilterThemeResourceKeys`** in PlatformShared.
