# DemoFeatures.md — DataFilter demo contract

This document defines the **feature contract** that **all** DataFilter demo applications must follow.
It is based on the reference implementation: **`DataFilter.Wpf.Demo`**.

The goal is consistency: when a user opens any demo (WPF / WinUI3 / WinForms / UWP XAML / MAUI / Blazor / …), they should find the same scenarios, the same dataset, and the same expected behaviors—adapted to the platform UI patterns where necessary.

## Definitions

- **Demo app**: Any runnable project under `demo/` that showcases DataFilter UI + filtering behavior.
- **Scenario**: A dedicated page/tab/route showcasing one specific integration mode.
- **Filter context**: The shared state that holds active filter descriptors for a scenario.
- **Filtered items**: The current view of data after filters (and optionally sorting) have been applied.

## Required shared dataset contract

All demos **must** use the shared demo data model and generator from `DataFilter.Demo.Shared` (or an equivalent shared package if a platform cannot reference it directly).

- **Entity**: `Employee`
- **Fields (minimum)**:
  - `Id` (int)
  - `Name` (string)
  - `Department` (string)
  - `Country` (string)
  - `Salary` (float)
  - `HireDate` (DateTime)
  - `Time` (TimeSpan)
  - `IsActive` (bool)
- **Generator**: deterministic pseudo-random generation (stable distribution and categories), with:
  - Departments: IT, HR, Sales, Marketing, Engineering
  - Countries: France, USA, UK, Germany, Japan
- **Default row count**: 1000 rows on first launch.

## Required app shell contract

Every demo app **must** expose the following shell-level controls and behaviors:

- **Row count input**
  - A numeric input that controls the number of `Employee` rows to generate.
  - Must support at least \(1\) to \(100000\) rows (platform-dependent upper bounds are allowed, but must be documented in the demo UI).
- **Regenerate data action**
  - A button/command labeled similarly to “Regenerate Data”.
  - Regenerates the dataset using the configured row count.
  - Refreshes the active scenario immediately.
  - If the demo hosts multiple scenarios at once (tabs), it **should** update all scenarios, like the WPF reference.
- **Clear filters action**
  - A button/command labeled similarly to “Clear filters”.
  - Clears all active filter descriptors for the **current scenario** (or for all scenarios if no “current” concept exists).
  - Triggers a refresh so the full dataset is visible again.

## Required scenario set

All demo apps **must** implement the following scenarios. The UI form factor may differ:

- WPF/WinForms/WinUI/UWP: Tabs, navigation view, or separate pages
- Blazor: Routes/pages
- MAUI: Shell tabs or pages

### Scenario 1 — Local Filtering (in-memory)

**Purpose**: demonstrate filtering performed locally against an in-memory collection.

**Requirements**

- Uses a local data source (the generated `Employee` list).
- Filter UI is available from column headers (or the platform-equivalent interaction).
- Filtering updates the displayed items without a “server” roundtrip.

**Expected user-observable behavior**

- Selecting values / using search in the filter popup updates the results immediately.
- Clearing filters restores the full dataset.

### Scenario 2 — Async Filtering (remote/server simulation)

**Purpose**: demonstrate async fetching where filtering/sorting is handled “server-side”.

**Requirements**

- Uses an async data provider that simulates a remote API (network delay is acceptable and recommended).
- Filtering triggers an async refresh/fetch to obtain results.
- The UI shows that async mode is enabled (banner text, badge, progress indicator, etc.).

**Expected user-observable behavior**

- Applying a filter causes a short loading period, then updated results.
- Clearing filters causes a refresh and returns to the unfiltered result set.

### Scenario 3 — Hybrid Filtering (local results + async distinct values)

**Purpose**: demonstrate local filtering of results while distinct-value retrieval can be async.

**Requirements**

- Local filtering is enabled for the result set.
- An async provider is still configured so the filter UI can fetch distinct values (platform-equivalent behavior).
- The scenario must clearly communicate it is “hybrid”.

**Expected user-observable behavior**

- Filtering the dataset updates results locally.
- Opening a filter UI may trigger async work to populate distinct values (if supported by the platform integration).

### Scenario 4 — Customization (themes/styling)

**Purpose**: demonstrate how to customize the look-and-feel of the filtering UI.

**Requirements**

- Provides at least a **Light** and **Dark** styling option.
- A toggle (or equivalent control) switches between Light and Dark at runtime.
- The scenario explains where to customize resources/styles (platform-appropriate guidance).

**Expected user-observable behavior**

- Switching theme changes the filter popup/header visuals (not just the app background).
- Filtering behavior remains unchanged across themes.

### Scenario 5 — “ListView / list control” integration

**Purpose**: show DataFilter integrated with a list-style control (not only a data grid).

**Requirements**

- Uses a list control with a column-like header concept if available (e.g., WPF `ListView/GridView`, WinUI ListView with header templates, etc.).
- At minimum, the following columns must be shown and filterable:
  - Id, Name, Department, Country, Time

**Platform notes**

- If the platform does not support column headers on list controls, implement the closest equivalent (e.g., a header row with filter buttons bound to the same filter context).

### Scenario 6 — CollectionView / view-adapter integration

**Purpose**: demonstrate integration with a platform “view” abstraction (collection view / binding view / adapter).

**Requirements**

- Uses the platform’s view abstraction (e.g., WPF `ICollectionView`, MAUI `CollectionView`, WinUI collection view source, etc.) or an adapter that wraps it.
- Filtering must operate through that view abstraction (or a provided adapter) and update UI.

## Cross-cutting behavior requirements

These behaviors apply to **every** scenario:

- **Filter context consistency**
  - Each scenario owns its own filter context (unless the platform demo intentionally shares it and documents why).
  - “Clear filters” must clear the active context and refresh results.
- **Column coverage**
  - Demos must expose a representative mix of types (string, numeric, date/time, bool).
  - When using auto-generated columns (platform-supported), the demo must still ensure `Employee` contains the required fields above.
- **Virtualization/performance**
  - For large row counts, the scenario should enable UI virtualization where the platform supports it.
- **No hidden prerequisites**
  - The demo should run with no external services required (async scenario must be simulated locally).

## Acceptance checklist (per demo project)

Each demo project is considered compliant if:

- [ ] Uses the shared `Employee` dataset (or an explicitly documented equivalent).
- [ ] Provides Row Count input + Regenerate Data + Clear filters at the app shell level.
- [ ] Implements all 6 required scenarios (or documents a platform limitation and provides the closest equivalent UX).
- [ ] Local scenario filters in-memory items.
- [ ] Async scenario performs async refresh and communicates loading state.
- [ ] Hybrid scenario combines local filtering with async distinct-value fetching (or equivalent).
- [ ] Customization scenario provides runtime Light/Dark theme switching affecting filter UI.
- [ ] List integration scenario provides filterable list columns (Id/Name/Department/Country/Time).
- [ ] CollectionView scenario demonstrates filtering through a view abstraction / adapter.

