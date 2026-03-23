# Features.md - Feature Contract for DataFilter Specialization Projects

This document defines the feature contract for the DataFilter specialization projects, so the project can be ported to other frameworks/UI layers.

It is derived only from:
- `src/DataFilter.Blazor`
- `src/DataFilter.Wpf`

Where Blazor and WPF differ, the difference is documented explicitly so future ports can either reproduce the expected behavior or deliberately unify it.

## 1. Definitions (used terms)

1. **Column**: a field shown in the grid/table (header) and filtered independently.
2. **Per-column filter state**: `ExcelFilterState` (from `DataFilter.Filtering.ExcelLike.Models`).
3. **Distinct values list**: `ExcelFilterState.DistinctValues` and its UI representation via `FilterValueItem` (hierarchy possible for dates).
4. **Custom/advanced filter**: the operator selected in the "Advanced Filter" section (e.g. `Contains`, `Between`).
5. **Add to existing filter**: a mode that merges criteria with an already active filter for the same column, using an accumulation semantics of `Union` or `Intersection`.
6. **Sorting**: direction selection (A-Z / Z-A) and multi-column ordering (sub-sorts).
7. **Snapshot**: a serializable representation of the whole state (filters + sorting) via `IFilterSnapshot`.

## 2. Functional contract (UI + interaction) expected from any specialization

## 2.1 Filter button in the column header (per column)

A specialization must provide an interactive element placed in the column header:
- Show a filter active vs inactive indicator.
- Open/close a dedicated filtering panel (popup).
- Close when the user clicks outside the popup ("outside click" behavior).

Examples in the existing projects:
- WPF: `ColumnFilterButton` is injected into each header via `FilterableColumnHeaderBehavior`.
- Blazor: `ColumnFilterButton` appears in the headers of `DataFilterGrid.razor`.

## 2.2 Filtering popup (Excel-like filtering panel per column)

A specialization must provide a popup/panel with the following sections:

### 2.2.1 Sorting section

Required controls (4 commands):
- `Sort A to Z` (ascending sort direction).
- `Sort Z to A` (descending sort direction).
- `Add Sort A to Z` (add a sort criterion as a sub-sort).
- `Add Sort Z to A` (add a sort criterion as a sub-sort).

Semantics:
- Sorting must be applied after filtering (as done in the existing implementations).
- The specialization must support multi-column ordering (primary sort + sub-sorts).
- Adding a sub-sort must replace any existing sort for the same property (in the existing projects, criteria for the same field are replaced before adding).

Examples:
- Blazor: `FilterPopup.razor` calls the commands on `IBlazorColumnFilterViewModel`.
- WPF: `FilterPopup.xaml` calls `SortAscendingCommand`, `SortDescendingCommand`, `AddSubSortAscendingCommand`, `AddSubSortDescendingCommand`.

### 2.2.2 Search bar (search-as-you-type / async distinct values)

A specialization must support:
- Text entry in a search area.
- Search must trigger a refresh of the distinct values list displayed in the popup.
- A loading indicator must be shown during the refresh.

Implementation notes inferred from the code:
- WPF provides `AsyncFilterBehavior` (debounce) but it is not wired in `FilterPopup.xaml` (the behavior exists, but is unused/commented there).
- Blazor triggers logic on `@oninput` (no debounce inside the component itself).

### 2.2.3 Add to existing filter + AccumulationMode

A specialization must expose a merge mode:
- A checkbox `Add selection to filter` / `AddToExistingFilter`.
- When enabled, a select control must let the user choose the merge semantics `AccumulationMode`:
  - `Union` (logical OR accumulation)
  - `Intersection` (logical AND accumulation)

Expected behavior:
- In merge mode, the new selection (or evaluation result) must be merged with the existing set according to `Union`/`Intersection`.
- In merge mode, the "custom/advanced filter" section is reset in the existing implementations (see section 4).

### 2.2.4 Advanced/custom filter (contextual operators)

A specialization must provide a collapsible/expandable "Advanced Filter" section:
- Show an `AvailableOperators` list dependent on the column type (see section 4.5).
- Provide input fields for `CustomValue1` and (optionally) `CustomValue2` for the `Between` case.
- Changing the operator (and the values) must update the selection UI (checkboxes) in real time.

Available operators inferred from the view model code:
- Text type:
  - `Equals`
  - `NotEquals`
  - `Contains`
  - `NotContains`
  - `StartsWith`
  - `EndsWith`
- Number / date / time type:
  - `Equals`
  - `NotEquals`
  - `GreaterThan`
  - `GreaterThanOrEqual`
  - `LessThan`
  - `LessThanOrEqual`
  - `Between` (using `CustomValue1` / `CustomValue2`)

### 2.2.5 Multi-selection distinct values list (checkboxes)

A specialization must display a hierarchical list/control:
- Multi-select checkboxes for `FilterValueItem`.
- Support a `Select All` checkbox:
  - A tri-state boolean (checked / unchecked / indeterminate) via `bool?` (Blazor and WPF).
- For dates: a hierarchical (tree) presentation.

UI deductions:
- Blazor: `FilterValueItemView.razor` manages `IsExpanded` with an explicit (+/-) expander button for nodes that have children.
- WPF: `FilterPopup.xaml` uses a `TreeView` with `Children`; expansion relies on the internal `TreeViewItem` expansion behavior (the binding `IsExpanded` from the model is not used in XAML).

### 2.2.6 Handling "Blanks" (null values)

The list must include an entry representing null values:
- Blazor displays `(Blanks)`.
- WPF displays `FilterResources.Blanks` (from localized resources).

Selection semantics:
- The "blanks" entry value is `null` stored in `FilterValueItem.Value`.
- Selection must be correctly included/excluded in the (optional) `SelectedValues` set.

### 2.2.7 OK / Clear actions

A specialization must provide:
- `OK` button:
  - apply the filter to the data source
  - close the popup (in both Blazor and WPF)
- `Clear` button:
  - reset the per-column filter state

Observed difference important for ports:
- WPF: `Clear` closes the popup via the `ColumnFilterViewModel.OnClear` event.
- Blazor: `Clear` resets the state but does not close the popup in `FilterPopup.razor` (closing is only triggered on `Apply`).

### 2.2.8 Popup resizing

A specialization must allow resizing the popup:
- Blazor: via `DataFilterInterops.initResizable` (drag handle + clamp to viewport).
- WPF: via a `Thumb` in `FilterPopup.xaml` (drag delta for width/height).

## 3. Data model contract (internal UI)

## 3.1 `ExcelFilterState` (per-column filter state)

The specialization must manage a state equivalent to `ExcelFilterState`:
- `SearchText: string`
- `UseWildcards: bool` (default false; not currently used as an explicit UI element in the Blazor/WPF projects)
- `DistinctValues: List<object>` (values shown in the list)
- `SelectedValues: HashSet<object>` (explicit selected values)
- `SelectAll: bool` (selects all visible distinct values when true)
- `CustomOperator: FilterOperator?`
- `CustomValue1: object?`
- `CustomValue2: object?`
- method `Clear()` that resets:
  - `SearchText`
  - empties `DistinctValues` and `SelectedValues`
  - sets `SelectAll=true`
  - sets `CustomOperator` / `CustomValue*` to null

## 3.2 `FilterValueItem` (checkbox tree node in the UI)

The specialization must provide a node representation:
- `Value: object?`
- `DisplayText: string`
- `Children: ObservableCollection<FilterValueItem>` (0 children = leaf)
- `IsSelected: bool?` (tri-state support for indeterminate)
- `IsNull: bool?` (derived from `IsSelected == null` in the model)
- `IsExpanded: bool` (used explicitly by Blazor; WPF does not depend on it)
- method `GetSelectedValues(...)`:
  - adds leaf nodes where `IsSelected == true`
  - ignores leaf nodes where `IsSelected == null` (indeterminate)

Selection propagation:
- When a child node changes, parent nodes must recompute their state:
  - allSelected/allUnselected => `IsSelected` true/false
  - otherwise => `IsSelected` null

## 4. Mapping contract (UI state -> filtered data)

## 4.1 Distinct values + search

The specialization must be able to:
- Extract distinct values for a column to feed the UI.
- Apply filtering of those distinct values based on `SearchText`.

Semantics inferred from the code:

### WPF local / adapter behavior
- If `searchText` contains wildcards `*` or `?`:
  - wildcard matcher is case-insensitive (`WildcardMatcher`).
- Otherwise:
  - use a case-insensitive "starts-with" match.

### WPF async behavior
- `IAsyncDataProvider.FetchDistinctValuesAsync(propertyName, searchText, ...)` is responsible for applying the "search -> distinct values" filtering.

### Blazor behavior
- The refresh of the distinct values list is asynchronous via `SearchCommand` (on `IBlazorColumnFilterViewModel`).
- The current `DataFilterGrid.razor` implementation ignores `SearchText` in the distinct values provider (demo behavior). For a real port, the specialization must provide a provider that respects `SearchText`; otherwise the search-as-you-type feature does not work correctly.

## 4.2 Building filters from `ExcelFilterState`

Two different approaches are visible in the existing projects:

### 4.2.1 WPF: `ExcelFilterDescriptor` (custom precedence vs selection)

In WPF, the per-column filter is applied via `ExcelFilterDescriptor`:
- If `State.CustomOperator != null`, then the descriptor:
  - adds a criterion for `CustomOperator` with `CustomValue1` (and a `RangeValue` when `Between`)
  - does not automatically add an `In` criterion for `SelectedValues` (the "manual selection" logic is in an else-if branch).
- If `State.CustomOperator == null`, then:
  - the list selection is used via `FilterOperator.In` and `State.SelectedValues` (unless `SelectAll` is true and/or unless the selection corresponds to the full set).

Consequence:
- When the user enables "Advanced Filter", checkbox selection can be synchronized visually, but the final filtering logic gives priority to `CustomOperator`.

### 4.2.2 Blazor: direct combination of `In` + custom (AND composition)

In Blazor, `DataFilterGrid.razor` assembles descriptors manually:
- if `SelectAll == false` and `SelectedValues.Count > 0`:
  - add a `FilterOperator.In` criterion with the `SelectedValues`
- if `CustomOperator` is defined:
  - add a custom criterion (including `Between` with `RangeValue`)
- criteria are combined into a `FilterGroup` with `LogicalOperator.And`.

Consequence:
- Blazor may combine `In(SelectedValues)` and the custom criterion in an AND.

### 4.2.3 Recommendation for ports

For a robust port, the port must document/decide which combination semantics to adopt:
- WPF compatibility (custom precedence over manual selection) OR
- Blazor compatibility (AND: selection list + custom when both are active).

The core engine supports filter groups and AND/OR (via `FilterExpressionBuilder`), so the difference must be encoded explicitly.

## 4.3 Operators supported by the core (engine)

Specializations must produce valid filters for these `FilterOperator` values:
- `Contains`
- `NotContains`
- `StartsWith`
- `EndsWith`
- `Equals`
- `NotEquals`
- `GreaterThan`
- `GreaterThanOrEqual`
- `LessThan`
- `LessThanOrEqual`
- `Between` (value = `RangeValue`)
- `In` / `NotIn` (value = `IEnumerable`, the `In` logic is inferred/encoded by descriptor assembly)
- `IsNull` / `IsNotNull`

