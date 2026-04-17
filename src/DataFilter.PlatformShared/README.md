# DataFilter.PlatformShared

Shared view-model logic for DataFilter UI specializations (WinForms, MAUI, WinUI 3, Uno/UWP XAML).

## Filter pipeline integration

`IFilterableDataGridViewModel` (implemented by `FilterableDataGridViewModel` and WPF’s `CollectionViewFilterAdapter`) includes:

- **`Task ApplyFilterPipelineAsync(FilterPipeline pipeline)`** — compiles the pipeline, calls **`FilterContext.ReplaceDescriptors`**, resets page to 1, and refreshes data. Use this after loading a preset from JSON or editing the pipeline in your UI.
- **`FilterPipeline CreatePipelineFromCurrentSnapshot()`** — builds a mutable **`FilterPipeline`** from the current filters via **`FilterPipelineInterop.FromLegacySnapshot(ExtractSnapshot())`**, suitable for displaying or serializing the active state.

Excel-style column filters continue to use **`ApplyColumnFilter`** / **`ClearColumnFilter`** (`AddOrUpdateDescriptor` under the hood). Mixing both approaches is possible but you should treat one path as the source of truth for a given screen, or sync explicitly.
