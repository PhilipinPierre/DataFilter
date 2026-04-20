# DataFilter.PlatformShared

Shared view-model logic for DataFilter UI specializations (WinForms, MAUI, WinUI 3, Uno/UWP XAML).

## Filter pipeline integration

`IFilterableDataGridViewModel` (implemented by `FilterableDataGridViewModel` and WPF’s `CollectionViewFilterAdapter`) includes:

- **`Task ApplyFilterPipelineAsync(FilterPipeline pipeline)`** — compiles the pipeline, calls **`FilterContext.ReplaceDescriptors`**, resets page to 1, and refreshes data. Use this after loading a preset from JSON or editing the pipeline in your UI.
- **`FilterPipeline CreatePipelineFromCurrentSnapshot()`** — builds a mutable **`FilterPipeline`** from the current filters via **`FilterPipelineInterop.FromLegacySnapshot(ExtractSnapshot())`**, suitable for displaying or serializing the active state.

Excel-style column filters continue to use **`ApplyColumnFilter`** / **`ClearColumnFilter`** (`AddOrUpdateDescriptor` under the hood). Mixing both approaches is possible but you should treat one path as the source of truth for a given screen, or sync explicitly.

## `FilterableDataGridViewModel` and column popup sync

- **`IFilterableDataGridViewModel.FilterDescriptorsChanged`**: Raised when the pipeline is applied/restored, when a column is cleared, and—on the **local** data path—when **`LocalDataSource`** **reference** changes after **`RefreshDataAsync`** (so column headers can refetch distinct values and **`LoadStateAsync`** into the popup).
- **`RefreshDataAsync`**: Reconciles **`ExcelFilterDescriptor.State.SelectedValues`** against distincts from the current source, then applies filters. **Assign a new `LocalDataSource` then call `RefreshDataAsync`** so reconciliation and the event above run in order.
- **`ColumnFilterViewModel`**: Applies **`ExcelFilterSelectionReconciler`** in **`InitializeAsync`** with **`dropSelectionsNotInDistinct: false`** to preserve selections not in the current narrow distinct list. **`LoadStateAsync`** restores list selection or, for **custom** filters, **`UpdateSelectionFromCustomFilter`** (including **all** stacked **`AdditionalCustomCriteria`**) so the popup matches **`ExcelFilterDescriptor`**. For persistence, the Excel-like state can also express **OR-combined search intent** (`OrSearchPatterns`, `OrSelectedValues`) to avoid materializing `In(list)` when reapplying presets on evolving data.
