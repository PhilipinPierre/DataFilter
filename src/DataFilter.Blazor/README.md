# DataFilter.Blazor

Modern Blazor UI components for the DataFilter library, providing Excel-like filtering, filter pipeline presets, and an optional active-filters bar.

**Visual customization:** [CUSTOMIZATION.md — Blazor](../../CUSTOMIZATION.md#blazor-datafilterblazor)

## Support

- **Blazor Server**
- **Blazor WebAssembly (Wasm)**
- **Blazor Hybrid (MAUI)**

## NuGet integration

### Install the package

```bash
dotnet add package DataFilter.Blazor
```

`DataFilter.Blazor.PopupHost` is referenced transitively (popup positioning interop).

### Target framework

`net8.0` (browser-compatible Razor class library)

### Dependencies (transitive)

`DataFilter.Core`, `DataFilter.Filtering.ExcelLike`, `DataFilter.PlatformShared`, `DataFilter.Localization`, `DataFilter.Blazor.PopupHost`

### Quick start

1. Register static assets in `App.razor`, `index.html`, or `_Host.cshtml`:

```html
<link href="_content/DataFilter.Blazor/DataFilter.css" rel="stylesheet" />
<script src="_content/DataFilter.Blazor/DataFilterInterops.js"></script>
```

2. Add imports in `_Imports.razor`:

```razor
@using DataFilter.Blazor.Components
@using DataFilter.Core.Models
@using DataFilter.Core.Services
```

3. Use **`DataFilterGrid`** (includes column popups; set **`ShowFilterBar="true"`** for the active-filters bar):

```razor
<DataFilterGrid @ref="_grid"
                Items="@employees"
                Columns="@columns"
                ShowFilterBar="true"
                AreColumnFiltersEnabled="true"
                ColumnFilterTriggerMode="ColumnFilterTriggerMode.FilterButton" />
```

Grid-level header settings (see **`DataFilter.PlatformShared.ColumnFilter`**):

- **`AreColumnFiltersEnabled`** — when `false`, no filter UI on any column.
- **`ColumnFilterTriggerMode`** — how the popup opens (`FilterButton`, `HeaderRightClick`, `HeaderLeftClick`, …). When not `FilterButton` and the column is filtered, an inner inset indicator is drawn via CSS class `df-column-header-filter-active` (`::after` pseudo-element).
- Per-column: **`ColumnDefinition.IsFilterable`**, **`ColumnDefinition.TriggerMode`** (`Inherit` or override).

```csharp
@code {
    private DataFilterGrid<Employee>? _grid;

    private async Task ApplyPreset(FilterPipelineSnapshot snapshot)
        => await _grid!.ApplyFilterPipelineSnapshotAsync(snapshot);

    private FilterPipelineSnapshot SyncFromGrid()
        => _grid!.CreateFilterPipelineSnapshot();
}
```

4. Headless integration — keep your own `<table>` and use **`Headless="true"`** with **`ChildContent`** (see demo `/demo/attach`).

## Key Components

### `DataFilterGrid`

All-in-one grid with filter headers, pipeline apply APIs (`ApplyFilterPipelineSnapshotAsync`, `CreateFilterPipelineSnapshot`, `ClearFilters`), and optional filter bar.

### `ColumnFilterButton`

Entry point for filtering in custom table headers.

### `FilterPopup`

Search, multi-select list, advanced operators, Union / Intersection modes. Uses **`DataFilter.PlatformShared.ViewModels.ColumnFilterViewModel`** for Excel-like behavior (selection reconciliation, stacked custom criteria, search-intent persistence via `OrSearchPatterns` / `OrSelectedValues`).

### `FilterBar`

Active-filters chip bar; enabled automatically when **`ShowFilterBar="true"`** on **`DataFilterGrid`**.

## Localization

```csharp
using System.Globalization;
using DataFilter.Localization;

LocalizationManager.Instance.SetCulture(new CultureInfo("fr"));
```

## Theming & customization

Components use stable CSS classes prefixed with **`df-`**. All colors are driven by **CSS custom properties** (`--df-*`) — see **`FilterThemeResourceKeys`** in PlatformShared.

### Quick start

```html
<link href="_content/DataFilter.Blazor/DataFilter.css" rel="stylesheet" />
```

```css
/* Global brand override */
:root {
    --df-primary-color: #e65100;
    --df-popup-bg: #fff8f0;
}

/* Or scoped dark theme */
.my-grid.df-theme-dark {
    --df-popup-bg: #252526;
}
```

### Component parameters

| Parameter | Role |
|-----------|------|
| **`ThemeClass`** | e.g. `df-theme-dark` (`FilterThemeResourceKeys.BlazorDarkThemeClass`) |
| **`Theme`** | `FilterTheme` instance → inline CSS variables via `ToCssVariableStyle()` |

```razor
<DataFilterGrid ThemeClass="df-theme-dark"
                Theme="@FilterTheme.Dark"
                … />
```

### CSS variable reference

| Variable | `FilterTheme` property |
|----------|------------------------|
| `--df-popup-bg` | `PopupBackground` |
| `--df-popup-fg` | `PopupForeground` |
| `--df-popup-border` | `PopupBorder` |
| `--df-primary-color` | `PrimaryColor` |
| `--df-secondary-bg` / `--df-secondary-border` | Secondary surfaces |
| `--df-header-bg` | Advanced-filter header |
| `--df-button-active-color` / `--df-button-inactive-color` | Header filter button |
| `--df-btn-primary-fg` | OK button text |
| `--df-overlay-bg` | Modal overlay |
| `--df-filter-bar-cluster-bg` | Filter bar AND clusters |
| `--df-popup-shadow` / `--df-resize-handle-color` | Chrome details |
| `--df-font-family` / `--df-font-size` | Typography |

Full details: [CUSTOMIZATION.md — Blazor](../../CUSTOMIZATION.md#blazor-datafilterblazor) and [DataFilter.css](wwwroot/DataFilter.css).
