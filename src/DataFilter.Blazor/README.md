# DataFilter.Blazor

Modern Blazor UI components for the DataFilter library, providing Excel-like filtering capabilities for your applications.

## Support
- **Blazor Server**
- **Blazor WebAssembly (Wasm)**
- **Blazor Hybrid (MAUI)**

## Key Components

### `ColumnFilterButton`
The entry point for filtering. Typically placed in a table header. It manages the filter state and toggles the popup.

### `FilterPopup`
The core UI for selection and advanced filtering. Supports:
- Search-as-you-type.
- Multi-select value list with hierarchical grouping (Dates).
- Advanced operators (Equals, Contains, Greater Than, etc.).
- Accumulation modes (Union / Intersection).
- **`BlazorColumnFilterViewModel`** mirrors the shared Excel behavior (from `DataFilter.PlatformShared`): reconciliation of **`SelectedValues`** when distincts change, `LoadStateAsync` vs custom-filter preview, **AND**-combined stacked custom criteria, and persistence of search intent in Union mode via `OrSearchPatterns` / `OrSelectedValues` (so presets don’t materialize huge `In(list)` snapshots).

## Usage

1. Add the project reference to `DataFilter.Blazor`.
2. Add the following to your `_Imports.razor`:
   ```razor
   @using DataFilter.Blazor.Components
   @using DataFilter.Blazor.ViewModels
   ```
3. Register the required JS and CSS in your `App.razor` or `index.html`:
   ```html
   <link href="_content/DataFilter.Blazor/DataFilter.css" rel="stylesheet" />
   <script src="_content/DataFilter.Blazor/DataFilterInterops.js"></script>
   ```

## Customization

All components use CSS classes prefixed with `df-`. You can easily override these in your application CSS:

```css
/* Example: Customizing the popup header */
.df-popup-header {
    background-color: #0078d4;
    color: white;
}
```

Refer to [DataFilter.css](wwwroot/DataFilter.css) for the list of available classes.
