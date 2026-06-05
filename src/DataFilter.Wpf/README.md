# DataFilter.Wpf

Visual data filtering system for WPF with Excel-like interaction, filter pipeline presets, optional active-filters bar, and asynchronous data support.

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
xmlns:behaviors="clr-namespace:DataFilter.Wpf;assembly=DataFilter.Wpf.PopupHost"

<GridViewColumn Header="Name" DisplayMemberBinding="{Binding Name}"
                behaviors:FilterableColumnHeaderBehavior.IsFilterable="True" />
```

Subscribes to **`IFilterableDataGridViewModel.FilterDescriptorsChanged`** and item-source changes. Opening the popup runs **`SearchCommand`** then **`LoadStateAsync`** from **`GetColumnFilterState`**.

## Theming

Controls use `Generic.xaml` with no hardcoded colors. Base themes: **`FilterLightTheme.xaml`**, **`FilterDarkTheme.xaml`**. See [CUSTOMIZATION.md](../../CUSTOMIZATION.md).
