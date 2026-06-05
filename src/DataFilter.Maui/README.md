# DataFilter.Maui

Native MAUI specialization of DataFilter: filter bar chrome, popup services, and list/grid integration aligned with **DataFilter.PlatformShared**.

## NuGet integration

### Install the packages

```bash
dotnet add package DataFilter.Maui
dotnet add package DataFilter.Maui.PopupHost
```

### Target frameworks

`net9.0-android`, `net9.0-ios`, `net9.0-maccatalyst`, `net9.0-windows10.0.19041.0` (Windows)

### Dependencies (transitive)

`DataFilter.Core`, `DataFilter.Filtering.ExcelLike`, `DataFilter.PlatformShared`, `DataFilter.Localization`

### Quick start

```csharp
using DataFilter.Maui.Controls;
using DataFilter.PlatformShared.ViewModels;

var vm = new FilterableDataGridViewModel<Employee>
{
    LocalDataSource = employees
};
await vm.RefreshDataAsync();

var grid = new FilterableDataGrid
{
    ViewModel = vm,
    ItemsSource = vm.FilteredItems
};
```

Optional active-filters bar — use **`FilterGridChromeView`** with **`ShowFilterBar="True"`** (see **DataFilter.PlatformShared** README).

Pipeline presets:

```csharp
await vm.ApplyFilterPipelineSnapshotAsync(vm.CreateFilterPipelineSnapshot());
```

## Localization

Popup UI texts come from **`DataFilter.Localization`**:

```csharp
using System.Globalization;
using DataFilter.Localization;

LocalizationManager.Instance.SetCulture(new CultureInfo("fr"));
```

Per-grid culture: **`IFilterableDataGridViewModel.CultureOverride`**.
