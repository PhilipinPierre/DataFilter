# DataFilter.Localization

Shared localization resources and runtime culture switching helpers used by DataFilter UI integrations.

**Theming vs. localization:** [CUSTOMIZATION.md — Localization vs. theming](../../CUSTOMIZATION.md#localization-vs-theming)

## NuGet integration

### Install the package

```bash
dotnet add package DataFilter.Localization
```

### Target frameworks

`netstandard2.0`, `net8.0`, `net9.0`

### Dependencies

None. Referenced transitively by all UI packages (`DataFilter.Wpf`, `DataFilter.Blazor`, `DataFilter.WinForms`, etc.).

### Quick start

```csharp
using System.Globalization;
using DataFilter.Localization;

// Switch popup language at runtime (all UI stacks)
LocalizationManager.Instance.SetCulture(new CultureInfo("fr"));

// Read a string
var okLabel = LocalizationManager.Instance["Ok"];

// Revert to process UI culture
LocalizationManager.Instance.SetCulture(null);
```

Per-grid culture override is available on `IFilterableDataGridViewModel.CultureOverride` (see **DataFilter.PlatformShared**).

**Theming** (colors, fonts, control chrome) is separate from localization — override via **`FilterTheme`** and platform resource keys; see [CUSTOMIZATION.md — Localization vs. theming](../../CUSTOMIZATION.md#localization-vs-theming).

## What this project contains

- **Shared RESX resources** for popup UI texts (buttons, section headers, operator names, etc.).
- A single runtime entry point: **`DataFilter.Localization.LocalizationManager`**.

Used by the UI integrations:

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

- `Ok`, `Cancel`, `Clear`
- `SortAscending`, `SortDescending`, `AddSubSortAscending`, `AddSubSortDescending`
- `AdvancedFilter`
- `OperatorText`, `ValueText`, `ToText`
- `SelectAll`, `Blanks`
- `LoadingText`
- `ModeUnion`, `ModeIntersection`
- `SearchPlaceholder`
