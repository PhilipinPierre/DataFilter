# Agent orientation (DataFilter)

This repository is a .NET library for Excel-style data filtering, with multiple UI integrations and a server-side layer for LINQ expressions. This document summarizes structure, dependencies, and common pitfalls for working effectively in the codebase.

## Solution and build files

- **Solution**: `DataFilter.slnx` at the repository root (XML “solution folder” format). CI and local commands use this file.
- **Packages**: Centralized versions in `Directory.Packages.props` (`ManagePackageVersionsCentrally`).
- **Shared properties**: `Directory.Build.props` (nullable, modern C# language version).

Useful commands: `dotnet restore DataFilter.slnx`, `dotnet build DataFilter.slnx`, `dotnet test DataFilter.slnx`.

## Directory layout

| Directory | Purpose |
|-----------|---------|
| `src/` | Library projects (NuGet packaging depends on configuration). |
| `tests/` | xUnit tests aligned with `src/` projects (often one test project per package). |
| `demo/` | Sample applications (WPF, Blazor Wasm/Server/Hybrid, WinForms, WinUI 3, MAUI, UWP XAML, etc.). |
| `.github/workflows/` | Windows CI: build, tests, pack, and NuGet push on `v*` tags. |

User-facing docs and UI customization live in `README.md`, `CUSTOMIZATION.md`, and per-project `README.md` files under `src/`.

## Project map and dependencies

```
DataFilter.Core                    (no internal project references)
    ↑
    ├── DataFilter.Filtering.ExcelLike
    ├── DataFilter.Expressions.Server
    └── (referenced indirectly wherever base models / engine are needed)

DataFilter.Filtering.ExcelLike
    ↑
    ├── DataFilter.PlatformShared  (Core + ExcelLike + CommunityToolkit.Mvvm)
    └── also referenced by Wpf and Blazor without going through PlatformShared for some stacks

DataFilter.PlatformShared
    ↑
    ├── DataFilter.Wpf            (adds WPF behaviors, XAML)
    ├── DataFilter.WinForms
    ├── DataFilter.Maui
    ├── DataFilter.WinUI3
    └── DataFilter.UwpXaml

DataFilter.Blazor                 → Core + ExcelLike (not PlatformShared)
```

**Key point**: The logical core is **always** `DataFilter.Core`. Excel-like behavior (distinct values, composite descriptors, etc.) lives in **`DataFilter.Filtering.ExcelLike`**. **Shared** ViewModels (filterable grid, column) are in **`DataFilter.PlatformShared`** and are used by WPF, WinForms, MAUI, WinUI 3, and UWP XAML — **not** by Blazor, which defines its own types (`BlazorColumnFilterViewModel`, `IBlazorColumnFilterViewModel` in `DataFilter.Blazor`).

## `src/` projects (role and targets)

| Project | Role | Typical targets |
|---------|------|-----------------|
| **DataFilter.Core** | Abstractions (`IFilterEngine`, `IFilterContext`, `IFilterDescriptor`, `IAsyncDataProvider`), models (`FilterSnapshot`, `FilterDescriptor`, logical groups), **filter pipeline** (`FilterPipeline`, `FilterPipelineSnapshot`, `FilterPipelineCompiler`, `FilterPipelineInterop`), engine (`FilterExpressionBuilder`, `ReflectionFilterEngine`), services (`FilterSnapshotBuilder`, `AsyncDataProviderAdapter`, `FilterPipelineSnapshotMapper`). No UI dependencies. | `net8.0`, `net9.0`, `netstandard2.0`, `netstandard2.1` |
| **DataFilter.Filtering.ExcelLike** | Excel-style engine and models (`ExcelFilterEngine`, `ExcelFilterDescriptor`, distinct-value extractors, etc.). | `net8.0`, `net9.0` |
| **DataFilter.PlatformShared** | Reusable ViewModels (`FilterableDataGridViewModel`, `ColumnFilterViewModel`) built on CommunityToolkit.Mvvm. | `net8.0`, `net9.0` |
| **DataFilter.Wpf** | WPF controls and behaviors. References Core, ExcelLike, and PlatformShared. | `net8.0-windows`, `net9.0-windows` |
| **DataFilter.Blazor** | Razor components (`DataFilterGrid`, `FilterPopup`, etc.) and Blazor-specific ViewModels. | `net8.0` (Razor SDK) |
| **DataFilter.Maui** | MAUI integration; depends on PlatformShared. | Multi-target (`net9.0-android`, iOS, Windows, etc.) |
| **DataFilter.WinForms** | Windows Forms integration via PlatformShared. | `net8.0-windows`, `net9.0-windows` |
| **DataFilter.WinUI3** | WinUI 3; Windows App SDK. | `net8.0-windows10.0.19041.0` |
| **DataFilter.UwpXaml** | UWP / Uno project (Uno toolkit in packages). | `net9.0-windows10.0.26100.0` |
| **DataFilter.Expressions.Server** | Maps `FilterSnapshot` to `Expression<Func<T,bool>>` for EF / `IQueryable`. | `net8.0`, `net9.0` |

Any change to **serializable models** or **contracts** (`IFilterContext`, snapshots, **`FilterPipelineSnapshot`**) may affect **Core**, **Expressions.Server**, **tests**, and indirectly **all** UIs.

## Cross-cutting concepts (where to look)

1. **`IFilterContext` / `FilterContext`**: Current filter and sort state; UIs and data providers depend on it. **`ReplaceDescriptors`** sets an **ordered** list of descriptors and allows **duplicate `PropertyName` values** (unlike `AddOrUpdateDescriptor`, which replaces by column key). Used when applying a compiled **filter pipeline**.
2. **`FilterSnapshot` + `FilterSnapshotBuilder`**: Serialize / restore state for APIs, persistence, or server layers. `RestoreSnapshot` requires a concrete `FilterContext` instance (see `FilterSnapshotBuilder`).
3. **Filter pipeline** (`DataFilter.Core.Pipeline`): Mutable graph of **criterion** and **named group** nodes (`CriterionPipelineNode`, `GroupPipelineNode`) with stable IDs, **enable/disable**, and root / group **`LogicalOperator`**. **`FilterPipelineCompiler`** turns a pipeline into `IReadOnlyList<IFilterDescriptor>` (nested `FilterGroup` with internal keys like `__group_{id}`). **`FilterPipelineSnapshot`** is the JSON-oriented DTO; **`FilterPipelineSnapshotMapper`** maps to/from the graph; **`FilterPipelineInterop.FromLegacySnapshot`** builds a pipeline from an existing **`IFilterSnapshot`**. **`FilterPipelineContextExtensions.ApplyToContext`** applies the compiled descriptors to a `FilterContext`.
4. **`ReflectionFilterEngine` / `FilterExpressionBuilder`**: In-memory or client-side expression evaluation and building.
5. **`IAsyncDataProvider<T>`**: Async loading, paging, distinct values for popups — central pattern for “remote data” scenarios.
6. **ExcelLike**: Specialized descriptors (text, numeric, date) and domain logic separate from the generic Core engine. Column popups still use **`AddOrUpdateDescriptor`** by property name; advanced UIs can drive **`ApplyFilterPipelineAsync`** (see PlatformShared) after editing a pipeline or preset.

## Excel column filter state, popup UI, and item source changes

When **`LocalDataSource`** (or the collection view’s **`SourceCollection`**) is **replaced**, distinct value identities change. The grid must keep **`ExcelFilterState.SelectedValues`** aligned with canonical instances from the current data, and the **column popup** must **reload distincts** and **reapply** the same logical filter as the context.

### Search persistence rule (avoid serializing distinct lists)

The column popup supports `SearchText` to narrow the *distinct values list*. When the user applies a search and keeps **Select All** enabled, the implementation converts that UI intent into a **single persisted search rule** (stored as a text operator + pattern) instead of serializing the resulting `In(list)` values. This prevents saved filters from becoming stale when data evolves.

Wildcards (`*`, `?`) are supported in **Core** text operators (expression builder + evaluator), so persisted patterns replay consistently without needing ExcelLike-only logic.

| Piece | Role |
|-------|------|
| **`ExcelFilterSelectionReconciler`** (`Filtering.ExcelLike`) | Maps `SelectedValues` to instances present in the current distinct list (reference equality first, then `Equals`). **`dropSelectionsNotInDistinct`**: default **`true`** when reconciling **descriptor state** (e.g. `FilterableDataGridViewModel.RefreshDataAsync`, `CollectionViewFilterAdapter`) so stale entries are removed. **`false`** in **`ColumnFilterViewModel.InitializeAsync`** / **`BlazorColumnFilterViewModel.InitializeAsync`** so search-narrowed distinct lists do not drop off-screen selections needed for “add to existing”. |
| **`FilterableDataGridViewModel.RefreshDataAsync`** | After reconciling descriptors, if **`LocalDataSource`** **reference** changed and there are descriptors, raises **`FilterDescriptorsChanged`** so WPF **`FilterableColumnHeaderBehavior`** runs **`SyncColumnFilterFromParentAsync`** (refetch distincts + `LoadStateAsync`). Hosts should call **`RefreshDataAsync`** after assigning a new items collection. |
| **`CollectionViewFilterAdapter.RefreshDataAsync`** | Same idea when **`CollectionView.SourceCollection`** reference changes; raises **`FilterDescriptorsChanged`** when descriptors exist. |
| **`ColumnFilterViewModel` / `BlazorColumnFilterViewModel`** | After **`InitializeAsync`**, if **`FilterState.CustomOperator`** is set, sync observable fields and call **`UpdateSelectionFromCustomFilter()`** so checkbox preview matches operators on **new** distincts. **`LoadStateAsync`**: for **list-only** filters, **`ApplySelectionStateToItemsRecursive`**; for **custom** filters, **`UpdateSelectionFromCustomFilter`** after **`_internalUpdate`** is cleared (In-list semantics must not overwrite operator-driven preview). |
| **Stacked custom criteria on one column** | **`ExcelFilterDescriptor.Descriptors`** emits **`CustomOperator`** + **`AdditionalCustomCriteria`** as **AND**-combined rules. **`UpdateSelectionFromCustomFilter`** uses **`ValueMatchesAllStackedCustomColumnFilters`** (primary operator + each **`ExcelFilterAdditionalCriterion`**) so the popup matches grid filtering. |

## Tests

- Location: `tests/DataFilter.*.Tests`.
- Framework: **xUnit**, **Moq** in some projects.
- After changes to Core, ExcelLike, Expressions.Server, or shared ViewModels, run **`dotnet test DataFilter.slnx`** on the branch.

## Notes for agents

1. **Multi-targeting**: Core may compile for `netstandard2.0` — avoid very new .NET APIs without compile-time guards, and keep signatures consistent across packages.
2. **Two ViewModel families**: Do not casually merge `PlatformShared` and `DataFilter.Blazor`; a WPF UI change does not automatically apply to Blazor without updating Razor components.
3. **Windows platforms**: WPF / WinForms require `-windows` TFMs; mixing with “any” libraries needs careful conditional references.
4. **MAUI / WinUI / UWP**: Heavier, sometimes OS-specific builds; CI uses **windows-latest** with .NET 8 and 9.
5. **Snapshot consistency**: Changes to `FilterGroup`, snapshot entries, **`FilterPipelineSnapshot` / `FilterPipelineNodeDto`**, or enums (`FilterOperator`, `LogicalOperator`) must stay aligned with **Expressions.Server**, **FilterSnapshotBuilder**, **pipeline mappers**, and related tests.
6. **Documentation**: Avoid duplicating this file at length; prefer existing per-project `README.md` files for usage details.

## Good starting reads

- `README.md` — product overview and quick start (includes **filter pipeline** summary).
- `src/DataFilter.Core/README.md` — abstractions, **`FilterPipeline` / `FilterPipelineSnapshot`**, and extending the engine.
- `src/DataFilter.PlatformShared/README.md` — shared ViewModels, **`ApplyFilterPipelineAsync`**, **`FilterDescriptorsChanged`**, and column popup sync.
- `src/DataFilter.Filtering.ExcelLike/README.md` — **`ExcelFilterDescriptor`**, **`ExcelFilterSelectionReconciler`**, stacked **`AdditionalCustomCriteria`**.
- `src/DataFilter.Expressions.Server/README.md` — server-side LINQ bridge.
- `CUSTOMIZATION.md` — WPF themes and Blazor CSS.

---

*Generated to help onboard agents on this repository; update when major structural changes occur (new projects, renames, public breaking changes).*
