# DataFilter WPF

A visual data filtering system inspired by Excel filtering, with multiple UI integrations (WPF, WinForms, Blazor, WinUI 3, MAUI) and support for asynchronous data loading from external APIs.

This repo’s guiding principle is **behavior alignment across UI frameworks**:
- **Filtering semantics** (operators, “search + select all” persistence, stacked criteria)
- **Popup behavior** (open/close, anchored positioning, scroll/resize tracking)
- **Interaction contracts** (stable selectors / IDs where applicable)

To enforce alignment, the repository includes a growing **UI contract suite** that runs the *same* high-level scenarios across different target frameworks.

The solution is divided into several main projects:

1.  **`DataFilter.Core`** (.NET 8 / 9 / .NET Standard 2.0 & 2.1)
    - Contains pure filtering logic and abstractions (`IFilterEngine`, `IFilterDescriptor`).
    - UI-independent.
2.  **`DataFilter.Filtering.ExcelLike`** (.NET 8 / 9)
    - Implements advanced filtering engine with distinct value selection formatted like Excel.
    - Handles complex composite filters (manual selection + contextual operators).
3.  **`DataFilter.Wpf`** (.NET 8 / 9 Windows)
    - Provides WPF controls (`FilterableDataGrid`, `FilterableGridView`, `FilterPopup`) and attachable behaviors.
4.  **`DataFilter.Blazor`** (.NET 8 / 9)
    - Provides Blazor components (`ColumnFilterButton`, `FilterPopup`) for WebAssembly, Server-side, and Hybrid.
    - Modern and fully customizable UI via CSS classes.
5.  **`DataFilter.Expressions.Server`** (.NET 8 / 9)
    - Extension for converting filter snapshots into LINQ Expressions for server-side evaluation.

## Key Features

### 🚀 Advanced Filtering
- **Excel-like Selection**: Multi-select checkboxes with hierarchical support (e.g., Dates grouped by Year/Month/Day).
- **Advanced Synchronization**: Changing a custom operator (like "Contains") automatically updates the selection list in real-time.
- **Contextual Operators**: 
  - **Text**: Contains, Not Contains, Starts with, Ends with, Equals, Not Equals.
  - **Numbers/Dates/Time**: Greater than, Less than, Between, Equals, Not Equals.
- **Search persistence without `In(list)`**: When the user filters distinct values using `SearchText` and keeps **Select All** enabled, the filter is persisted as a **search rule** (e.g., `StartsWith` + pattern) instead of serializing the resulting list of selected distinct values. This allows reapplying saved filters safely when data has changed.
- **Search unions without `In(list)`**: Repeated searches in **Union (OR)** mode are persisted as **OR-combined rules** (e.g., `StartsWith("Alice") OR StartsWith("Henry")`). If the user selects only a subset of a searched group, that group is persisted as `In(subset)` inside the OR, without materializing other groups.
- **Wildcards in search patterns**: Text operators support `*` (any sequence) and `?` (single character) directly in the Core evaluation pipeline, so saved search rules replay consistently.
- **Additive & Refinement Modes**: 
  - **Union (Additive)**: Merges new matches with the current selection (Logical OR).
  - **Intersection (Refinement)**: Keeps only items that match BOTH the current selection and the new criteria (Logical AND).
- **Cumulative Filtering**: "Add to current selection" mode allows merging successive search results.
- **Data source changes**: When **`LocalDataSource`** (or a collection view’s source) is **replaced**, call **`RefreshDataAsync`** on the grid ViewModel so **`SelectedValues`** are reconciled with the new distincts and column filters stay in sync (`FilterDescriptorsChanged` drives popup reload where applicable).
- **Stacked filters on one column**: Multiple custom rules on the same column (**`AdditionalCustomCriteria`**) are **AND**-combined in **`ExcelFilterDescriptor`**; the filter popup reapplies **all** of them when refreshing distinct values.

### 📶 Multi-Column Sorting
- **Sub-sorting**: Define secondary and tertiary order (e.g., Order by Name, then by Date).

### 🌐 Asynchronous Data Loading
- **Server-side Filtering**: Implement `IAsyncDataProvider<T>` to offload filtering and sorting to an API or database.
- **On-demand Distinct Values**: Fetch unique values for the filter popup only when needed.

### 🔗 Filter pipeline (ordered criteria, groups, presets)
- **Structured graph**: Model filters as an ordered tree of **criterion** and **named group** nodes with stable IDs, per-node **enable/disable**, and **AND/OR** at the root and inside groups (`FilterPipeline`, `CriterionPipelineNode`, `GroupPipelineNode` in **DataFilter.Core**).
- **Compilation**: `FilterPipelineCompiler` produces `IFilterDescriptor` instances for the existing engine; root-level **OR** is represented as a single logical group so behavior stays consistent with `FilterExpressionBuilder`.
- **Persistence**: Serialize **`FilterPipelineSnapshot`** (schema-versioned DTO) to JSON or your store; map with `FilterPipelineSnapshotMapper`. Import from legacy UI state via **`FilterPipelineInterop.FromLegacySnapshot(IFilterSnapshot)`**.
- **Context**: `IFilterContext.ReplaceDescriptors` applies the compiled list in order and allows **multiple criteria on the same property** when needed.
- **ViewModels**: `FilterableDataGridViewModel` exposes **`ApplyFilterPipelineAsync`** and **`CreatePipelineFromCurrentSnapshot`** (see **DataFilter.PlatformShared**). The WPF demo **Local filter** scenario includes a JSON panel (sync from grid / apply preset).

## Quick Start (WPF)

```xml
<controls:FilterableDataGrid ItemsSource="{Binding FilteredItems}" 
                             FilterContext="{Binding GridViewModel.Context}" />
```

