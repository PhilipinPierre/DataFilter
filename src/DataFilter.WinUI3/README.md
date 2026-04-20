# DataFilter.WinUI3

WinUI 3 specialization of DataFilter aligned with `Features.md`.

## Localization

Popup UI texts are provided by `DataFilter.Localization` and update at runtime when the culture changes:

```csharp
using System.Globalization;
using DataFilter.Localization;

LocalizationManager.Instance.SetCulture(new CultureInfo("fr"));
```

You can also force the popup culture per grid via `IFilterableDataGridViewModel.CultureOverride`.
