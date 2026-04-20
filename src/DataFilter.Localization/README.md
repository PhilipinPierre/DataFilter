# DataFilter.Localization

Shared localization resources and runtime culture switching helpers used by DataFilter UI integrations.

## What this project contains

- **Shared RESX resources** for popup UI texts (buttons, section headers, operator names, etc.).
- A single runtime entry point: **`DataFilter.Localization.LocalizationManager`**.

This is used by the UI integrations:

- `DataFilter.Wpf`
- `DataFilter.WinForms`
- `DataFilter.WinUI3`
- `DataFilter.Maui`
- `DataFilter.Blazor`

## Runtime language switching

Switch the UI culture at runtime:

```csharp
using System.Globalization;
using DataFilter.Localization;

LocalizationManager.Instance.SetCulture(new CultureInfo("fr"));
```

Revert to the current process UI culture:

```csharp
LocalizationManager.Instance.SetCulture(null);
```

## Getting localized strings

Retrieve a string by key:

```csharp
var ok = LocalizationManager.Instance["Ok"];
```

If a key is missing, the indexer returns the key itself (so missing resources are easy to spot).

## Per-grid popup culture override

UI hosts can force a popup culture via `IFilterableDataGridViewModel.CultureOverride`.
`DataFilter.PlatformShared.ViewModels.FilterableDataGridViewModel` provides convenience constructors:

```csharp
using System.Globalization;
using DataFilter.PlatformShared.ViewModels;

var vm = new FilterableDataGridViewModel<MyItem>(new CultureInfo("fr"));
```

UI integrations apply this override when opening the popup and restore the previous culture when the popup closes.

## Common keys

Some frequently used keys:

- `Ok`, `Clear`
- `SortAscending`, `SortDescending`, `AddSubSortAscending`, `AddSubSortDescending`
- `AdvancedFilter`
- `OperatorText`, `ValueText`, `ToText`
- `SelectAll`, `Blanks`
- `LoadingText`
- `ModeUnion`, `ModeIntersection`
- `SearchPlaceholder`

