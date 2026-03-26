# Bring all non-WPF demos into compliance with DemoFeatures.md

## Context

The reference implementation is **`DataFilter.Wpf.Demo`**, which already complies with every DemoFeatures.md requirement.
The five other demo projects are essentially empty scaffolds and must be updated to match the same feature contract.

**Shared data layer** (`DataFilter.Demo.Shared`): already compliant — `Employee` model + `EmployeeDataGenerator` match the spec entirely.

---

## Gap analysis per project

| Project | Shell controls | Scenario 1 Local | Scenario 2 Async | Scenario 3 Hybrid | Scenario 4 Theme | Scenario 5 ListView | Scenario 6 CollectionView |
|---|---|---|---|---|---|---|---|
| WPF (ref) | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **WinForms** | ❌ | ⚠️ one-scenario only | ❌ | ❌ | ❌ | ❌ | ❌ |
| **WinUI3** | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| **UWP** | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| **MAUI** | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| **Blazor** | ❌ | ⚠️ partial (one page) | ❌ | ❌ | ❌ | ❌ | ❌ |

---

## Proposed Changes

### Shared — `MockEmployeeApiService` (reused across projects)

Each non-WPF demo project will receive its own copy of `MockEmployeeApiService` + `IMockEmployeeApiService`, identical to the WPF demo's implementation (uses `ExcelFilterEngine<Employee>`, 500ms simulated delay).

---

### DataFilter.WinForms.Demo

Structure mirroring WPF demo (Services, ViewModels, scenario panels).

#### [MODIFY] [MainForm.cs](file:///d:/Workspaces_Personal/Dev/DataFilter/demo/DataFilter.WinForms.Demo/MainForm.cs)
- Add top `Panel` with: `NumericUpDown` (RowCount 1–100000), "Regenerate Data" button, "Clear filters" button.
- Add `TabControl` with 6 `TabPage`s containing scenario-specific `UserControl`s.
- Wire Regenerate and Clear filters to all 6 scenario ViewModels.

#### [NEW] Services/IMockEmployeeApiService.cs
#### [NEW] Services/MockEmployeeApiService.cs
#### [NEW] ViewModels/LocalFilterScenarioViewModel.cs
#### [NEW] ViewModels/AsyncFilterScenarioViewModel.cs
#### [NEW] ViewModels/HybridFilterScenarioViewModel.cs
#### [NEW] ViewModels/CustomizationScenarioViewModel.cs
#### [NEW] ViewModels/ListViewScenarioViewModel.cs
#### [NEW] ViewModels/CollectionViewScenarioViewModel.cs

Each ViewModel holds a `FilterableDataGridViewModel<Employee>` (WinForms), initializes with 1000 rows, and exposes `Regenerate(int count)`.

`CollectionViewScenarioViewModel` wraps a `BindingSource` + `BindingList<Employee>` to demonstrate WinForms data-binding adapter pattern.

#### Tab panels: one `UserControl` per scenario, each hosting a `FilterableDataGrid`.

- **Scenario 1**: Local `FilterableDataGrid`, auto-columns.
- **Scenario 2**: Banner label "Async Data Loading Enabled" + `FilterableDataGrid` with async provider.
- **Scenario 3**: Banner label "Hybrid: local results + async distinct values" + grid.
- **Scenario 4**: `CheckBox` "Dark Theme" + label with customization guidance + grid.
  - WinForms theme switching: toggle between two `Color`-based system color overrides applied to the `FilterableDataGrid`.
- **Scenario 5**: ListView equivalent — `DataGridView` with explicit columns (Id, Name, Department, Country, Time) bound through a `FilterableDataGrid` context.
- **Scenario 6**: `BindingSource`-backed grid demonstrating WinForms data-binding adapter.

---

### DataFilter.WinUI3.Demo

#### [MODIFY] [DataFilter.WinUI3.Demo.csproj](file:///d:/Workspaces_Personal/Dev/DataFilter/demo/DataFilter.WinUI3.Demo/DataFilter.WinUI3.Demo.csproj)
- Add `<ProjectReference>` to `DataFilter.WinUI3` (src) and `DataFilter.Demo.Shared`.