Semantics inferred from the engine / `FilterExpressionBuilder`:
- String comparisons: case-insensitive using `StringComparison.OrdinalIgnoreCase`.
- `Between`: inclusive min/max, expects `RangeValue`.
- `In`/`NotIn`: OR across equalities by value.
- `IsNull`/`IsNotNull`: supports nullable value types vs reference types.
- `propertyName` in filter descriptors can support nested paths via `.` (because the builder splits by `.`).

## 4.4 Sorting (ordering) and application order

Semantics inferred:
- Filters are applied first.
- Sorting is then applied on the result.

Multi-column sorting:
- Primary sort: replaces existing sort criteria (WPF: `SetSort`; Blazor: `SetSort` clears).
- Sub-sorts: add and replacement for the same field:
  - WPF: `ctx.AddSort` removes duplicates for the same property
  - Blazor: remove all existing sorts for the same `propertyName` then append

Nested `propertyName` for sorting:
- In both Blazor and WPF, sorting uses `GetProperty(sort.PropertyName)` without `.` navigation.
- Therefore, in practice, existing specializations support only simple property names for sorting.

## 4.5 Column type determination and available operators

The specialization must determine the contextual type of a column (equivalent to `FilterDataType`):
- `Text`: `string`
- `Number`: int/long/double/float/decimal/short (+ nullable equivalents)
- `Date`: `DateTime` / `DateTimeOffset`
- `Time`: `TimeSpan`
- `Boolean`: `bool`
- `Other`: fallback

When the type is determined, `AvailableOperators` must match the observed sets:
- Text: `Equals`/`NotEquals`/`Contains`/`NotContains`/`StartsWith`/`EndsWith`
- Number/Date/Time: `Equals`/`NotEquals`/`>`/`>=`/`<`/`<=`/`Between`

Note on `Time`:
- `Time` does not currently use hierarchy in the existing projects:
  - WPF has a TODO for Time tree (15-minute intervals).
  - Blazor treats Time as a flat list (no tree).

## 4.6 AddToExistingFilter + AccumulationMode (Union/Intersection)

Inferred semantics:
- The merge mode decides how the "current selection" (or custom evaluation result) is combined with the already active set.

Union (OR):
- `SelectedValues = SelectedValues ∪ newMatches`

Intersection (AND):
- `SelectedValues = SelectedValues ∩ newMatches`

Expected updates in the existing implementations:
- When merge is effective, `SelectAll` becomes false.
- The custom filter is reset:
  - Blazor and WPF reset `SelectedCustomOperator` and `CustomValue1/2`, and `IsCustomFilterExpanded`.

Guard on Intersection + empty custom values:
- WPF: when `AddToExistingFilter` is active, `AccumulationMode == Intersection`, and custom values v1/v2 are empty, immediate selection update is avoided to prevent wiping the previous state while the user is typing.
- Blazor: uses a similar guard via `string.IsNullOrEmpty(CustomValue1/2)` before updating.

## 4.7 Conversion and culture (points to keep identical or harmonize)

The existing projects show a potential divergence:
- For selection UI updates via evaluation (custom operator -> `IsSelected`), the `FilterEvaluator` converts values using `CultureInfo.InvariantCulture`.
- For final filter application (expression execution), `FilterExpressionBuilder` uses `Convert.ChangeType(value, targetType)` without an explicit provider (so conversion may depend on the runtime culture).

For a port, you must decide:
- document and keep the divergence identical, or
- harmonize by forcing conversion using the same culture as the descriptors.

## 5. Async support and pagination (`IAsyncDataProvider`)

## 5.1 Async contract

A specialization that supports async must expose/consume `IAsyncDataProvider<T>`:
- `FetchDataAsync(IFilterContext context, CancellationToken ct)` -> returns `PagedResult<T>` (Items, TotalCount, Page, PageSize)
- `FetchDistinctValuesAsync(string propertyName, string searchText, CancellationToken ct)` -> returns `IEnumerable<object>`

`IFilterContext` includes:
- `Descriptors` (filters)
- `SortDescriptors` (sorting)
- `Page` (current page, 1-based)
- `PageSize`

## 5.2 Expected behavior during user actions

Inferred from `FilterableDataGridViewModel<T>` (WPF):
- After `ApplyColumnFilter`, `ClearColumnFilter`, `ApplySort`, `AddSubSort`, `ClearSort`:
  - the page is reset to `1` (`ctx.Page = 1`)
  - a request is executed via `RefreshDataAsync()`

For async:
- `FilteredItems` comes from `AsyncDataProvider.FetchDataAsync(Context)` when `AsyncDataProvider != null`.

## 5.3 Loading indicator

Inferred:
- WPF: `IsLoading` shows `LoadingText` and hides the list via the inverse of visibility.
- Blazor: `IsLoading` shows `Loading...` and preserves the structure.

## 5.4 Blazor vs WPF difference for async data

WPF:
- full async support (remote filtering + remote distinct values).

Blazor:
- async support for rebuilding `FilterValues` via `SearchCommand` (distinct values provider).
- `DataFilterGrid.razor` applies filtering locally (via `ReflectionFilterEngine`) on `Items`.

For a port to another framework:
- decide if the specialization should offer a "local only" mode, or a "remote filtering" mode (often similar to WPF).

## 6. Persistence (snapshot) of filters and sorting

## 6.1 What must be supported as a feature

The specialization must allow:
- extracting a serializable snapshot of the current filters and sorting state
- restoring the state from a snapshot

The core provides:
- `FilterSnapshotBuilder.CreateSnapshot(IFilterContext)` -> `FilterSnapshot`
- `FilterSnapshotBuilder.RestoreSnapshot(IFilterContext, IFilterSnapshot)`

A `FilterSnapshot` includes:
- `Entries` (filters) -> may contain groups (hierarchy)
- `SortEntries` (sorting) -> ordered list

## 6.2 Existing implementations

WPF:
- `FilterableDataGridViewModel<T>` implements `ExtractSnapshot()` and `RestoreSnapshot(IFilterSnapshot)`.
- `CollectionViewFilterAdapter<T>` implements snapshot extraction/restoration as well.
- Restoration:
  - cleans entries to ignore non-filterable properties via `FilterableProperties`.

Blazor:
- there is no snapshot API exposed in the existing Blazor components.

For a port:
- include snapshot support at least at the WPF-equivalent level, or explicitly mark it as not implemented if targeting only the current local-only Blazor mode.

## 7. Customization (theming, styles, localization)

## 7.1 WPF: theming via ResourceDictionary

An equivalent WPF specialization must support:
- global theme replacement via `Generic.xaml` + `FilterLightTheme.xaml` or `FilterDarkTheme.xaml`
- style resource overrides via `DynamicResource` / `StaticResource`

Resource keys actually used in the code:
- `FilterPopupBackground`
- `FilterPopupForeground`
- `FilterPopupBorder`
- `FilterPopupMaxHeight`
- `FilterButtonStyle`
- `FilterCheckBoxStyle`
- `FilterButtonActiveColor`
- `FilterButtonInactiveColor`

`ColumnFilterButton` also supports customization:
- `IconTemplate` (DataTemplate)
- `ActiveBrush` / `InactiveBrush`

## 7.2 WPF: localization of text

The popup shows texts (buttons, labels) and descriptions via `FilterResources`:
- `OperatorToLocalizedDescriptionConverter` maps a `FilterOperator` to the key `FilterOperator_{OperatorName}`.
- `AccumulationModeToLocalizedDescriptionConverter` maps `AccumulationMode.Union/Intersection` to localized texts.

For portability:
- provide an equivalent localization mechanism or a text substitution mechanism.

## 7.3 Blazor: customization via CSS

An equivalent Blazor specialization must provide stable CSS classes (prefix `df-`) and/or CSS variables.

In `DataFilter.css`, we observe:
- `.df-popup-container` uses `background` and `border` via CSS variables (e.g. `--df-popup-bg`, `--df-popup-border`).
- `.df-resize-handle` for the resizing area.
- `.df-column-header-button.active` for the active state.

Rule:
- a Blazor port must keep a class structure that is easy to override.

Blazor localization:
- button text in `FilterPopup.razor` is hardcoded in English (e.g. "Sort A to Z", "Advanced Filter", "OK", "Clear").
- for multi-language portability, a port can add a localization layer.

## 8. Checklist: features to implement (operational summary)

1. Per-column filter button with active/inactive state and popup opening.
2. Resizable popup with outside-click closing.
3. Multi-column sorting integrated in the popup (set vs add sub-sort).
4. Search bar that triggers distinct value refresh and shows a loading state.
5. Multi-selection list of distinct values with `Select All` tri-state.
6. Collapsible advanced/custom filter section:
   - operators based on column type
   - 1 custom value (custom operator) or 2 values (`Between`)
   - real-time synchronization of the selection
7. Add-to-existing selection:
   - checkbox enable
   - `Union` / `Intersection` mode
   - reset the custom operator when merge becomes effective
8. Handle null values ("Blanks") in the list.
9. Filter semantics produced from `ExcelFilterState`:
   - selection -> `In`
   - `Between` -> `RangeValue`
   - custom operator -> `FilterOperator.*`
   - document the selection+custom combination semantics (WPF vs Blazor)
10. Apply sorting after filtering.
11. Async support:
   - at minimum: async distinct values
   - ideally also: remote filtering via `IAsyncDataProvider<T>` + pagination (`Page`/`PageSize`)
12. Snapshot support (filters + sorting) at the specialization level (at least WPF-equivalent).
13. Customization:
   - WPF: ResourceDictionary themes + override of styles/resource keys
   - Blazor: overridable CSS prefix `df-`
14. Date hierarchy support (year/month/day) and flat list fallback for Time (or explicitly documented limitation).

## 9. Compatibility notes to keep (Blazor vs WPF)

- Selection list vs custom operator combination semantics:
  - WPF: `CustomOperator` takes precedence over manual selection
  - Blazor: both can be combined in an AND
- Clear behavior:
  - WPF: clear closes the popup
  - Blazor: clear does not close the popup
- Debounce:
  - WPF: `AsyncFilterBehavior` exists (debounce), but is not active in the current XAML
  - Blazor: no debounce in the current UI
- Sorting and nested property paths:
  - filtering: supports paths with `.` (by builder)
  - sorting: supports only simple properties (no `.` navigation via `GetProperty`)
- Culture:
  - selection UI: `InvariantCulture` (`FilterEvaluator`)
  - final evaluation: `Convert.ChangeType` (runtime culture)

# Features.md - Feature Contract for DataFilter Specialization Projects

This document defines the feature contract for the DataFilter specialization projects, so the project can be ported to other frameworks/UI layers.

It is derived only from:
- `src/DataFilter.Blazor`
- `src/DataFilter.Wpf`

Where Blazor and WPF differ, the difference is documented explicitly so future ports can either reproduce the expected behavior or deliberately unify it.

## 1. Definitions (used terms)