```csharp
public class MyViewModel : ObservableObject
{
    public FilterableDataGridViewModel<MyItem> GridViewModel { get; }

    public MyViewModel()
    {
        GridViewModel = new FilterableDataGridViewModel<MyItem>
        {
            LocalDataSource = _myFullCollection
        };
        // Initialization
        _ = GridViewModel.RefreshDataAsync();
    }
}
```

### 2. Manual Integration (e.g. into GridView)

```xml
<GridViewColumn Header="Name" 
                DisplayMemberBinding="{Binding Name}"
                behaviors:FilterableColumnHeaderBehavior.IsFilterable="True" />
```

## Detailed Usage & Examples

For more in-depth examples and configuration options, please refer to the project-specific documentation:

- [**DataFilter.Wpf**](src/DataFilter.Wpf/README.md): Detailed UI setup, theming, and behaviors for WPF.
- [**DataFilter.Blazor**](src/DataFilter.Blazor/README.md): Component usage, styling, and host configuration for Blazor.
- [**DataFilter.Core**](src/DataFilter.Core/README.md): Extending the filtering engine and understanding abstractions.
- [**DataFilter.Expressions.Server**](src/DataFilter.Expressions.Server/README.md): Integrating server-side filtering with LINQ.

## Visual Customization

### WPF
The WPF controls are designed using `Generic.xaml` with no hardcoded styles.
Two base themes are provided: `FilterLightTheme.xaml` and `FilterDarkTheme.xaml`.

### Blazor
The Blazor components use modern Vanilla CSS with explicit classes (prefix `df-`).
Customization is done by overriding these classes in your app's stylesheet.

See [CUSTOMIZATION.md](CUSTOMIZATION.md) for full details on both platforms.

## Localization (popup internationalization)

Popup UI texts (Sort / Advanced filter / operators / etc.) are shared across UI stacks via **`DataFilter.Localization`**.

### Runtime language switching

To switch the UI language at runtime:

```csharp
using System.Globalization;
using DataFilter.Localization;

LocalizationManager.Instance.SetCulture(new CultureInfo("fr"));
```

### Forcing a culture per grid

UI integrations can force the popup culture via `IFilterableDataGridViewModel.CultureOverride`.

```csharp
using System.Globalization;
using DataFilter.PlatformShared.ViewModels;

var vm = new FilterableDataGridViewModel<MyItem>(new CultureInfo("fr"))
{
    LocalDataSource = items
};
await vm.RefreshDataAsync();
```

## Unit Testing
The solution includes a comprehensive test suite (75+ tests) covering:
- Core expression building logic.
- Excel-like descriptor combinations.
- Server-side queryable integration.
- WPF & Blazor ViewModel commands and state management.

To run all tests:
```bash
dotnet test
```

## Visual / UI contract testing (cross-framework alignment)

The goal of the UI contract suite is not pixel-perfect screenshots; it’s to validate that **end-user behaviors** remain consistent across the supported UI stacks.

### Blazor demos (Playwright E2E)

Project:
- `demo/DataFilter.Blazor.Demo.PlaywrightTests`

What it validates (against `/demo/attach`):
- **PopupOpenClose**
- **AnchoredPositioning** (position computed by the same interop used at runtime)
- **ScrollKeepsPopupAnchored**
- **FilteringAffectsRows**

Run against Blazor **Server** (default):

```bash
dotnet test "demo/DataFilter.Blazor.Demo.PlaywrightTests/DataFilter.Blazor.Demo.PlaywrightTests.csproj" -c Release
```

Run against Blazor **WASM hosted**:

```powershell
$env:DF_DEMO_HOST = "wasm"
dotnet test "demo/DataFilter.Blazor.Demo.PlaywrightTests/DataFilter.Blazor.Demo.PlaywrightTests.csproj" -c Release
```

Notes:
- CI installs Playwright **Chromium** and runs these tests for **Server** and **WASM hosted**.
- The Blazor popup host exposes stable selectors via deterministic IDs / `data-testid` (e.g. `df-filter-btn-Department`, `df-filter-popup-Department`).

### Desktop demos (WPF / WinForms / WinUI 3) via UI Automation (FlaUI)

Project:
- `tests/UIContracts.FlaUI.Tests`

What it validates today:
- **WPF**: Attach scenario → popup can be opened (contract: popup opens, then app closes)
- **WinForms**: Attach scenario → popup can be opened (header button is owner-drawn, clicked by coordinates)
- **WinUI 3**: same scenario is present, but requires **Windows App Runtime**. If the runtime is missing, the test is a no-op to avoid breaking environments that can’t run WinUI 3 unpackaged.

Run:

```bash
dotnet test "tests/UIContracts.FlaUI.Tests/UIContracts.FlaUI.Tests.csproj" -c Release
```

Important:
- **The UI tests always close the application process** (best-effort `Close()` then `Kill()` in `finally`) so no windows are left open after the run.

### MAUI (Appium) – environment-driven contract suite

Project:
- `tests/UIContracts.Appium.Tests`

This suite is intentionally **environment-driven** (Appium server + device/emulator required). Until those are configured, tests are no-op.

Run:

```bash
dotnet test "tests/UIContracts.Appium.Tests/UIContracts.Appium.Tests.csproj" -c Release
```

Environment variables (to enable execution):
- `UICT_APP_PLATFORM`: `android` or `ios`
- `UICT_APPIUM_SERVER`: e.g. `http://127.0.0.1:4723/`
- `UICT_APP_PATH`: path to the built app package (`.apk`, `.app`, `.ipa`)

As we add stable `AutomationId` / accessibility identifiers to the MAUI demo, this becomes a deterministic contract test (e.g. `df-filter-btn-Department` / `df-filter-popup-Department`).
