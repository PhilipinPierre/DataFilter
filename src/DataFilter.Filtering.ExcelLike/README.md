# DataFilter.Filtering.ExcelLike

High-performance engine for advanced, Excel-like filtering logic.

## Logic Overview
- **Hierarchical Distinct Values**: Grouping dates by Year/Month/Day.
- **Custom Filters**: Combining primitive selection with operator-based rules.
- **Accumulation Modes**:
  - `Union`: Add items from search to results.
  - `Intersection`: Refine current results with search.

## Key Services

### `ExcelFilterEngine`
The central hub for data analysis and filter creation.

### `ExcelFilterDescriptor` / `ExcelFilterState`
- **`Descriptors`** builds filter rules from the column state. A **custom** filter (`CustomOperator`) adds one rule; **`AdditionalCustomCriteria`** adds further **AND**-combined rules on the **same** property (same semantics as the popup’s stacked custom filters).
- **`SelectedValues`** holds the distinct values used for list/In selection; the UI reconciles these when the backing data is replaced.
- **`OrSearchPatterns`** persists *Union (OR)* of full search results without materializing distinct lists (e.g., `StartsWith(\"Alice\") OR StartsWith(\"Henry\")`).
- **`OrSelectedValues`** persists *Union (OR)* when the user selects only a subset of a searched group (e.g., `StartsWith(\"Alice\") OR In([\"Henry 124\",\"Henry 146\"])`).

### `ExcelFilterSelectionReconciler`
Keeps **`ExcelFilterState.SelectedValues`** aligned with **current** distinct value instances (e.g. after **`ItemSource` / `LocalDataSource`** replacement). Uses reference equality where possible, then value equality. The **`dropSelectionsNotInDistinct`** flag distinguishes **strict** reconciliation (grid refresh) from **popup** **InitializeAsync** (preserve off-list selections during search narrowing).

### `WildcardMatcher`
Performance-optimized utility for text pattern matching (supporting `*` and `?`).