#### [MODIFY] [MainWindow.xaml](file:///d:/Workspaces_Personal/Dev/DataFilter/demo/DataFilter.WinUI3.Demo/MainWindow.xaml)
- Shell: row with `NumberBox` (RowCount), "Regenerate Data" `Button`, "Clear filters" `Button`.
- `NavigationView` hosting 6 page frames, one per scenario.

#### [NEW] MainViewModel.cs — RowCount, RegenerateCommand, ClearFiltersCommand
#### [NEW] Services/IMockEmployeeApiService.cs + Services/MockEmployeeApiService.cs
#### [NEW] ViewModels/LocalFilterScenarioViewModel.cs … CollectionViewScenarioViewModel.cs (×6)
#### [NEW] Pages/LocalFilterPage.xaml + .cs … CollectionViewPage.xaml + .cs (×6)

- **Scenario 4 (theme)**: WinUI3 `ToggleSwitch` bound to `IsDarkTheme`; on change, switch `Application.RequestedTheme` between `Light` and `Dark` at runtime.
- **Scenario 5**: `ListView` with `GridViewColumn`-equivalent (`GridView` columns) hosting `FilterableGridView` if available, else explicit column header templates.
- **Scenario 6**: `AdvancedCollectionView` (WinUI Community Toolkit) or `CollectionViewSource` wrapper bound through the DataFilter adapter.

---

### DataFilter.UwpXaml.Demo

#### [MODIFY] [DataFilter.UwpXaml.Demo.csproj](file:///d:/Workspaces_Personal/Dev/DataFilter/demo/DataFilter.UwpXaml.Demo/DataFilter.UwpXaml.Demo.csproj)
- Add `<ProjectReference>` to `DataFilter.UwpXaml` (src) and `DataFilter.Demo.Shared`.

#### [MODIFY] [MainPage.xaml](file:///d:/Workspaces_Personal/Dev/DataFilter/demo/DataFilter.UwpXaml.Demo/MainPage.xaml)
- Shell: `StackPanel` with `TextBox`/`NumberBox` (RowCount), buttons.
- `Pivot` control with 6 `PivotItem`s for the 6 scenarios.

#### [NEW] MainViewModel.cs
#### [NEW] Services/IMockEmployeeApiService.cs + Services/MockEmployeeApiService.cs  
#### [NEW] ViewModels/\*ScenarioViewModel.cs (×6)

- **Scenario 4**: `ToggleSwitch`; switches `RequestedTheme` on root element between `Light` and `Dark`.

---

### DataFilter.Demo.Maui

#### [MODIFY] [DataFilter.Maui.Demo.csproj](file:///d:/Workspaces_Personal/Dev/DataFilter/demo/DataFilter.Demo.Maui/DataFilter.Maui.Demo.csproj)
- Add `<ProjectReference>` to `DataFilter.Maui` (src) and `DataFilter.Demo.Shared`.

#### [MODIFY] [AppShell.xaml](file:///d:/Workspaces_Personal/Dev/DataFilter/demo/DataFilter.Demo.Maui/AppShell.xaml)
- Replace default content with `Shell` tabs or `FlyoutItem`s for the 6 scenario pages.
- Shell header toolbar or page-level toolbar with RowCount, Regenerate, Clear filters.

#### [MODIFY] [MainPage.xaml](file:///d:/Workspaces_Personal/Dev/DataFilter/demo/DataFilter.Demo.Maui/MainPage.xaml)
- Replace Hello World content — become the Local Filtering scenario page.

#### [NEW] Pages/AsyncFilterPage.xaml + .cs … CollectionViewPage.xaml + .cs (remaining 5)
#### [NEW] ViewModels/MainViewModel.cs
#### [NEW] Services/IMockEmployeeApiService.cs + Services/MockEmployeeApiService.cs
#### [NEW] ViewModels/\*ScenarioViewModel.cs (×6)

