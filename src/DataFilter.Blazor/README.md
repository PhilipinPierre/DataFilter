# DataFilter.Blazor

Modern Blazor UI components for the DataFilter library, providing Excel-like filtering, filter pipeline presets, and an optional active-filters bar.

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
                ShowFilterBar="true" />
```

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

## Customization

Components use CSS classes prefixed with `df-`. Override in your app stylesheet. See [DataFilter.css](wwwroot/DataFilter.css) and [CUSTOMIZATION.md](../../CUSTOMIZATION.md).
