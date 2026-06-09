# DataFilter.Wpf

Visual data filtering system for WPF with Excel-like interaction, filter pipeline presets, optional active-filters bar, and asynchronous data support.

**Visual customization:** [CUSTOMIZATION.md — WPF](../../CUSTOMIZATION.md#wpf-datafilterwpf)

## NuGet integration

### Install the packages

```bash
dotnet add package DataFilter.Wpf
dotnet add package DataFilter.Wpf.PopupHost
```

`DataFilter.Wpf.PopupHost` provides `FilterableDataGrid`, `FilterableGridView`, and `FilterableColumnHeaderBehavior`. It is required for built-in grid headers and attach scenarios.

### Target frameworks

`net8.0-windows`, `net9.0-windows`

### Dependencies (transitive)

`DataFilter.Core`, `DataFilter.Filtering.ExcelLike`, `DataFilter.PlatformShared`, `DataFilter.Localization`, `CommunityToolkit.Mvvm`, `Microsoft.Xaml.Behaviors.Wpf`

### Quick start

1. Apply a theme in `App.xaml`:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="pack://application:,,,/DataFilter.Wpf;component/Themes/FilterLightTheme.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

2. Create a ViewModel and host the grid inside **`FilterGridChrome`** (recommended — includes optional filter bar):

```xml
xmlns:wpf="clr-namespace:DataFilter.Wpf.Controls;assembly=DataFilter.Wpf"
xmlns:controls="clr-namespace:DataFilter.Wpf.Controls;assembly=DataFilter.Wpf.PopupHost"
```

```csharp
// ViewModel
GridViewModel = new FilterableDataGridViewModel<Employee>();
GridViewModel.LocalDataSource = employees;
await GridViewModel.RefreshDataAsync();

// View (code-behind) — place FilterableDataGrid inside chrome
var grid = new FilterableDataGrid { AutoGenerateColumns = true };
grid.SetBinding(FilterableDataGrid.ItemsSourceProperty, new Binding(nameof(vm.FilteredItems)) { Source = vm });
grid.SetBinding(FilterableDataGrid.ViewModelProperty, new Binding { Source = vm });
chrome.SetGridContent(grid);
```

```xml
<wpf:FilterGridChrome GridViewModel="{Binding GridViewModel}"
                      ShowFilterBar="True" />
```

3. Pipeline presets (optional):

```csharp
var snapshot = GridViewModel.CreateFilterPipelineSnapshot();
await GridViewModel.ApplyFilterPipelineSnapshotAsync(snapshot);
```

See demo `DataFilter.Wpf.Demo` → **Local filter** for JSON sync/apply and snapshot editing.

## Controls

### `FilterGridChrome`

Hosts an optional **`FilterBar`** above grid content. Set **`ShowFilterBar="True"`** for the active-filters chip bar. Wire **`GridViewModel`** to `IFilterableDataGridViewModel` and call **`SetGridContent`** with your `FilterableDataGrid` or custom `DataGrid`.

### `FilterableDataGrid` (PopupHost)

Specialized `DataGrid` with built-in filtering headers. Bind **`ItemsSource`** to **`FilteredItems`**, **`ViewModel`** to the grid ViewModel, and optionally **`FilterContext`**.

### `FilterableGridView` (PopupHost)

`GridView` variant for `ListView` controls.

### `FilterBar`

Standalone active-filters bar; prefer **`FilterGridChrome`** for popup wiring from bar chip edits.

### `ColumnFilterButton`

Header button that toggles the filter popup. Used by behaviors and custom headers.

## Behaviors

### `FilterableColumnHeaderBehavior` (PopupHost)

Make any `GridViewColumn` or `DataGridColumn` filterable:

```xml
xmlns:behaviors="clr-namespace:DataFilter.Wpf.Behaviors;assembly=DataFilter.Wpf.PopupHost"
xmlns:df="clr-namespace:DataFilter.PlatformShared.ColumnFilter;assembly=DataFilter.PlatformShared"

<controls:FilterableDataGrid AreColumnFiltersEnabled="{Binding ShowColumnFilters}"
                             ColumnFilterTriggerMode="HeaderRightClick"
                             ViewModel="{Binding}" />

<GridViewColumn Header="Name" DisplayMemberBinding="{Binding Name}"
                behaviors:FilterableColumnHeaderBehavior.IsFilterable="True"
                behaviors:FilterableColumnHeaderBehavior.ColumnFilterTriggerMode="FilterButton" />
```

Grid-level settings:

- **`AreColumnFiltersEnabled`** — bindable; when `false`, no filter UI on any column.
- **`ColumnFilterTriggerMode`** — see table below.

Per-column overrides: **`IsFilterable`** (disable one column), **`ColumnFilterTriggerMode`** (`Inherit` or a specific mode).

| Mode | Opens popup via | Filter indicator (when active) | Native column sort |
|------|-----------------|------------------------------|-------------------|
| `FilterButton` | Dedicated ▼ button | Button chrome | Unchanged |
| `HeaderRightClick` | Right-click header | Inner inset border | Unchanged |
| `HeaderLeftClick` | Left-click header | Inner inset border | **Disabled** on that column |
| `HeaderDoubleClick` | Double-click header | Inner inset border | Unchanged (single-click can still sort) |
| `HeaderMiddleClick` | Middle mouse button | Inner inset border | Unchanged |
| `None` | No header trigger (filter bar / API only) | Inner inset border | Unchanged |
| `ContextMenuFilter` | Right-click → “Filter…” menu | Inner inset border | Unchanged |
| `HeaderLongPress` | Long press (~500 ms) | Inner inset border | Unchanged |
| `KeyboardShortcut` | `Alt+↓` when header focused | Inner inset border | Unchanged |
| `HoverRevealButton` | ▼ button on hover only | Inner inset border | Unchanged |
| `ShiftClick` | Shift + left-click | Inner inset border | Unchanged |
| `CtrlClick` | Ctrl + left-click | Inner inset border | Unchanged |

When **`ColumnFilterTriggerMode` ≠ `FilterButton`** and the column is filtered, an **inner inset border** is drawn on the header (default header chrome is unchanged otherwise). With **`FilterButton`**, state is shown on the button instead.

For existing `DataGrid` / `GridView` hosts, use **`FilterableGridAttach.AreColumnFiltersEnabled`** and **`FilterableGridAttach.ColumnFilterTriggerMode`**.

Subscribes to **`IFilterableDataGridViewModel.FilterDescriptorsChanged`** and item-source changes. Opening the popup runs **`SearchCommand`** then **`LoadStateAsync`** from **`GetColumnFilterState`**.

## Theming

Controls use `Generic.xaml` with **no hardcoded colors**. Override via ResourceDictionary keys (see **`FilterThemeResourceKeys`** in PlatformShared).

### Base themes

- `Themes/Generic.xaml` — `ColumnFilterButton` template
- `Themes/FilterLightTheme.xaml` — default light palette
- `Themes/FilterDarkTheme.xaml` — default dark palette

Merge in `App.xaml` (see [CUSTOMIZATION.md — WPF](../../CUSTOMIZATION.md#wpf-datafilterwpf)).

### Key resources

| Resource key | Purpose |
|--------------|---------|
| `FilterPopupBackground` / `FilterPopupForeground` / `FilterPopupBorder` | Popup surface |
| `FilterButtonActiveColor` / `FilterButtonInactiveColor` | Header filter button |
| `FilterPopupMaxHeight` | Popup height cap |
| `FilterButtonStyle`, `FilterCheckBoxStyle`, `FilterSearchBoxStyle`, `FilterExpanderStyle` | Control styles |

Override colors or replace entire styles:

```xml
<Color x:Key="FilterPopupBackgroundColor">#FAFAFA</Color>
<SolidColorBrush x:Key="FilterButtonActiveColor" Color="Orange" />
```

`ColumnFilterButton` also supports **`IconTemplate`**, **`ActiveBrush`**, **`InactiveBrush`**.

Runtime preset switch (demo **Customization**): swap merged `FilterLightTheme.xaml` / `FilterDarkTheme.xaml`. For cross-stack alignment, values match **`FilterTheme.Light`** / **`FilterTheme.Dark`**.