1. **Column**: a field shown in the grid/table (header) and filtered independently.
2. **Per-column filter state**: `ExcelFilterState` (from `DataFilter.Filtering.ExcelLike.Models`).
3. **Distinct values list**: `ExcelFilterState.DistinctValues` and its UI representation via `FilterValueItem` (hierarchy possible for dates).
4. **Custom/advanced filter**: the operator selected in the "Advanced Filter" section (e.g. `Contains`, `Between`).
5. **Add to existing filter**: a mode that merges criteria with an already active filter for the same column, using an accumulation semantics of `Union` or `Intersection`.
6. **Sorting**: direction selection (A-Z / Z-A) and multi-column ordering (sub-sorts).
7. **Snapshot**: a serializable representation of the whole state (filters + sorting) via `IFilterSnapshot`.

## 2. Functional contract (UI + interaction) expected from any specialization

## 2.1 Filter button in the column header (per column)

A specialization must provide an interactive element placed in the column header:
- Show a filter active vs inactive indicator.
- Open/close a dedicated filtering panel (popup).
- Close when the user clicks outside the popup ("outside click" behavior).

Examples in the existing projects:
- WPF: `ColumnFilterButton` is injected into each header via `FilterableColumnHeaderBehavior`.
- Blazor: `ColumnFilterButton` appears in the headers of `DataFilterGrid.razor`.

## 2.2 Filtering popup (Excel-like filtering panel per column)

A specialization must provide a popup/panel with the following sections:

### 2.2.1 Sorting section

Required controls (4 commands):
- `Sort A to Z` (ascending sort direction).
- `Sort Z to A` (descending sort direction).
- `Add Sort A to Z` (add a sort criterion as a sub-sort).
- `Add Sort Z to A` (add a sort criterion as a sub-sort).

Semantics:
- Sorting must be applied after filtering (as done in the existing implementations).
- The specialization must support multi-column ordering (primary sort + sub-sorts).
- Adding a sub-sort must replace any existing sort for the same property (in the existing projects, criteria for the same field are replaced before adding).

Examples:
- Blazor: `FilterPopup.razor` calls the commands on `IBlazorColumnFilterViewModel`.
- WPF: `FilterPopup.xaml` calls `SortAscendingCommand`, `SortDescendingCommand`, `AddSubSortAscendingCommand`, `AddSubSortDescendingCommand`.

### 2.2.2 Search bar (search-as-you-type / async distinct values)

A specialization must support:
- Text entry in a search area.
- Search must trigger a refresh of the distinct values list displayed in the popup.
- A loading indicator must be shown during the refresh.

Implementation notes inferred from the code:
- WPF provides `AsyncFilterBehavior` (debounce) but it is not wired in `FilterPopup.xaml` (the behavior exists, but is unused/commented there).
- Blazor triggers logic on `@oninput` (no debounce inside the component itself).

### 2.2.3 Add to existing filter + AccumulationMode

A specialization must expose a merge mode:
- A checkbox `Add selection to filter` / `AddToExistingFilter`.
- When enabled, a select control must let the user choose the merge semantics `AccumulationMode`:
  - `Union` (logical OR accumulation)
  - `Intersection` (logical AND accumulation)

Expected behavior:
- In merge mode, the new selection (or evaluation result) must be merged with the existing set according to `Union`/`Intersection`.
- In merge mode, the "custom/advanced filter" section is reset in the existing implementations (see section 4).

### 2.2.4 Advanced/custom filter (contextual operators)

A specialization must provide a collapsible/expandable "Advanced Filter" section:
- Show an `AvailableOperators` list dependent on the column type (see section 4.5).
- Provide input fields for `CustomValue1` and (optionally) `CustomValue2` for the `Between` case.
- Changing the operator (and the values) must update the selection UI (checkboxes) in real time.

Available operators inferred from the view model code:
- Text type:
  - `Equals`
  - `NotEquals`
  - `Contains`
  - `NotContains`
  - `StartsWith`
  - `EndsWith`
- Number / date / time type:
  - `Equals`
  - `NotEquals`
  - `GreaterThan`
  - `GreaterThanOrEqual`
  - `LessThan`
  - `LessThanOrEqual`
  - `Between` (using `CustomValue1` / `CustomValue2`)

### 2.2.5 Multi-selection distinct values list (checkboxes)

A specialization must display a hierarchical list/control:
- Multi-select checkboxes for `FilterValueItem`.
- Support a `Select All` checkbox:
  - A tri-state boolean (checked / unchecked / indeterminate) via `bool?` (Blazor and WPF).
- For dates: a hierarchical (tree) presentation.

UI deductions:
- Blazor: `FilterValueItemView.razor` manages `IsExpanded` with an explicit (+/-) expander button for nodes that have children.
- WPF: `FilterPopup.xaml` uses a `TreeView` with `Children`; expansion relies on the internal `TreeViewItem` expansion behavior (the binding `IsExpanded` from the model is not used in XAML).

### 2.2.6 Handling "Blanks" (null values)

The list must include an entry representing null values:
- Blazor displays `(Blanks)`.
- WPF displays `FilterResources.Blanks` (from localized resources).

Selection semantics:
- The "blanks" entry value is `null` stored in `FilterValueItem.Value`.
- Selection must be correctly included/excluded in the (optional) `SelectedValues` set.

### 2.2.7 OK / Clear actions

A specialization must provide:
- `OK` button:
  - apply the filter to the data source
  - close the popup (in both Blazor and WPF)
- `Clear` button:
  - reset the per-column filter state

Observed difference important for ports:
- WPF: `Clear` closes the popup via the `ColumnFilterViewModel.OnClear` event.
- Blazor: `Clear` resets the state but does not close the popup in `FilterPopup.razor` (closing is only triggered on `Apply`).

### 2.2.8 Popup resizing

A specialization must allow resizing the popup:
- Blazor: via `DataFilterInterops.initResizable` (drag handle + clamp to viewport).
- WPF: via a `Thumb` in `FilterPopup.xaml` (drag delta for width/height).

## 3. Data model contract (internal UI)

## 3.1 `ExcelFilterState` (per-column filter state)

The specialization must manage a state equivalent to `ExcelFilterState`:
- `SearchText: string`
- `UseWildcards: bool` (default false; not currently used as an explicit UI element in the Blazor/WPF projects)
- `DistinctValues: List<object>` (values shown in the list)
- `SelectedValues: HashSet<object>` (explicit selected values)
- `SelectAll: bool` (selects all visible distinct values when true)
- `CustomOperator: FilterOperator?`
- `CustomValue1: object?`
- `CustomValue2: object?`
- method `Clear()` that resets:
  - `SearchText`
  - empties `DistinctValues` and `SelectedValues`
  - sets `SelectAll=true`
  - sets `CustomOperator` / `CustomValue*` to null

## 3.2 `FilterValueItem` (checkbox tree node in the UI)

The specialization must provide a node representation:
- `Value: object?`
- `DisplayText: string`
- `Children: ObservableCollection<FilterValueItem>` (0 children = leaf)
- `IsSelected: bool?` (tri-state support for indeterminate)
- `IsNull: bool?` (derived from `IsSelected == null` in the model)
- `IsExpanded: bool` (used explicitly by Blazor; WPF does not depend on it)
- method `GetSelectedValues(...)`:
  - adds leaf nodes where `IsSelected == true`
  - ignores leaf nodes where `IsSelected == null` (indeterminate)

Selection propagation:
- When a child node changes, parent nodes must recompute their state:
  - allSelected/allUnselected => `IsSelected` true/false
  - otherwise => `IsSelected` null

## 4. Mapping contract (UI state -> filtered data)

## 4.1 Distinct values + search

The specialization must be able to:
- Extract distinct values for a column to feed the UI.
- Apply filtering of those distinct values based on `SearchText`.

Semantics inferred from the code:

### WPF local / adapter behavior
- If `searchText` contains wildcards `*` or `?`:
  - wildcard matcher is case-insensitive (`WildcardMatcher`).
- Otherwise:
  - use a case-insensitive "starts-with" match.

### WPF async behavior
- `IAsyncDataProvider.FetchDistinctValuesAsync(propertyName, searchText, ...)` is responsible for applying the "search -> distinct values" filtering.

### Blazor behavior
- The refresh of the distinct values list is asynchronous via `SearchCommand` (on `IBlazorColumnFilterViewModel`).
- The current `DataFilterGrid.razor` implementation ignores `SearchText` in the distinct values provider (demo behavior). For a real port, the specialization must provide a provider that respects `SearchText`; otherwise the search-as-you-type feature does not work correctly.

## 4.2 Building filters from `ExcelFilterState`

Two different approaches are visible in the existing projects:

### 4.2.1 WPF: `ExcelFilterDescriptor` (custom precedence vs selection)

In WPF, the per-column filter is applied via `ExcelFilterDescriptor`:
- If `State.CustomOperator != null`, then the descriptor:
  - adds a criterion for `CustomOperator` with `CustomValue1` (and a `RangeValue` when `Between`)
  - does not automatically add an `In` criterion for `SelectedValues` (the "manual selection" logic is in an else-if branch).
- If `State.CustomOperator == null`, then:
  - the list selection is used via `FilterOperator.In` and `State.SelectedValues` (unless `SelectAll` is true and/or unless the selection corresponds to the full set).

Consequence:
- When the user enables "Advanced Filter", checkbox selection can be synchronized visually, but the final filtering logic gives priority to `CustomOperator`.

### 4.2.2 Blazor: direct combination of `In` + custom (AND composition)

In Blazor, `DataFilterGrid.razor` assembles descriptors manually:
- if `SelectAll == false` and `SelectedValues.Count > 0`:
  - add a `FilterOperator.In` criterion with the `SelectedValues`
- if `CustomOperator` is defined:
  - add a custom criterion (including `Between` with `RangeValue`)
- criteria are combined into a `FilterGroup` with `LogicalOperator.And`.

Consequence:
- Blazor may combine `In(SelectedValues)` and the custom criterion in an AND.

### 4.2.3 Recommendation for ports

For a robust port, the port must document/decide which combination semantics to adopt:
- WPF compatibility (custom precedence over manual selection) OR
- Blazor compatibility (AND: selection list + custom when both are active).

The core engine supports filter groups and AND/OR (via `FilterExpressionBuilder`), so the difference must be encoded explicitly.

## 4.3 Operators supported by the core (engine)

Specializations must produce valid filters for these `FilterOperator` values:
- `Contains`
- `NotContains`
- `StartsWith`
- `EndsWith`
- `Equals`
- `NotEquals`
- `GreaterThan`
- `GreaterThanOrEqual`
- `LessThan`
- `LessThanOrEqual`
- `Between` (value = `RangeValue`)
- `In` / `NotIn` (value = `IEnumerable`, the `In` logic is inferred/encoded by descriptor assembly)
- `IsNull` / `IsNotNull`

Semantics inferred from the engine / `FilterExpressionBuilder`:
- String comparisons: case-insensitive using `StringComparison.OrdinalIgnoreCase`.
- `Between`: inclusive min/max, expects `RangeValue`.
- `In`/`NotIn`: OR across equalities by value.
- `IsNull`/`IsNotNull`: supports nullable value types vs reference types.
- `propertyName` in filter descriptors can support nested paths via `.` (because the builder splits by `.`).

## 4.4 Sorting (ordering) and application order

Semantics inferred:
- Filters are applied first.
- Sorting is then applied on the result.

Multi-column sorting:
- Primary sort: replaces existing sort criteria (WPF: `SetSort`; Blazor: `SetSort` clears).
- Sub-sorts: add and replacement for the same field:
  - WPF: `ctx.AddSort` removes duplicates for the same property
  - Blazor: remove all existing sorts for the same `propertyName` then append

