# DataFilter.Wpf

Visual data filtering system for WPF with Excel-like interaction and asynchronous data support.

## Controls

### `FilterableDataGrid`
A specialized `DataGrid` with built-in filtering headers. Supports sorting and easy binding to a `FilterContext`.

### `FilterableGridView`
A `GridView` variant for use in `ListView` controls, enabling Excel-like filtering for list-based data.

### `ColumnFilterButton`
The header button that toggles the filter popup. Can be attached to any column via behaviors.

## Usage

1. Reference `DataFilter.Wpf`.
2. Apply a theme in `App.xaml`:
   ```xml
   <ResourceDictionary Source="pack://application:,,,/DataFilter.Wpf;component/Themes/FilterLightTheme.xaml" />
   ```
3. Use the control:
   ```xml
   <controls:FilterableDataGrid ItemsSource="{Binding Items}" 
                                 FilterContext="{Binding GridViewModel.Context}" />
   ```

## Behaviors

### `FilterableColumnHeaderBehavior`
Allows making any `GridViewColumn` or `DataGridColumn` filterable by simply setting an attached property:
```xml
<GridViewColumn Header="Name" DisplayMemberBinding="{Binding Name}"
                behaviors:FilterableColumnHeaderBehavior.IsFilterable="True" />
```
Subscribes to **`IFilterableDataGridViewModel.FilterDescriptorsChanged`** and **`LocalDataSource`** / **`FilteredItems`** property changes so the header button and filter popup stay aligned with the grid context after filters or **item source** updates. Opening the popup runs **`SearchCommand`** (empty search) then **`LoadStateAsync`** from **`GetColumnFilterState`**, so distinct lists and selection state match the current data.
