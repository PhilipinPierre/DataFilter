# DataFilter.WinForms

Windows Forms specialization of DataFilter: filter bar chrome, popup controls, and header behaviors. Grid hosting lives in **`DataFilter.WinForms.PopupHost`**.

## NuGet integration

### Install the packages

```bash
dotnet add package DataFilter.WinForms
dotnet add package DataFilter.WinForms.PopupHost
```

Use **`DataFilter.WinForms.PopupHost`** for `FilterableDataGrid` or **`DataGridViewFilterAdapter.Attach`** on an existing grid.

### Target frameworks

`net8.0-windows`, `net9.0-windows`

### Dependencies (transitive)

`DataFilter.Core`, `DataFilter.Filtering.ExcelLike`, `DataFilter.PlatformShared`, `DataFilter.Localization`

### Quick start

```csharp
using DataFilter.PlatformShared.ViewModels;
using DataFilter.WinForms.PopupHost;

var vm = new FilterableDataGridViewModel<Employee>
{
    LocalDataSource = employees
};
await vm.RefreshDataAsync();

var grid = new FilterableDataGrid
{
    Dock = DockStyle.Fill,
    AutoGenerateColumns = true,
    ViewModel = vm
};
grid.DataSource = vm.FilteredItems.Cast<Employee>().ToList();

vm.PropertyChanged += (_, e) =>
{
    if (e.PropertyName == nameof(vm.FilteredItems))
        grid.DataSource = vm.FilteredItems.Cast<Employee>().ToList();
};

Controls.Add(grid);
```

Optional filter bar — wrap with **`FilterGridChromeControl`** and set **`ShowFilterBar = true`** (see **DataFilter.PlatformShared** README).

Pipeline presets:

```csharp
await vm.ApplyFilterPipelineSnapshotAsync(vm.CreateFilterPipelineSnapshot());
```

## Controls

- **`FilterGridChromeControl`** — hosts optional **`FilterBarControl`** above grid content.
- **`FilterBarControl`** — active-filters chip bar (enable via chrome host).
- **`FilterPopupControl`** — column filter popup surface.
- **`FilterHeaderBehavior`** — attach filtering to column headers on a custom grid.

## Localization

Popup UI texts come from **`DataFilter.Localization`**.

```csharp
using System.Globalization;
using DataFilter.Localization;

LocalizationManager.Instance.SetCulture(new CultureInfo("fr"));
```

Per-grid culture: **`IFilterableDataGridViewModel.CultureOverride`**.