Nested `propertyName` for sorting:
- In both Blazor and WPF, sorting uses `GetProperty(sort.PropertyName)` without `.` navigation.
- Therefore, in practice, existing specializations support only simple property names for sorting.

## 4.5 Column type determination and available operators

The specialization must determine the contextual type of a column (equivalent to `FilterDataType`):
- `Text`: `string`
- `Number`: int/long/double/float/decimal/short (+ nullable equivalents)
- `Date`: `DateTime` / `DateTimeOffset`
- `Time`: `TimeSpan`
- `Boolean`: `bool`
- `Other`: fallback

When the type is determined, `AvailableOperators` must match the observed sets:
- Text: `Equals`/`NotEquals`/`Contains`/`NotContains`/`StartsWith`/`EndsWith`
- Number/Date/Time: `Equals`/`NotEquals`/`>`/`>=`/`<`/`<=`/`Between`

Note on `Time`:
- `Time` does not currently use hierarchy in the existing projects:
  - WPF has a TODO for Time tree (15-minute intervals).
  - Blazor treats Time as a flat list (no tree).

## 4.6 AddToExistingFilter + AccumulationMode (Union/Intersection)

Inferred semantics:
- The merge mode decides how the "current selection" (or custom evaluation result) is combined with the already active set.

Union (OR):
- `SelectedValues = SelectedValues ∪ newMatches`

Intersection (AND):
- `SelectedValues = SelectedValues ∩ newMatches`

Expected updates in the existing implementations:
- When merge is effective, `SelectAll` becomes false.
- The custom filter is reset:
  - Blazor and WPF reset `SelectedCustomOperator` and `CustomValue1/2`, and `IsCustomFilterExpanded`.

Guard on Intersection + empty custom values:
- WPF: when `AddToExistingFilter` is active, `AccumulationMode == Intersection`, and custom values v1/v2 are empty, immediate selection update is avoided to prevent wiping the previous state while the user is typing.
- Blazor: uses a similar guard via `string.IsNullOrEmpty(CustomValue1/2)` before updating.

## 4.7 Conversion and culture (points to keep identical or harmonize)

The existing projects show a potential divergence:
- For selection UI updates via evaluation (custom operator -> `IsSelected`), the `FilterEvaluator` converts values using `CultureInfo.InvariantCulture`.
- For final filter application (expression execution), `FilterExpressionBuilder` uses `Convert.ChangeType(value, targetType)` without an explicit provider (so conversion may depend on the runtime culture).

For a port, you must decide:
- document and keep the divergence identical, or
- harmonize by forcing conversion using the same culture as the descriptors.

## 5. Async support and pagination (`IAsyncDataProvider`)

## 5.1 Async contract

A specialization that supports async must expose/consume `IAsyncDataProvider<T>`:
- `FetchDataAsync(IFilterContext context, CancellationToken ct)` -> returns `PagedResult<T>` (Items, TotalCount, Page, PageSize)
- `FetchDistinctValuesAsync(string propertyName, string searchText, CancellationToken ct)` -> returns `IEnumerable<object>`

`IFilterContext` includes:
- `Descriptors` (filters)
- `SortDescriptors` (sorting)
- `Page` (current page, 1-based)
- `PageSize`

## 5.2 Expected behavior during user actions

Inferred from `FilterableDataGridViewModel<T>` (WPF):
- After `ApplyColumnFilter`, `ClearColumnFilter`, `ApplySort`, `AddSubSort`, `ClearSort`:
  - the page is reset to `1` (`ctx.Page = 1`)
  - a request is executed via `RefreshDataAsync()`

For async:
- `FilteredItems` comes from `AsyncDataProvider.FetchDataAsync(Context)` when `AsyncDataProvider != null`.

## 5.3 Loading indicator

Inferred:
- WPF: `IsLoading` shows `LoadingText` and hides the list via the inverse of visibility.
- Blazor: `IsLoading` shows `Loading...` and preserves the structure.

## 5.4 Blazor vs WPF difference for async data

WPF:
- full async support (remote filtering + remote distinct values).

Blazor:
- async support for rebuilding `FilterValues` via `SearchCommand` (distinct values provider).
- `DataFilterGrid.razor` applies filtering locally (via `ReflectionFilterEngine`) on `Items`.

For a port to another framework:
- decide if the specialization should offer a "local only" mode, or a "remote filtering" mode (often similar to WPF).

## 6. Persistence (snapshot) of filters and sorting

## 6.1 What must be supported as a feature

The specialization must allow:
- extracting a serializable snapshot of the current filters and sorting state
- restoring the state from a snapshot

The core provides:
- `FilterSnapshotBuilder.CreateSnapshot(IFilterContext)` -> `FilterSnapshot`
- `FilterSnapshotBuilder.RestoreSnapshot(IFilterContext, IFilterSnapshot)`

A `FilterSnapshot` includes:
- `Entries` (filters) -> may contain groups (hierarchy)
- `SortEntries` (sorting) -> ordered list

## 6.2 Existing implementations

WPF:
- `FilterableDataGridViewModel<T>` implements `ExtractSnapshot()` and `RestoreSnapshot(IFilterSnapshot)`.
- `CollectionViewFilterAdapter<T>` implements snapshot extraction/restoration as well.
- Restoration:
  - cleans entries to ignore non-filterable properties via `FilterableProperties`.

Blazor:
- there is no snapshot API exposed in the existing Blazor components.

For a port:
- include snapshot support at least at the WPF-equivalent level, or explicitly mark it as not implemented if targeting only the current local-only Blazor mode.

## 7. Customization (theming, styles, localization)

## 7.1 WPF: theming via ResourceDictionary

An equivalent WPF specialization must support:
- global theme replacement via `Generic.xaml` + `FilterLightTheme.xaml` or `FilterDarkTheme.xaml`
- style resource overrides via `DynamicResource` / `StaticResource`

Resource keys actually used in the code:
- `FilterPopupBackground`
- `FilterPopupForeground`
- `FilterPopupBorder`
- `FilterPopupMaxHeight`
- `FilterButtonStyle`
- `FilterCheckBoxStyle`
- `FilterButtonActiveColor`
- `FilterButtonInactiveColor`

`ColumnFilterButton` also supports customization:
- `IconTemplate` (DataTemplate)
- `ActiveBrush` / `InactiveBrush`

## 7.2 WPF: localization of text

The popup shows texts (buttons, labels) and descriptions via `FilterResources`:
- `OperatorToLocalizedDescriptionConverter` maps a `FilterOperator` to the key `FilterOperator_{OperatorName}`.
- `AccumulationModeToLocalizedDescriptionConverter` maps `AccumulationMode.Union/Intersection` to localized texts.

For portability:
- provide an equivalent localization mechanism or a text substitution mechanism.

## 7.3 Blazor: customization via CSS

An equivalent Blazor specialization must provide stable CSS classes (prefix `df-`) and/or CSS variables.

In `DataFilter.css`, we observe:
- `.df-popup-container` uses `background` and `border` via CSS variables (e.g. `--df-popup-bg`, `--df-popup-border`).
- `.df-resize-handle` for the resizing area.
- `.df-column-header-button.active` for the active state.

Rule:
- a Blazor port must keep a class structure that is easy to override.

Blazor localization:
- button text in `FilterPopup.razor` is hardcoded in English (e.g. "Sort A to Z", "Advanced Filter", "OK", "Clear").
- for multi-language portability, a port can add a localization layer.

## 8. Checklist: features to implement (operational summary)

1. Per-column filter button with active/inactive state and popup opening.
2. Resizable popup with outside-click closing.
3. Multi-column sorting integrated in the popup (set vs add sub-sort).
4. Search bar that triggers distinct value refresh and shows a loading state.
5. Multi-selection list of distinct values with `Select All` tri-state.
6. Collapsible advanced/custom filter section:
   - operators based on column type
   - 1 custom value (custom operator) or 2 values (`Between`)
   - real-time synchronization of the selection
7. Add-to-existing selection:
   - checkbox enable
   - `Union` / `Intersection` mode
   - reset the custom operator when merge becomes effective
8. Handle null values ("Blanks") in the list.
9. Filter semantics produced from `ExcelFilterState`:
   - selection -> `In`
   - `Between` -> `RangeValue`
   - custom operator -> `FilterOperator.*`
   - document the selection+custom combination semantics (WPF vs Blazor)
10. Apply sorting after filtering.
11. Async support:
   - at minimum: async distinct values
   - ideally also: remote filtering via `IAsyncDataProvider<T>` + pagination (`Page`/`PageSize`)
12. Snapshot support (filters + sorting) at the specialization level (at least WPF-equivalent).
13. Customization:
   - WPF: ResourceDictionary themes + override of styles/resource keys
   - Blazor: overridable CSS prefix `df-`
14. Date hierarchy support (year/month/day) and flat list fallback for Time (or explicitly documented limitation).

## 9. Compatibility notes to keep (Blazor vs WPF)

- Selection list vs custom operator combination semantics:
  - WPF: `CustomOperator` takes precedence over manual selection
  - Blazor: both can be combined in an AND
- Clear behavior:
  - WPF: clear closes the popup
  - Blazor: clear does not close the popup
- Debounce:
  - WPF: `AsyncFilterBehavior` exists (debounce), but is not active in the current XAML
  - Blazor: no debounce in the current UI
- Sorting and nested property paths:
  - filtering: supports paths with `.` (by builder)
  - sorting: supports only simple properties (no `.` navigation via `GetProperty`)
- Culture:
  - selection UI: `InvariantCulture` (`FilterEvaluator`)
  - final evaluation: `Convert.ChangeType` (runtime culture)

# Features.md - Feature Contract for DataFilter Specialization Projects

This document defines the feature contract for the DataFilter specialization projects, so the project can be ported to other frameworks/host UI layers.

It is derived only from:
- `src/DataFilter.Blazor`
- `src/DataFilter.Wpf`

Where Blazor and WPF differ, the difference is documented explicitly so future ports can either reproduce the expected behavior or deliberately unify it.

## 1. Definitions (used terms)

1. **Column**: a field shown in the grid/table (header) and filtered independently.
2. **Per-column filter state**: `ExcelFilterState` (from `DataFilter.Filtering.ExcelLike.Models`).
3. **Distinct values list**: `ExcelFilterState.DistinctValues` and its UI representation via `FilterValueItem` (hierarchy possible for dates).
4. **Custom/advanced filter**: the operator selected in the "Advanced Filter" section (e.g. `Contains`, `Between`).
5. **Add to existing filter**: a mode that merges criteria with an already active filter for the same column, using an accumulation semantics of `Union` or `Intersection`.
6. **Sorting**: direction selection (A-Z / Z-A) and multi-column ordering (sub-sorts).
7. **Snapshot**: a serializable representation of the whole state (filters + sorting) via `IFilterSnapshot`.

## 2. Functional contract (UI + interaction) expected from any specialization

## 2.1 Filter button in the column header (per column)

A specialization must provide an interactive element placed in the column header:
- Show a filter active vs inactive indicator.
- Open/close a dedicated filtering panel (popup).
- Close when the user clicks outside the popup ("outside click" behavior).

