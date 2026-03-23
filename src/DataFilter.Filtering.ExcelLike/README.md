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

### `WildcardMatcher`
Performance-optimized utility for text pattern matching (supporting `*` and `?`).
