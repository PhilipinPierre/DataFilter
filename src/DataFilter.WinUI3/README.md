# DataFilter.WinUI3

WinUI 3 specialization of DataFilter: filter bar chrome, header behaviors, and grid controls aligned with **DataFilter.PlatformShared**.

## NuGet integration

### Install the packages

```bash
dotnet add package DataFilter.WinUI3
dotnet add package DataFilter.WinUI3.PopupHost
```

Requires **Windows App SDK** and **`net8.0-windows10.0.19041.0`** (or later Windows TFM used by the package).

### Dependencies (transitive)

`DataFilter.Core`, `DataFilter.Filtering.ExcelLike`, `DataFilter.PlatformShared`, `DataFilter.Localization`

### Quick start

```xml
xmlns:controls="using:DataFilter.WinUI3.Controls"
```

```xml
<controls:FilterableDataGrid x:Name="Grid"
                             ViewModel="{x:Bind ViewModel.GridViewModel, Mode=OneWay}"
                             AreColumnFiltersEnabled="True"
                             ColumnFilterTriggerMode="FilterButton" />
```

Grid-level **`AreColumnFiltersEnabled`** and **`ColumnFilterTriggerMode`** (see **`DataFilter.PlatformShared.ColumnFilter`**) control popup triggers and header border chrome. ListView attach: **`ListViewFilterHeaderAdapter`**.

```csharp
ViewModel.GridViewModel = new FilterableDataGridViewModel<Employee>
{
    LocalDataSource = employees
};
await ViewModel.GridViewModel.RefreshDataAsync();
Grid.ItemsSource = ViewModel.GridViewModel.FilteredItems;
```

Optional filter bar — **`FilterGridChrome`** with **`ShowFilterBar="True"`**. Pipeline presets via **`ApplyFilterPipelineSnapshotAsync`**.

## Localization

```csharp
using System.Globalization;
using DataFilter.Localization;

LocalizationManager.Instance.SetCulture(new CultureInfo("fr"));
```

Per-grid culture: **`IFilterableDataGridViewModel.CultureOverride`**.