Examples in the existing projects:
- WPF: `ColumnFilterButton` is injected into each header via `FilterableColumnHeaderBehavior`.
- Blazor: `ColumnFilterButton` appears in the headers of `DataFilterGrid.razor`.

## 2.2 Filtering popup (Excel-like filtering panel per column)

A specialization must provide a popup/panel with the following sections:

### 2.2.1 Sorting section

Required controls (4 commands):
- `Sort A to Z` (ascending sort direction).
- `Sort Z to A` (descending sort direction).
- `Add Sort A to Z` (add a sort criterion as a sub-sort).
- `Add Sort Z to A` (add a sort criterion as a sub-sort).

Semantics:
- Sorting must be applied after filtering (in the existing implementations).
- The specialization must support multi-column ordering (primary sort + sub-sorts).
- Adding a sub-sort must replace any existing sort for the same property (in the existing projects, criteria for the same field are replaced before adding).

Examples:
- Blazor: `FilterPopup.razor` calls the commands on `IBlazorColumnFilterViewModel`.
- WPF: `FilterPopup.xaml` calls `SortAscendingCommand`, `SortDescendingCommand`, `AddSubSortAscendingCommand`, `AddSubSortDescendingCommand`.

### 2.2.2 Search bar (search-as-you-type / async distinct values)

A specialization must support:
- Text entry in a search area.
- Search must trigger a refresh of the distinct values list displayed in the popup.
- A loading indicator must be shown during the refresh.

Implementation notes inferred from the code:
- WPF provides `AsyncFilterBehavior` (debounce) but it is not wired in `FilterPopup.xaml` (the behavior exists, but is commented/unused there).
- Blazor triggers logic on `@oninput` (no debounce inside the component itself).

### 2.2.3 Add to existing filter + AccumulationMode

A specialization must expose a merge mode:
- A checkbox `Add selection to filter` / `AddToExistingFilter`.
- When enabled, a select control must let the user choose the merge semantics `AccumulationMode`:
  - `Union` (logical OR accumulation)
  - `Intersection` (logical AND accumulation)

Expected behavior:
- In merge mode, the new selection (or evaluation result) must be merged with the existing set according to `Union`/`Intersection`.
- In merge mode, the "custom/advanced filter" section is reset in the existing implementations (see section 4).

### 2.2.4 Advanced/custom filter (contextual operators)

A specialization must provide a collapsible/expandable "Advanced Filter" section:
- Show an `AvailableOperators` list dependent on the column type (see section 4.5).
- Provide input fields for `CustomValue1` and (optionally) `CustomValue2` for the `Between` case.
- Changing the operator (and the values) must update the selection UI (checkboxes) in real time.

Available operators inferred from the view model code:
- Text type:
  - `Equals`
  - `NotEquals`
  - `Contains`
  - `NotContains`
  - `StartsWith`
  - `EndsWith`
- Number / date / time type:
  - `Equals`
  - `NotEquals`
  - `GreaterThan`
  - `GreaterThanOrEqual`
  - `LessThan`
  - `LessThanOrEqual`
  - `Between` (using `CustomValue1` / `CustomValue2`)

### 2.2.5 Multi-selection distinct values list (checkboxes)

A specialization must display a hierarchical list/control:
- Multi-select checkboxes for `FilterValueItem`.
- Support a `Select All` checkbox:
  - A tri-state boolean (checked / unchecked / indeterminate) via `bool?` (Blazor and WPF).
- For dates: a hierarchical (tree) presentation.

UI deductions:
- Blazor: `FilterValueItemView.razor` manages `IsExpanded` with an explicit (+/-) expander button for nodes that have children.
- WPF: `FilterPopup.xaml` uses a `TreeView` with `Children`; expansion relies on the internal `TreeViewItem` expansion behavior (the binding `IsExpanded` from the model is not used in XAML).

### 2.2.6 Handling "Blanks" (null values)

The list must include an entry representing null values:
- Blazor displays `(Blanks)`.
- WPF displays `FilterResources.Blanks` (from localized resources).

Selection semantics:
- The "blanks" entry value is `null` stored in `FilterValueItem.Value`.
- Selection must be correctly included/excluded in the (optional) `SelectedValues` set.

### 2.2.7 OK / Clear actions

A specialization must provide:
- `OK` button:
  - apply the filter to the data source
  - close the popup (in both Blazor and WPF)
- `Clear` button:
  - reset the per-column filter state

Observed difference important for ports:
- WPF: `Clear` closes the popup via the `ColumnFilterViewModel.OnClear` event.
- Blazor: `Clear` resets the state but does not close the popup in `FilterPopup.razor` (closing is only triggered on `Apply`).

### 2.2.8 Popup resizing

A specialization must allow resizing the popup:
- Blazor: via `DataFilterInterops.initResizable` (drag handle + clamp to viewport).
- WPF: via a `Thumb` in `FilterPopup.xaml` (drag delta for width/height).

## 3. Data model contract (internal UI)

## 3.1 `ExcelFilterState` (per-column filter state)

The specialization must manage a state equivalent to `ExcelFilterState`:
- `SearchText: string`
- `UseWildcards: bool` (default false; not currently used as an explicit UI element in the Blazor/WPF projects)
- `DistinctValues: List<object>` (values shown in the list)
- `SelectedValues: HashSet<object>` (explicit selected values)
- `SelectAll: bool` (selects all visible distinct values when true)
- `CustomOperator: FilterOperator?`
- `CustomValue1: object?`
- `CustomValue2: object?`
- method `Clear()` that resets:
  - `SearchText`
  - empties `DistinctValues` and `SelectedValues`
  - sets `SelectAll=true`
  - sets `CustomOperator` / `CustomValue*` to null

## 3.2 `FilterValueItem` (checkbox tree node in the UI)

The specialization must provide a node representation:
- `Value: object?`
- `DisplayText: string`
- `Children: ObservableCollection<FilterValueItem>` (0 children = leaf)
- `IsSelected: bool?` (tri-state support for indeterminate)
- `IsNull: bool?` (derived from `IsSelected == null` in the model)
- `IsExpanded: bool` (used explicitly by Blazor; WPF does not depend on it)
- method `GetSelectedValues(...)`:
  - adds leaf nodes where `IsSelected == true`
  - ignores leaf nodes where `IsSelected == null` (indeterminate)

Selection propagation:
- When a child node changes, parent nodes must recompute their state:
  - allSelected/allUnselected => `IsSelected` true/false
  - otherwise => `IsSelected` null

## 4. Mapping contract (UI state -> filtered data)

## 4.1 Distinct values + search

The specialization must be able to:
- Extract distinct values for a column to feed the UI.
- Apply filtering of those distinct values based on `SearchText`.

Semantics inferred from the code:

### WPF local / adapter behavior
- If `searchText` contains wildcards `*` or `?`:
  - wildcard matcher is case-insensitive (`WildcardMatcher`).
- Otherwise:
  - use a case-insensitive "starts-with" match.

### WPF async behavior
- `IAsyncDataProvider.FetchDistinctValuesAsync(propertyName, searchText, ...)` is responsible for applying the "search -> distinct values" filtering.

### Blazor behavior
- The refresh of the distinct values list is asynchronous via `SearchCommand` (on `IBlazorColumnFilterViewModel`).
- The current `DataFilterGrid.razor` implementation ignores `SearchText` in the distinct values provider (demo behavior). For a real port, the specialization must provide a provider that respects `SearchText`; otherwise the search-as-you-type feature does not work correctly.

## 4.2 Building filters from `ExcelFilterState`

Two different approaches are visible in the existing projects:

### 4.2.1 WPF: `ExcelFilterDescriptor` (custom precedence vs selection)

In WPF, the per-column filter is applied via `ExcelFilterDescriptor`:
- If `State.CustomOperator != null`, then the descriptor:
  - adds a criterion for `CustomOperator` with `CustomValue1` (and a `RangeValue` when `Between`)
  - does not automatically add an `In` criterion for `SelectedValues` (the "manual selection" logic is in an else-if branch).
- If `State.CustomOperator == null`, then:
  - the list selection is used via `FilterOperator.In` and `State.SelectedValues` (unless `SelectAll` is true and/or unless the selection corresponds to the full set).

Consequence:
- When the user enables "Advanced Filter", checkbox selection can be synchronized visually, but the final filtering logic gives priority to `CustomOperator`.

### 4.2.2 Blazor: direct combination of `In` + custom (AND composition)

In Blazor, `DataFilterGrid.razor` assembles descriptors manually:
- if `SelectAll == false` and `SelectedValues.Count > 0`:
  - add a `FilterOperator.In` criterion with the `SelectedValues`
- if `CustomOperator` is defined:
  - add a custom criterion (including `Between` with `RangeValue`)
- criteria are combined into a `FilterGroup` with `LogicalOperator.And`.

Consequence:
- Blazor may combine `In(SelectedValues)` and the custom criterion in an AND.

### 4.2.3 Recommendation for ports

For a robust port, the port must document/decide which combination semantics to adopt:
- WPF compatibility (custom precedence over manual selection) OR
- Blazor compatibility (AND: selection list + custom when both are active).

The core engine supports filter groups and AND/OR (via `FilterExpressionBuilder`), so the difference must be encoded explicitly.

## 4.3 Operators supported by the core (engine)

Specializations must produce valid filters for these `FilterOperator` values:
- `Contains`
- `NotContains`
- `StartsWith`
- `EndsWith`
- `Equals`
- `NotEquals`
- `GreaterThan`
- `GreaterThanOrEqual`
- `LessThan`
- `LessThanOrEqual`
- `Between` (value = `RangeValue`)
- `In` / `NotIn` (value = `IEnumerable`, the `In` logic is inferred/encoded by descriptor assembly)
- `IsNull` / `IsNotNull`

Semantics inferred from the engine / `FilterExpressionBuilder`:
- String comparisons: case-insensitive using `StringComparison.OrdinalIgnoreCase`.
- `Between`: inclusive min/max, expects `RangeValue`.
- `In`/`NotIn`: OR across equalities by value.
- `IsNull`/`IsNotNull`: supports nullable value types vs reference types.
- `propertyName` in filter descriptors can support nested paths via `.` (because the builder splits by `.`).

## 4.4 Sorting (ordering) and application order

Semantics inferred:
- Filters are applied first.
- Sorting is then applied on the result.

Multi-column sorting:
- Primary sort: replaces existing sort criteria (WPF: `SetSort`; Blazor: `SetSort` clears).
- Sub-sorts: add and replacement for the same field:
  - WPF: `ctx.AddSort` removes duplicates for the same property
  - Blazor: remove all existing sorts for the same `propertyName` then append

Nested `propertyName` for sorting:
- In both Blazor and WPF, sorting uses `GetProperty(sort.PropertyName)` without `.` navigation.
- Therefore, in practice, existing specializations support only simple property names for sorting.

## 4.5 Column type determination and available operators

The specialization must determine the contextual type of a column (equivalent to `FilterDataType`):
- `Text`: `string`
- `Number`: int/long/double/float/decimal/short (+ nullable equivalents)
- `Date`: `DateTime` / `DateTimeOffset`
- `Time`: `TimeSpan`
- `Boolean`: `bool`
- `Other`: fallback

