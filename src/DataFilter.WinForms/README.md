# DataFilter.WinForms

Windows Forms specialization of DataFilter, based on the feature contract in `Features.md`.

## Localization

Popup UI texts are provided by `DataFilter.Localization`.

- Switch language at runtime:

```csharp
using System.Globalization;
using DataFilter.Localization;

LocalizationManager.Instance.SetCulture(new CultureInfo("fr"));
```

- Force popup culture for a specific grid via `IFilterableDataGridViewModel.CultureOverride` (when available on your VM implementation).
