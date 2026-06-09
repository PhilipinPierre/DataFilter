# DataFilter.PlatformShared

Shared view-model logic for DataFilter UI specializations (WinForms, MAUI, WinUI 3, WPF adapters, Blazor grid).

## NuGet integration

### Install the package

```bash
dotnet add package DataFilter.PlatformShared
```

### Target frameworks

`net8.0`, `net9.0`

### Dependencies

- `DataFilter.Core`
- `DataFilter.Filtering.ExcelLike`
- `DataFilter.Localization`
- `CommunityToolkit.Mvvm`

Add a UI package (`DataFilter.Wpf`, `DataFilter.Blazor`, …) for controls; use **PlatformShared** alone when wiring your own grid.

### Quick start

```csharp
using DataFilter.Core.Enums;
using DataFilter.Core.Models;
using DataFilter.Core.Services;
using DataFilter.PlatformShared.ViewModels;

var grid = new FilterableDataGridViewModel<Employee>
{
    LocalDataSource = employees
};
await grid.RefreshDataAsync();

// Apply a pipeline preset (filters + sort) edited in memory
var snapshot = grid.CreateFilterPipelineSnapshot();
FilterPipelineSnapshotEditor.AddRootCriterion(snapshot, "Department", nameof(FilterOperator.Equals), "IT");
await grid.ApplyFilterPipelineSnapshotAsync(snapshot);
```

## Localization

All popup UI stacks source user-facing strings from **`DataFilter.Localization.LocalizationManager`**.

### Per-grid culture override

`IFilterableDataGridViewModel` exposes **`CultureInfo? CultureOverride`**. Constructors:

- `new FilterableDataGridViewModel(cultureOverride)`
- `new FilterableDataGridViewModel<T>(cultureOverride)`

## Filter pipeline integration

`IFilterableDataGridViewModel` includes:

- **`Task ApplyFilterPipelineAsync(FilterPipeline pipeline)`** — compiles the pipeline, calls **`FilterContext.ReplaceDescriptors`**, resets page to 1, and refreshes data.
- **`FilterPipeline CreatePipelineFromCurrentSnapshot()`** — mutable pipeline from current filters via **`FilterPipelineInterop.FromLegacySnapshot(ExtractSnapshot())`**.
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

Excel-style column filters continue to use **`ApplyColumnFilter`** / **`ClearColumnFilter`** (`AddOrUpdateDescriptor` under the hood). Mixing both approaches is possible but treat one path as the source of truth per screen, or sync explicitly.

## Active filters bar (optional UI)

`IFilterableDataGridViewModel` exposes:

- **`FilterPipelineSession PipelineSession`** — live pipeline with stable node IDs (synced from context).
- **`FilterBarViewModel FilterBar`** — chips, AND/OR layout, enable/disable, remove, **+** (add AND criterion on the same cluster), and **OR+** (add a new OR group).
- **`ApplyBarCriterionAsync` / `RemoveBarNodeAsync`** — targeted edits from the bar popup.

Each UI package provides a default bar control (hidden by default). Enable with **`ShowFilterBar="True"`**:

| Stack | Chrome host (bar + popup wiring) | Bar control alone |
|-------|----------------------------------|-------------------|
| WPF | `FilterGridChrome` | `FilterBar` |
| Blazor | `DataFilterGrid` | `FilterBar` |
| WinForms | `FilterGridChromeControl` | `FilterBarControl` |
| WinUI 3 | `FilterGridChrome` | `FilterBarControl` |
| MAUI | `FilterGridChromeView` | `FilterBarView` |

Prefer the **chrome** host so bar edits open the column popup with pipeline apply/remove semantics.

Interactions: left-click chip → column popup (single criterion); right-click → toggle enabled; **×** or Clear → remove node; **+** → add AND sibling; **OR+** → add a new AND group. Set **`FilterPipeline.RootCombineOperator`** to **`Or`** when a second group is added.

**Drag and drop**: drag a chip onto another **cluster** to move the criterion; drag onto an **OR** separator to detach into its own OR branch. Uses **`FilterPipelineEditor.MoveCriterionToCluster`** / **`MoveCriterionToOrGap`**.

Example JSON (two OR groups + sort):

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

## Column filter header settings (`ColumnFilter` namespace)

Shared types in **`DataFilter.PlatformShared.ColumnFilter`**:

| Type | Role |
|------|------|
| **`ColumnFilterTriggerMode`** | How the column popup opens (`FilterButton`, `HeaderRightClick`, `HeaderLeftClick`, `HeaderDoubleClick`, `HeaderMiddleClick`, `None`, `ContextMenuFilter`, `HeaderLongPress`, `KeyboardShortcut`, `HoverRevealButton`, `ShiftClick`, `CtrlClick`, `Inherit`) |
| **`ColumnFilterHeaderOptions`** | Resolves grid/column settings; **`ShowsFilterStateOnHeaderBorder`** is `true` for every mode except **`FilterButton`** |
| **`ColumnFilterHeaderChrome`** | Shared constants (long-press duration, `Alt+Down`, Blazor CSS class names) |
| **`ExcelFilterActiveState`** | Whether an **`ExcelFilterState`** counts as “filtered” for header chrome |

**`HeaderLeftClick`** disables native column sorting on that column (WPF `CanUserSort=false`, WinForms `SortMode=NotSortable`).

UI stacks expose grid-level **`AreColumnFiltersEnabled`** and **`ColumnFilterTriggerMode`** on their filterable grid / attach APIs (WPF `FilterableDataGrid`, Blazor `DataFilterGrid`, WinForms `FilterableDataGrid` / `DataGridViewFilterAdapter`, WinUI 3 `FilterableDataGrid`, MAUI `FilterableDataGrid`, list header adapters).

## `FilterableDataGridViewModel` and column popup sync

- **`IFilterableDataGridViewModel.FilterDescriptorsChanged`**: Raised when the pipeline is applied/restored, when a column is cleared, and—on the **local** data path—when **`LocalDataSource`** **reference** changes after **`RefreshDataAsync`**.
- **`RefreshDataAsync`**: Reconciles **`ExcelFilterDescriptor.State.SelectedValues`** against distincts from the current source. **Assign a new `LocalDataSource` then call `RefreshDataAsync`**.
- **`ColumnFilterViewModel`**: Applies **`ExcelFilterSelectionReconciler`** in **`InitializeAsync`** with **`dropSelectionsNotInDistinct: false`**. **`LoadStateAsync`** restores list selection or, for **custom** filters, **`UpdateSelectionFromCustomFilter`** (including stacked **`AdditionalCustomCriteria`**).