- **Scenario 4**: `Switch` bound to `IsDarkTheme`; changes `Application.Current.UserAppTheme` between `AppTheme.Light` and `AppTheme.Dark`.
- **Scenario 5**: MAUI `CollectionView` with a header row of labeled columns + filter buttons — closest equivalent to a list with column headers.
- **Scenario 6**: MAUI `CollectionView` with a filter adapter wrapping the local list, applying the filter context.

---

### DataFilter.Blazor.Demo.Shared (affects Server + Wasm + Hybrid)

#### [MODIFY] [DemoPage.razor](file:///d:/Workspaces_Personal/Dev/DataFilter/demo/DataFilter.Blazor.Demo.Shared/Pages/DemoPage.razor)
- Convert from single-scenario page into a layout page with tab navigation.
- Or keep it as the Local Filtering route (`/demo/local`) and add 5 more pages.

#### [NEW] Pages/AsyncFilterPage.razor, HybridFilterPage.razor, CustomizationPage.razor, ListViewPage.razor, CollectionViewPage.razor
#### [NEW] Layout/DemoNavMenu.razor — sidebar/tab nav linking all 6 pages
#### [NEW] Layout/DemoShell.razor — shared shell: RowCount `<input type="number">`, "Regenerate Data" button, "Clear filters" button

#### [NEW] Services/MockEmployeeApiService.cs (Blazor — no WPF-specific types)
#### [NEW] State/DemoState.cs — shared Blazor state (employees list, row count, per-scenario filter contexts via `CascadingValue`)

- **Scenario 2**: `DataFilterGrid` with async provider + loading spinner / banner "Async Mode".
- **Scenario 3**: `DataFilterGrid` with `LocalDataSource` + `AsyncDataProvider` for distinct values.
- **Scenario 4**: `<button>` renders "Toggle Dark/Light"; adds/removes a CSS class on `<body>` via JS interop; filter component picks up the CSS custom properties.
- **Scenario 5**: `DataFilterGrid` with explicit column definitions for Id, Name, Department, Country, Time only (no Salary, HireDate, IsActive) — represents a "list view" with subset columns.
- **Scenario 6**: Demonstrate filtering through a `IQueryable<Employee>` adapter approach, if supported, or equivalent state-managed collection view.

---

## Verification Plan

### Automated Tests

```
dotnet build d:\Workspaces_Personal\Dev\DataFilter\DataFilter.slnx -c Debug
```

All six demo projects must build with **0 errors** and **0 warnings** (or only pre-existing warnings) to be considered compliant at build level.

> [!NOTE]
> There are no automated unit tests for demo projects — they are verification-by-running apps. The existing `tests/` projects test the library, not the demo UI.

### Manual Verification

For each non-WPF demo project, the user should run it and verify:

1. **Shell**: The app starts and shows a numeric input (Row Count), a "Regenerate Data" button, and a "Clear filters" button.
2. **Scenarios**: There are exactly 6 scenarios (tabs/pages/navigation items): Local Filtering, Async Filtering, Hybrid Filtering, Customization, ListView/List integration, CollectionView/Adapter.
3. **Scenario 1 — Local**: Change a filter → data updates immediately (no loading delay).
4. **Scenario 2 — Async**: Change a filter → brief loading indicator → data updates.
5. **Scenario 3 — Hybrid**: Filtering updates data locally; opening filter popup fetches distinct values asynchronously.
6. **Scenario 4 — Customization**: Toggle Light/Dark → filter popup visuals change (not just background).
7. **Scenario 5 — ListView**: Only columns Id, Name, Department, Country, Time are shown; all are filterable.
8. **Scenario 6 — CollectionView**: Filtering works through the view/binding adapter.
9. **Regenerate Data**: Enter e.g. 500 in Row Count, click Regenerate → all scenarios update to 500 rows.
10. **Clear filters**: Apply a filter in any scenario, click Clear filters → results reset to full dataset.