When the type is determined, `AvailableOperators` must match the observed sets:
- Text: `Equals`/`NotEquals`/`Contains`/`NotContains`/`StartsWith`/`EndsWith`
- Number/Date/Time: `Equals`/`NotEquals`/`>`, `>=`, `<`, `<=`, `Between`

Note on `Time`:
- `Time` does not currently use hierarchy in the existing projects:
  - WPF has a TODO for Time tree (15-minute intervals).
  - Blazor treats Time as a flat list (no tree).

## 4.6 AddToExistingFilter + AccumulationMode (Union/Intersection)

Inferred semantics:
- The merge mode decides how the "current selection" (or custom evaluation) is combined with the already active set.

Union (OR):
- `SelectedValues = SelectedValues ∪ newMatches`

Intersection (AND):
- `SelectedValues = SelectedValues ∩ newMatches`

Expected updates in the existing implementations:
- When merge is effective, `SelectAll` becomes false.
- The custom filter is reset:
  - Blazor and WPF reset `SelectedCustomOperator` and `CustomValue1/2`, and `IsCustomFilterExpanded`.

Guard on Intersection + empty custom values:
- WPF: when `AddToExistingFilter` is active, `AccumulationMode == Intersection`, and custom values v1/v2 are empty, immediate selection update is avoided to prevent wiping the previous state while the user is typing.
- Blazor: uses a similar guard via `string.IsNullOrEmpty(CustomValue1/2)` before updating.

## 4.7 Conversion and culture (points to keep identical or harmonize)

The existing projects show a potential divergence:
- For selection UI updates via evaluation (custom operator -> `IsSelected`), the `FilterEvaluator` converts values using `CultureInfo.InvariantCulture`.
- For final filter application (expression execution), `FilterExpressionBuilder` uses `Convert.ChangeType(value, targetType)` without an explicit provider (so conversion may depend on the runtime culture).

For a port, you must decide:
- document and keep the divergence identical, or
- harmonize by forcing conversion using the same culture as the descriptors.

## 5. Async support and pagination (`IAsyncDataProvider`)

## 5.1 Async contract

A specialization that supports async must expose/consume `IAsyncDataProvider<T>`:
- `FetchDataAsync(IFilterContext context, CancellationToken ct)` -> returns `PagedResult<T>` (Items, TotalCount, Page, PageSize)
- `FetchDistinctValuesAsync(string propertyName, string searchText, CancellationToken ct)` -> returns `IEnumerable<object>`

`IFilterContext` includes:
- `Descriptors` (filters)
- `SortDescriptors` (sorting)
- `Page` (current page, 1-based)
- `PageSize`

## 5.2 Expected behavior during user actions

Inferred from `FilterableDataGridViewModel<T>` (WPF):
- After `ApplyColumnFilter`, `ClearColumnFilter`, `ApplySort`, `AddSubSort`, `ClearSort`:
  - the page is reset to `1` (`ctx.Page = 1`)
  - a request is executed via `RefreshDataAsync()`

For async:
- `FilteredItems` comes from `AsyncDataProvider.FetchDataAsync(Context)` when `AsyncDataProvider != null`.

## 5.3 Loading indicator

Inferred:
- WPF: `IsLoading` shows `LoadingText` and hides the list via the inverse of visibility.
- Blazor: `IsLoading` shows `Loading...` and preserves the structure.

## 5.4 Blazor vs WPF difference for async data

WPF:
- full async support (remote filtering + remote distinct values).

Blazor:
- async support for rebuilding `FilterValues` via `SearchCommand` (distinct values provider).
- `DataFilterGrid.razor` applies filtering locally (via `ReflectionFilterEngine`) on `Items`.

For a port to another framework:
- decide if the specialization should offer a "local only" mode, or a "remote filtering" mode (often similar to WPF).

## 6. Persistence (snapshot) of filters and sorting

## 6.1 What must be supported as a feature

The specialization must allow:
- extracting a serializable snapshot of the current filters and sorting state
- restoring the state from a snapshot

The core provides:
- `FilterSnapshotBuilder.CreateSnapshot(IFilterContext)` -> `FilterSnapshot`
- `FilterSnapshotBuilder.RestoreSnapshot(IFilterContext, IFilterSnapshot)`

A `FilterSnapshot` includes:
- `Entries` (filters) -> may contain groups (hierarchy)
- `SortEntries` (sorting) -> ordered list

## 6.2 Existing implementations

WPF:
- `FilterableDataGridViewModel<T>` implements `ExtractSnapshot()` and `RestoreSnapshot(IFilterSnapshot)`.
- `CollectionViewFilterAdapter<T>` implements snapshot extraction/restoration as well.
- Restoration:
  - cleans entries to ignore non-filterable properties via `FilterableProperties`.

Blazor:
- there is no snapshot API exposed in the existing Blazor components.

For a port:
- include snapshot support at least at the WPF-equivalent level, or explicitly mark it as not implemented if targeting only the current local-only Blazor mode.

## 7. Customization (theming, styles, localization)

## 7.1 WPF: theming via ResourceDictionary

An equivalent WPF specialization must support:
- global theme replacement via `Generic.xaml` + `FilterLightTheme.xaml` or `FilterDarkTheme.xaml`
- style resource overrides via `DynamicResource` / `StaticResource`

Resource keys actually used in the code:
- `FilterPopupBackground`
- `FilterPopupForeground`
- `FilterPopupBorder`
- `FilterPopupMaxHeight`
- `FilterButtonStyle`
- `FilterCheckBoxStyle`
- `FilterButtonActiveColor`
- `FilterButtonInactiveColor`

`ColumnFilterButton` also supports customization:
- `IconTemplate` (DataTemplate)
- `ActiveBrush` / `InactiveBrush`

## 7.2 WPF: localization of text

The popup shows texts (buttons, labels) and descriptions via `FilterResources`:
- `OperatorToLocalizedDescriptionConverter` maps a `FilterOperator` to the key `FilterOperator_{OperatorName}`.
- `AccumulationModeToLocalizedDescriptionConverter` maps `AccumulationMode.Union/Intersection` to localized texts.

For portability:
- provide an equivalent localization mechanism or a text substitution mechanism.

## 7.3 Blazor: customization via CSS

An equivalent Blazor specialization must provide stable CSS classes (prefix `df-`) and/or CSS variables.

In `DataFilter.css`, we observe:
- `.df-popup-container` uses `background` and `border` via CSS variables (e.g. `--df-popup-bg`, `--df-popup-border`).
- `.df-resize-handle` for the resizing area.
- `.df-column-header-button.active` for the active state.

Rule:
- a Blazor port must keep a class structure that is easy to override.

Blazor localization:
- button text in `FilterPopup.razor` is hardcoded in English (e.g. "Sort A to Z", "Advanced Filter", "OK", "Clear").
- for multi-language portability, a port can add a localization layer.

## 8. Checklist: features to implement (operational summary)

1. Per-column filter button with active/inactive state and popup opening.
2. Resizable popup with outside-click closing.
3. Multi-column sorting integrated in the popup (set vs add sub-sort).
4. Search bar that triggers distinct value refresh and shows a loading state.
5. Multi-selection list of distinct values with `Select All` tri-state.
6. Collapsible advanced/custom filter section:
   - operators based on column type
   - 1 custom value (custom operator) or 2 values (`Between`)
   - real-time synchronization of the selection
7. Add-to-existing selection:
   - checkbox enable
   - `Union` / `Intersection` mode
   - reset the custom operator when merge becomes effective
8. Handle null values ("Blanks") in the list.
9. Filter semantics produced from `ExcelFilterState`:
   - selection -> `In`
   - `Between` -> `RangeValue`
   - custom operator -> `FilterOperator.*`
   - document the selection+custom combination semantics (WPF vs Blazor)
10. Apply sorting after filtering.
11. Async support:
   - at minimum: async distinct values
   - ideally also: remote filtering via `IAsyncDataProvider<T>` + pagination (`Page`/`PageSize`)
12. Snapshot support (filters + sorting) at the specialization level (at least WPF-equivalent).
13. Customization:
   - WPF: ResourceDictionary themes + override of styles/resource keys
   - Blazor: overridable CSS prefix `df-`
14. Date hierarchy support (year/month/day) and flat list fallback for Time (or explicitly documented limitation).

## 9. Compatibility notes to keep (Blazor vs WPF)

- Selection list vs custom operator combination semantics:
  - WPF: `CustomOperator` takes precedence over manual selection
  - Blazor: both can be combined in an AND
- Clear behavior:
  - WPF: clear closes the popup
  - Blazor: clear does not close the popup
- Debounce:
  - WPF: `AsyncFilterBehavior` exists (debounce), but is not active in the current XAML
  - Blazor: no debounce in the current UI
- Sorting and nested property paths:
  - filtering: supports paths with `.` (by builder)
  - sorting: supports only simple properties (no `.` navigation via `GetProperty`)
- Culture:
  - selection UI: `InvariantCulture` (`FilterEvaluator`)
  - final evaluation: `Convert.ChangeType` (runtime culture)

<!--
# Features.md - Contrat de features pour les projets de specialisation DataFilter

Ce document sert de base pour porter DataFilter (au sens: coeur de filtrage + UI de filtrage "style Excel-like") vers d'autres frameworks/UI.

Il est deduit uniquement des projets:
- `src/DataFilter.Blazor`
- `src/DataFilter.Wpf`

