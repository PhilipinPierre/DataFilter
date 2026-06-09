# DataFilter.WinForms.PopupHost

Hosts and positions the WinForms filter popup, and provides `FilterableDataGrid` plus attach adapters.

**Visual customization:** [CUSTOMIZATION.md — WinForms](../../../CUSTOMIZATION.md#winforms-datafilterwinforms)

## NuGet integration

### Install the packages

```bash
dotnet add package DataFilter.WinForms
dotnet add package DataFilter.WinForms.PopupHost
```

### Target frameworks

`net8.0-windows`, `net9.0-windows`

### Dependencies

- `DataFilter.WinForms` (transitive: Core, ExcelLike, PlatformShared, Localization)

### Quick start — FilterableDataGrid

```csharp
using DataFilter.PlatformShared.ViewModels;
using DataFilter.WinForms.PopupHost;

var vm = new FilterableDataGridViewModel<Employee> { LocalDataSource = items };
await vm.RefreshDataAsync();

var grid = new FilterableDataGrid
{
    Dock = DockStyle.Fill,
    ViewModel = vm,
    AutoGenerateColumns = true
};
grid.DataSource = vm.FilteredItems.Cast<Employee>().ToList();
```

### Quick start — attach to existing DataGridView

```csharp
var adapter = DataGridViewFilterAdapter.Attach(existingGrid, viewModel);
```

## What this package contains

- **`FilterableDataGrid`**, **`DataGridViewFilterAdapter`**, **`FilterableDataGridViewComponent`** (designer extender).
- Popup hosting: open/close lifecycle, LTR/RTL anchoring, screen clamping.

Filter bar and popup **content** remain in **`DataFilter.WinForms`**.

## Theming

Popups call **`FilterPopupControl.ApplyTheme()`** using **`FilterTheme.Current`**. Customize via **`FilterTheme`** / **`FilterTheme.With(...)`** — see [CUSTOMIZATION.md — WinForms](../../../CUSTOMIZATION.md#winforms-datafilterwinforms).
