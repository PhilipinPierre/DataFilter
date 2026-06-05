# DataFilter.PlatformShared

Shared view-model logic for DataFilter UI specializations (WinForms, MAUI, WinUI 3, Uno/UWP XAML).

## Localization

All popup UI stacks should source their user-facing strings (buttons, section headers, operator names, etc.)
from **`DataFilter.Localization.LocalizationManager`** so language switching works at runtime.

### Per-grid culture override

`IFilterableDataGridViewModel` exposes:

- **`CultureInfo? CultureOverride`**: optional UI override culture used by integrations when showing popups.

`FilterableDataGridViewModel` provides constructors:

- `new FilterableDataGridViewModel(CultureInfo? cultureOverride)`
- `new FilterableDataGridViewModel<T>(CultureInfo? cultureOverride)`

## Filter pipeline integration

`IFilterableDataGridViewModel` (implemented by `FilterableDataGridViewModel` and WPF’s `CollectionViewFilterAdapter`) includes:

- **`Task ApplyFilterPipelineAsync(FilterPipeline pipeline)`** — compiles the pipeline, calls **`FilterContext.ReplaceDescriptors`**, resets page to 1, and refreshes data. Use this after loading a preset from JSON or editing the pipeline in your UI.
- **`FilterPipeline CreatePipelineFromCurrentSnapshot()`** — builds a mutable **`FilterPipeline`** from the current filters via **`FilterPipelineInterop.FromLegacySnapshot(ExtractSnapshot())`**, suitable for displaying or serializing the active state.
- **`FilterPipelineSnapshot CreateFilterPipelineSnapshot()`** / **`ApplyFilterPipelineSnapshotAsync`** — in-memory preset round-trip (filters + ordered **`SortEntries`**). JSON is optional via your own serializer.
- **`Task ApplyPipelineSessionAsync()`** — applies live edits on **`PipelineSession.Pipeline`** and **`PipelineSession.SortEntries`** without building a snapshot.
- **`FilterPipelineSnapshotEditor`** (Core) — helpers to mutate a snapshot before apply (`AddRootCriterion`, `AddSort`, `RemoveNode`, …).

```csharp
// Direct list edits (no JSON)
var snapshot = grid.CreateFilterPipelineSnapshot();
FilterPipelineSnapshotEditor.AddSort(snapshot, "Name");
FilterPipelineSnapshotEditor.AddRootCriterion(snapshot, "Department", nameof(FilterOperator.Equals), "IT");
await grid.ApplyFilterPipelineSnapshotAsync(snapshot);

// Or mutate session lists, then apply
grid.PipelineSession.SortEntries.Add(new SortSnapshotEntry { PropertyName = "Name" });
await grid.ApplyPipelineSessionAsync();
```

Excel-style column filters continue to use **`ApplyColumnFilter`** / **`ClearColumnFilter`** (`AddOrUpdateDescriptor` under the hood). Mixing both approaches is possible but you should treat one path as the source of truth for a given screen, or sync explicitly.

## Active filters bar (optional UI)

`IFilterableDataGridViewModel` exposes:

- **`FilterPipelineSession PipelineSession`** — live pipeline with stable node IDs (synced from context).
- **`FilterBarViewModel FilterBar`** — chips, AND/OR layout, enable/disable, remove, **+** (add AND criterion on the same cluster), and **OR+** (add a new OR group).
- **`ApplyBarCriterionAsync` / `RemoveBarNodeAsync`** — targeted edits from the bar popup.

Each UI package provides a default bar control (hidden by default). Enable it with **`ShowFilterBar="True"`**:

| Stack | Chrome host (bar + popup wiring) | Bar control alone |
|-------|----------------------------------|-------------------|
| WPF | `FilterGridChrome` | `FilterBar` (popup built-in) |
| Blazor | `DataFilterGrid` | `FilterBar` |
| WinForms | `FilterGridChromeControl` | `FilterBarControl` |
| WinUI 3 | `FilterGridChrome` | `FilterBarControl` |
| MAUI | `FilterGridChromeView` | `FilterBarView` |

Prefer the **chrome** host so bar edits open the column popup with pipeline apply/remove semantics.

Interactions: left-click chip → column popup (single criterion); right-click → toggle enabled; **×** or Clear → remove node; **+** → add AND sibling on the same cluster; **OR+** → add a new AND group (e.g. `(Department = IT AND Name starts with Alice) OR (Department = RH AND Name starts with Bob)`). Set **`FilterPipeline.RootCombineOperator`** to **`Or`** automatically when a second group is added.

**Drag and drop**: drag a chip onto another **cluster** (bordered AND group) to move the criterion into that group; drag onto an **OR** separator to detach it into its own OR branch at that position. Uses **`FilterPipelineEditor.MoveCriterionToCluster`** / **`MoveCriterionToOrGap`** and reapplies the pipeline to the grid.

Example JSON (two OR groups):

```json
{
  "schemaVersion": 1,
  "rootCombineOperator": "Or",
  "nodes": [
    {
      "kind": "group",
      "logicalOperator": "And",
      "children": [
        { "kind": "criterion", "propertyName": "Department", "operator": "Equals", "value": "IT" },
        { "kind": "criterion", "propertyName": "Name", "operator": "StartsWith", "value": "Alice" }
      ]
    },
    {
      "kind": "group",
      "logicalOperator": "And",
      "children": [
        { "kind": "criterion", "propertyName": "Department", "operator": "Equals", "value": "RH" },
        { "kind": "criterion", "propertyName": "Name", "operator": "StartsWith", "value": "Bob" }
      ]
    }
  ],
  "sortEntries": [
    { "propertyName": "Name", "isDescending": false },
    { "propertyName": "Department", "isDescending": true }
  ]
}
```

## `FilterableDataGridViewModel` and column popup sync

- **`IFilterableDataGridViewModel.FilterDescriptorsChanged`**: Raised when the pipeline is applied/restored, when a column is cleared, and—on the **local** data path—when **`LocalDataSource`** **reference** changes after **`RefreshDataAsync`** (so column headers can refetch distinct values and **`LoadStateAsync`** into the popup).
- **`RefreshDataAsync`**: Reconciles **`ExcelFilterDescriptor.State.SelectedValues`** against distincts from the current source, then applies filters. **Assign a new `LocalDataSource` then call `RefreshDataAsync`** so reconciliation and the event above run in order.
- **`ColumnFilterViewModel`**: Applies **`ExcelFilterSelectionReconciler`** in **`InitializeAsync`** with **`dropSelectionsNotInDistinct: false`** to preserve selections not in the current narrow distinct list. **`LoadStateAsync`** restores list selection or, for **custom** filters, **`UpdateSelectionFromCustomFilter`** (including **all** stacked **`AdditionalCustomCriteria`**) so the popup matches **`ExcelFilterDescriptor`**. For persistence, the Excel-like state can also express **OR-combined search intent** (`OrSearchPatterns`, `OrSelectedValues`) to avoid materializing `In(list)` when reapplying presets on evolving data.