Quand Blazor et WPF ont un comportement different, la difference est documentee (afin que les futurs ports puissent reproduire le comportement attendu, ou decider de l'uniformiser).

## 1. Definitions (termes utilises)

1. **Colonne**: un champ affiche dans la grille/table (header) et filtrable independamment.
2. **Etat de filtre de colonne**: `ExcelFilterState` (depuis `DataFilter.Filtering.ExcelLike.Models`).
3. **Liste de valeurs distinctes**: `ExcelFilterState.DistinctValues` et sa representation UI via `FilterValueItem` (arborecence possible pour les dates).
4. **Filtre de type "custom/advanced"**: l'operateur selectionne dans la section "Advanced Filter" (ex: `Contains`, `Between`).
5. **Add to existing filter**: mode qui fusionne des criteres avec un filtre deja actif sur la meme colonne, avec une semantique d'accumulation `Union` ou `Intersection`.
6. **Tri (sorting)**: selection de direction (A-Z / Z-A) et gestion de multi-colonnes (sub-sorts).
7. **Snapshot**: representation serialisable de l'ensemble (filtres + tri) via `IFilterSnapshot`.

## 2. Contrat fonctionnel (UI + interaction) attendu par toute specialisation

## 2.1 Bouton de filtre dans le header (par colonne)

Une specialisation doit offrir un element interactif place dans le header de colonne:
- Affichage d'un indicateur d'etat "filtre actif" vs "filtre inactif".
- Ouverture/fermeture d'un panneau (popup) dedie au filtrage de la colonne.
- Fermeture si l'utilisateur clique en dehors du popup (comportement "outside click").

Exemples dans les projets existants:
- WPF: `ColumnFilterButton` injecte dans chaque header via `FilterableColumnHeaderBehavior`.
- Blazor: `ColumnFilterButton` apparait dans les headers du `DataFilterGrid.razor`.

## 2.2 Popup de filtrage (panneau Excel-like par colonne)

La specialisation doit fournir un popup/panneau avec les zones suivantes:

### 2.2.1 Section Tri (sorting)

Boutons requis (4 commandes):
- `Sort A to Z` (sort direction ascending).
- `Sort Z to A` (sort direction descending).
- `Add Sort A to Z` (ajoute un criteres de tri comme sub-sort).
- `Add Sort Z to A` (ajoute un criteres de tri comme sub-sort).

Semantique:
- Le tri doit etre applique apres l'application des filtres (dans les implementations existantes).
- La specialisation doit supporter un ordre multi-colonnes (primary sort + sub-sorts).
- L'ajout d'un sub-sort doit remplacer un tri deja existant pour la meme propriete (dans les projets existants, les criteres du meme champ sont remplaces avant d'ajouter).

Exemples:
- Blazor: `FilterPopup.razor` appelle les commandes sur `IBlazorColumnFilterViewModel`.
- WPF: `FilterPopup.xaml` appelle `SortAscendingCommand`, `SortDescendingCommand`, `AddSubSortAscendingCommand`, `AddSubSortDescendingCommand`.

### 2.2.2 Barre de recherche (search-as-you-type / async distinct values)

Une specialisation doit permettre:
- Saisie texte dans une zone de recherche.
- Cette recherche doit declencher le rafraichissement de la liste des valeurs distinctes affichees dans le popup.
- Affichage d'un indicateur de chargement pendant le rafraichissement.

Details d'implementation deduits:
- WPF fournit `AsyncFilterBehavior` (debounce) mais n'est pas branche dans `FilterPopup.xaml` (le comportement est present, mais utilise/commentaire).
- Blazor declenche la logique sur `@oninput` (pas de debounce dans le composant lui-meme).

### 2.2.3 Mode Add to existing filter + AccumulationMode

Une specialisation doit exposer un mode de fusion:
- Une case a cocher `Add selection to filter` / `AddToExistingFilter`.
- Quand ce mode est active, un select doit permettre de choisir la semantique `AccumulationMode`:
  - `Union` (accumulation logique OR)
  - `Intersection` (accumulation logique AND)

Comportement attendu:
- En mode fusion, la nouvelle selection (ou evaluation) doit etre fusionnee avec l'ensemble existant selon `Union`/`Intersection`.
- En mode fusion, la section "custom/advanced filter" est reinitialisee/reset dans les implementations existantes (voir section 4).

### 2.2.4 Advanced/custom filter (operateurs contextuels)

Une specialisation doit fournir une section "Advanced Filter" repliable/expandable:
- Liste d'operateurs disponibles `AvailableOperators` dependante du type de colonne (voir section 4.5).
- Champs d'input pour `CustomValue1` et (optionnellement) `CustomValue2` pour le cas `Between`.
- Le changement d'operateur (et des valeurs) doit mettre a jour la selection UI (les checkboxes) en temps reel.

Operators disponibles (deduits du code des view models):
- Type texte:
  - `Equals`
  - `NotEquals`
  - `Contains`
  - `NotContains`
  - `StartsWith`
  - `EndsWith`
- Type nombre / date / time:
  - `Equals`
  - `NotEquals`
  - `GreaterThan`
  - `GreaterThanOrEqual`
  - `LessThan`
  - `LessThanOrEqual`
  - `Between` (avec `CustomValue1`/`CustomValue2`)

### 2.2.5 Liste de valeurs distinctes multi-selection (checkboxes)

Une specialisation doit afficher une liste/controle hierarchique:
- Checkboxes multi-selection pour `FilterValueItem`.
- Support d'un checkbox `Select All`:
  - Etat bool tri-state (checked / unchecked / indeterminate) via `bool?` (Blazor et WPF).
- Dans le cas des dates: un affichage hierarchique (arborecence).

Deductions UI:
- Blazor: `FilterValueItemView.razor` gere `IsExpanded` et une expander explicite (+/-) pour les noeuds ayant des enfants.
- WPF: `FilterPopup.xaml` utilise un `TreeView` avec des `Children`, et l'expansion utilise l'etat interne des `TreeViewItem` (le binding `IsExpanded` du modele n'est pas utilise dans XAML).

### 2.2.6 Gestion "Blanks" (valeurs nulles)

La liste doit inclure une entree representant les valeurs nulles:
- Blazor affiche `(Blanks)`.
- WPF affiche `FilterResources.Blanks` (depuis les resources localisees).

Semantique de selection:
- La valeur de l'entree blanks est une valeur `null` stockee dans `FilterValueItem.Value`.
- La selection doit etre correctement incluse/exclue dans l'eventuel ensemble `SelectedValues`.

### 2.2.7 Actions OK / Clear

Une specialisation doit fournir:
- Bouton `OK`:
  - declenche l'application du filtre sur la source de donnees
  - ferme le popup (dans Blazor et WPF)
- Bouton `Clear`:
  - reinitialise l'etat de filtre de la colonne

Difference observee (important pour ports):
- WPF: `Clear` ferme le popup via l'event `ColumnFilterViewModel.OnClear`.
- Blazor: `Clear` reinitialise l'etat mais ne ferme pas le popup dans `FilterPopup.razor` (la fermeture n'est declenchee que sur `Apply`).

### 2.2.8 Redimensionnement (resize) du popup

Une specialisation doit permettre de redimensionner le popup:
- Blazor: via `DataFilterInterops.initResizable` (draghandle + clamp viewport).
- WPF: via un `Thumb` dans `FilterPopup.xaml` (drag delta pour width/height).

## 3. Contrat modele de donnees (interne UI)

## 3.1 `ExcelFilterState` (etat de filtre de colonne)

La specialisation doit manipuler un etat equivalant a `ExcelFilterState`:
- `SearchText: string`
- `UseWildcards: bool` (defaut false; non UI dans les projets Blazor/WPF actuels)
- `DistinctValues: List<object>` (valeurs affichees dans la liste)
- `SelectedValues: HashSet<object>` (valeurs selectionnees explicitement)
- `SelectAll: bool` (selectionne toutes les valeurs visibles quand true)
- `CustomOperator: FilterOperator?`
- `CustomValue1: object?`
- `CustomValue2: object?`
- methode `Clear()` qui reset `SearchText`, vide `DistinctValues` et `SelectedValues`, remet `SelectAll=true` et `CustomOperator/CustomValue*` a null.

## 3.2 `FilterValueItem` (noeud checkbox dans l'UI)

La specialisation doit fournir une representation de noeuds:
- `Value: object?`
- `DisplayText: string`
- `Children: ObservableCollection<FilterValueItem>` (0 enfant = feuille)
- `IsSelected: bool?` (tri-state pour support indeterminate)
- `IsNull: bool?` (derive de IsSelected == null dans le modele)
- `IsExpanded: bool` (utilise explicitement par Blazor; WPF n'en depend pas)
- Methode `GetSelectedValues(...)`:
  - ajoute les feuilles dont `IsSelected == true`
  - ignore les feuilles avec `IsSelected == null` (indeterminate)

La mise a jour de `IsSelected` doit se propager:
- Si un noeud enfant change, les noeuds parents doivent recalculer leur etat (allSelected/allUnselected => IsSelected true/false, sinon null).

## 4. Contrat mapping "UI state -> donnees filtrees"

## 4.1 Distinct values + search

La specialisation doit etre capable de:
- Extraire les valeurs distinctes d'une colonne pour alimenter l'UI.
- Appliquer un filtrage de ces valeurs distinctes en fonction de `SearchText`.

Semantique deduite (WPF local / adapter):
- Si `searchText` contient des wildcards `*` ou `?`:
  - matcher wildcard case-insensitive (`WildcardMatcher`).
- Sinon:
  - matcher "starts-with" case-insensitive.

Semantique deduite (WPF async):
- `IAsyncDataProvider.FetchDistinctValuesAsync(propertyName, searchText, ...)` est responsable du filtrage "search -> distinct values".

Semantique deduite (Blazor):
- Le rafraichissement de la liste distincte est "asynchrone" via `SearchCommand` (sur `IBlazorColumnFilterViewModel`).
- Le composant `DataFilterGrid.razor` (implementation actuelle) ignore `SearchText` dans le fournisseur distinct values (comportement demo). Pour un port, la specialisation doit fournir un fournisseur qui respecte `SearchText` (sinon la feature search-as-you-type ne fonctionne pas reellement).

## 4.2 Construction du filtre a partir de `ExcelFilterState`

Deux approches differentes sont visibles dans les projets existants:

### 4.2.1 WPF: `ExcelFilterDescriptor` (precedence custom vs selection)

En WPF, l'application du filtre de colonne est faite via `ExcelFilterDescriptor`:
- Si `State.CustomOperator != null`, alors le descripteur:
  - ajoute un critere sur `CustomOperator` avec `CustomValue1` (et `RangeValue` quand `Between`)
  - n'ajoute pas automatiquement un critere `In` sur `SelectedValues` (la logique "manual selection" est en else-if).
- Si `State.CustomOperator == null`, alors:
  - la selection liste est utilisee via `FilterOperator.In` et `State.SelectedValues` (si pas "SelectAll" ou si la selection ne correspond pas a la totalite).

Semantique consequence:
- Quand l'utilisateur active "Advanced Filter", la selection checkboxes peut etre synchronisee visuellement, mais la logique de filtrage finale privilegie `CustomOperator`.

### 4.2.2 Blazor: assemblage direct de criteres `In` + custom (combinaison AND)

Dans Blazor, `DataFilterGrid.razor` assemble des descriptors manuellement:
- si `SelectAll == false` et `SelectedValues.Count > 0`:
  - ajoute un critere `FilterOperator.In` avec les `SelectedValues`
- si `CustomOperator` est defini:
  - ajoute un critere custom (y compris `Between` avec `RangeValue`)
- les criteres sont combines dans un `FilterGroup` avec `LogicalOperator.And`.

Semantique consequence:
- Blazor combine potentiellement `In(SelectedValues)` et le critere custom dans un AND.

### 4.2.3 Recommendation pour ports

Pour une portabilite robuste, un port doit documenter/decider explicitement la semantique de combinaison:
- Compatibilite WPF (precedence custom sur selection) OU
- Compatibilite Blazor (AND selection list + custom quand les deux sont actives).

Le coeur moteur supporte les groupes et l'AND/OR (via `FilterExpressionBuilder`), donc la difference doit etre codifiee.

## 4.3 Operateurs supportes par le coeur (moteur)

Les specialisations doivent produire des filtres valides pour ces operateurs `FilterOperator`:
- `Contains`
- `NotContains`
- `StartsWith`
- `EndsWith`
- `Equals`
- `NotEquals`
- `GreaterThan`
- `GreaterThanOrEqual`
- `LessThan`
- `LessThanOrEqual`
- `Between` (valeur = `RangeValue`)
- `In` / `NotIn` (valeur = IEnumerable, code deduit `In` en assemblage)
- `IsNull` / `IsNotNull`

Semantique deduite du moteur/FilterExpressionBuilder:
- Comparaisons string: case-insensitive `StringComparison.OrdinalIgnoreCase`.
- `Between`: inclusif min/max, attend `RangeValue`.
- `In`/`NotIn`: OR entre les egalites par valeur.
- `IsNull`/`IsNotNull`: support pour types value nullable vs reference types.
- `propertyName` de filtre peut supporter des chemins imbriques via '.' (car le builder split par '.').

## 4.4 Tri (sorting) et ordre d'application

Semantique deduite:
- Les filtres sont appliques en premier.
- Le tri est ensuite applique sur le resultat.

Tri multi-colonnes:
- Primary sort: remplace les criteres existants (WPF: `SetSort`; Blazor: `SetSort` clear).
- Sub-sorts: ajout et remplacement pour le meme champ (WPF: `ctx.AddSort` supprime duplicates; Blazor: `RemoveAll` par propertyName puis append).

Support de propertyName imbrique pour tri:
- Dans Blazor et WPF, le tri utilise `GetProperty(sort.PropertyName)` sans navigation par '.'.
- Donc, en pratique, les specialisations existantes supportent seulement les noms de proprietes simples pour le tri.

## 4.5 Determination du type de colonne et operateurs affiches

Une specialisation doit determiner le type "contextuel" d'une colonne (equivalent a `FilterDataType`):
- `Text`: string
- `Number`: int/long/double/float/decimal/short (+ nullable equivalents)
- `Date`: DateTime / DateTimeOffset
- `Time`: TimeSpan
- `Boolean`: bool
- `Other`: fallback

Quand le type est determine, la liste `AvailableOperators` doit correspondre aux ensembles observes:
- Text: Equals/NotEquals/Contains/NotContains/StartsWith/EndsWith
- Number/Date/Time: Equals/NotEquals/>, >=, <, <=, Between

Note: `Time` n'utilise pas d'arborecence dans les projets actuels:
- WPF contient un TODO pour Time tree (15-min intervals).
- Blazor traite Time comme flat list (pas d'arbre).

## 4.6 AddToExistingFilter + AccumulationMode (Union/Intersection)

Semantique deduite:
- Le mode fusion decide comment combiner la "selection actuelle" (ou l'evaluation de custom) avec l'ensemble deja actif.

Union (OR):
- `SelectedValues = SelectedValues ∪ newMatches`

Intersection (AND):
- `SelectedValues = SelectedValues ∩ newMatches`

Mises a jour attendues dans les implementations existantes:
- Quand fusion est effective, `SelectAll` est mis a false.
- Le filtre custom est reinitialise:
  - Blazor et WPF reset `SelectedCustomOperator` et `CustomValue1/2` et `IsCustomFilterExpanded`.

Garde (guard) sur Intersection + custom vide:
- WPF: quand `AddToExistingFilter` est actif, `AccumulationMode == Intersection` et que les valeurs custom v1/v2 sont vides, la mise a jour immediate de la selection est evitee pour ne pas effacer l'etat precedent pendant la saisie.
- Blazor: utilise une logique similaire via une guard sur `string.IsNullOrEmpty(CustomValue1/2)` avant de mettre a jour.

## 4.7 Conversion et culture (points a garder identiques ou a harmoniser)

Les projets existants montrent une divergence potentielle:
- Pour la mise a jour de selection UI via evaluation (custom operator -> IsSelected), le `FilterEvaluator` convertit les valeurs avec `CultureInfo.InvariantCulture`.
- Pour l'application finale des filtres (execution des expressions), `FilterExpressionBuilder` utilise `Convert.ChangeType(value, targetType)` sans provider explicite (donc culture dependant du runtime).

Pour un port, il faut:
- Documenter cette divergence si elle reste identique.
- Ou harmoniser en forca la conversion avec la meme culture des lescripteurs.

## 5. Support async et pagination (IAsyncDataProvider)

## 5.1 Interface contractuelle async

Une specialisation supportant l'async doit exposer/consommer `IAsyncDataProvider<T>`:
- `FetchDataAsync(IFilterContext context, CancellationToken ct)` -> renvoie `PagedResult<T>` (Items, TotalCount, Page, PageSize)
- `FetchDistinctValuesAsync(string propertyName, string searchText, CancellationToken ct)` -> renvoie `IEnumerable<object>`

`IFilterContext` inclut:
- `Descriptors` (filtres)
- `SortDescriptors` (tri)
- `Page` (page courante, 1-based)
- `PageSize`

## 5.2 Comportement attendu lors des actions utilisateur

Deduit de WPF `FilterableDataGridViewModel<T>`:
- Apres `ApplyColumnFilter`, `ClearColumnFilter`, `ApplySort`, `AddSubSort`, `ClearSort`:
  - la page est reset a `1` (`ctx.Page = 1`)
  - une requete est executee via `RefreshDataAsync()`

Pour async:
- `FilteredItems` provient de `AsyncDataProvider.FetchDataAsync(Context)` (quand `AsyncDataProvider != null`).

## 5.3 Loading indicator

Deduit:
- WPF: `IsLoading` -> affiche `LoadingText` et cache la liste via l'inverse de la visibility.
- Blazor: `IsLoading` -> affiche `Loading...` et conserve la structure.

## 5.4 Différence Blazor vs WPF sur l'async data

WPF:
- support complet d'async data (filtrage distant + distinct values distant).

Blazor:
- support async pour la re-fabrication de `FilterValues` via `SearchCommand` (distinct values provider).
- `DataFilterGrid.razor` applique les filtres localement (via `ReflectionFilterEngine`) sur `Items`.

Pour un port vers un autre framework:
- decidez si la specialisation doit proposer un mode "local only" ou un mode "remote filtering" (souvent via une version similaire a WPF).

## 6. Persistence (snapshot) des filtres et du tri

## 6.1 Ce qui est attendu comme feature

La specialisation doit pouvoir:
- Extraire un snapshot serialisable de l'etat courant des filtres et du tri.
- Restorer l'etat a partir d'un snapshot.

Le coeur fournit:
- `FilterSnapshotBuilder.CreateSnapshot(IFilterContext)` -> `FilterSnapshot`
- `FilterSnapshotBuilder.RestoreSnapshot(IFilterContext, IFilterSnapshot)`

Un `FilterSnapshot` inclut:
- `Entries` (filtres) -> peut contenir des groupes (hierarchie)
- `SortEntries` (tri) -> liste ordonnee

## 6.2 Implementations existantes

WPF:
- `FilterableDataGridViewModel<T>` implement `ExtractSnapshot()` et `RestoreSnapshot(IFilterSnapshot)`.
- `CollectionViewFilterAdapter<T>` implement aussi `ExtractSnapshot()` et `RestoreSnapshot(IFilterSnapshot)`.
- Restoration:
  - nettoie les entries pour ignorer les proprietes non-filtables via `FilterableProperties`.

Blazor:
- pas d'API de snapshot exposee dans les composants existants.

Pour un port:
- inclure la feature snapshot au moins au niveau WPF-equivalent (ou clairement la marquer comme non implantee si on cible uniquement le mode local Blazor actuel).

## 7. Customisation (theming, styles, localisation)

## 7.1 WPF: theming via ResourceDictionary

Une specialisation WPF-equivalente doit permettre:
- Remplacement global du theme via `Generic.xaml` + `FilterLightTheme.xaml` ou `FilterDarkTheme.xaml`.
- Surcharge de ressources de styles par `DynamicResource`/`StaticResource`.

Clefs/resources effectivement utilisees dans le code:
- `FilterPopupBackground`
- `FilterPopupForeground`
- `FilterPopupBorder`
- `FilterPopupMaxHeight`
- `FilterButtonStyle`
- `FilterCheckBoxStyle`
- `FilterButtonActiveColor`
- `FilterButtonInactiveColor`

Le controle `ColumnFilterButton` supporte aussi une customisation:
- `IconTemplate` (DataTemplate)
- `ActiveBrush` / `InactiveBrush`

## 7.2 WPF: localisation des textes

La popup affiche les textes (boutons, libelles) et descriptions via `FilterResources`:
- Le convertisseur `OperatorToLocalizedDescriptionConverter` mappe un `FilterOperator` vers la cle `FilterOperator_{OperatorName}`.
- Le convertisseur `AccumulationModeToLocalizedDescriptionConverter` mappe `AccumulationMode.Union/Intersection` vers les textes localises.

Pour une portabilite:
- prevoir un mecanisme de localisation equivalent, ou un mecanisme de substituion des textes.

## 7.3 Blazor: customisation via CSS

Une specialisation Blazor-equivalente doit fournir des classes CSS stables (prefix `df-`) et/ou des variables CSS.

Dans `DataFilter.css`, on observe:
- `.df-popup-container` avec `background` et `border` via variables CSS (ex: `--df-popup-bg`, `--df-popup-border`).
- `.df-resize-handle` pour la zone de redimensionnement.
- `.df-column-header-button.active` pour l'etat actif.

Regle: un port Blazor doit conserver une structure de classes facilement surchargable.

Localisation Blazor:
- Les textes de boutons dans `FilterPopup.razor` sont hardcodes en anglais (ex: "Sort A to Z", "Advanced Filter", "OK", "Clear").
- Pour une portabilite multi-langues, un port peut ajouter une couche de localisation.

## 8. Checklist "features a implenter" (resume opereable)

1. Bouton de filtre par colonne avec etat actif/inactif et ouverture popup.
2. Popup resizable avec fermeture outside click.
3. Tri multi-colonnes integre dans la popup (set vs add sub-sort).
4. Barre de recherche declenchant le rafraichissement des distinct values et affichant un loading state.
5. Liste multi-selection de valeurs distinctes avec `Select All` tri-state.
6. Section advanced/custom filter repliable:
   - operateurs selon type de colonne
   - 1 valeur (custom) ou 2 valeurs (Between)
   - synchronisation en temps reel de la selection
7. Add-to-existing selection:
   - checkbox enable
   - mode `Union`/`Intersection`
   - reset du custom operator en mode fusion effective
8. Gestion des valeurs nulles ("Blanks") dans la liste.
9. Semantique des filtres produite a partir de `ExcelFilterState`:
   - selection -> `In`
   - Between -> `RangeValue`
   - custom operator -> `FilterOperator.*`
   - documenter la combinaison selection+custom (WPF vs Blazor)
10. Application de tri apres filtrage.
11. Support async:
   - au minimum async distinct values
   - idealement aussi remote filtering via `IAsyncDataProvider<T>` + pagination (Page/PageSize).
12. Support snapshot (filtres + tri) au niveau specialization (au moins WPF-equivalent).
13. Customisation:
   - WPF: ResourceDictionary themes + override des styles/clefs
   - Blazor: CSS prefix `df-` surchargable
14. Support de la hierarchie de dates (annee/mois/jour) et fallback flat list pour Time (ou limitation explicite).

## 9. Notes de compatibilite a garder (Blazor vs WPF)

- Semantique de combinaison selection list vs custom operator:
  - WPF: `CustomOperator` prend precedence sur la selection manuelle
  - Blazor: les deux peuvent etre combines dans un AND
- Clear:
  - WPF: clear ferme le popup
  - Blazor: clear ne ferme pas le popup
- Debounce:
  - WPF: `AsyncFilterBehavior` existe (debounce), mais n'est pas active dans le XAML actuel
  - Blazor: aucune debounce dans la UI actuelle
- Tri et chemins imbriques:
  - filtrage: support des chemins avec '.' (par builder)
  - tri: support seulement des proprietes simples (GetProperty sans navigation)
- Culture:
  - selection UI: InvariantCulture (FilterEvaluator)
  - evaluation finale: Convert.ChangeType (culture runtime)
-->
