# Visual Customization Guide (all UI stacks)

DataFilter keeps **colors, typography, and control chrome** under your control. Each UI package exposes the same logical palette via **`FilterTheme`** (`DataFilter.PlatformShared.Theming`) and platform-specific resource keys.

## Shared model: `FilterTheme`

```csharp
using DataFilter.PlatformShared.Theming;

// Built-in presets (aligned with WPF themes and Blazor CSS)
FilterTheme.Current = FilterTheme.Dark;

// Fine-grained override
FilterTheme.Current = FilterTheme.Light.With(
    popupBackground: "#FAFAFA",
    primaryColor: "#E65100");

// Blazor inline CSS variables
var style = FilterTheme.Current.ToCssVariableStyle();
// "--df-popup-bg: #FAFAFA; --df-primary-color: #E65100; …"
```

| Property | Role |
|----------|------|
| `PopupBackground` | Popup / chip surface |
| `PopupForeground` | Primary text |
| `PopupBorder` | Popup and chip borders |
| `PrimaryColor` | Accent (OK button, active filter indicator) |
| `SecondaryBackground` | Sort buttons hover, inputs |
| `SecondaryBorder` | Input borders, secondary buttons |
| `HeaderBackground` | Advanced-filter section header |
| `ButtonActiveColor` | Column filter button (active) |
| `ButtonInactiveColor` | Column filter button (idle) |
| `PrimaryButtonForeground` | Text on primary buttons |
| `OverlayBackground` | Modal dim (`#AARRGGBB`) |
| `FilterBarClusterBackground` | AND-cluster background in filter bar |
| `PopupShadow` | CSS `box-shadow` value |
| `ResizeHandleColor` | Blazor resize grip |
| `FontFamily` / `FontSizePt` | Optional typography |
| `PopupMaxHeight` | WPF popup max height |

Canonical key names for each stack are in **`FilterThemeResourceKeys`**.

---

## WPF (`DataFilter.Wpf`)

Styles live in **Resource Dictionaries** — no hardcoded colors in controls.

### Base themes

- `pack://application:,,,/DataFilter.Wpf;component/Themes/Generic.xaml`
- `pack://application:,,,/DataFilter.Wpf;component/Themes/FilterLightTheme.xaml`
- `pack://application:,,,/DataFilter.Wpf;component/Themes/FilterDarkTheme.xaml`

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="pack://application:,,,/DataFilter.Wpf;component/Themes/Generic.xaml" />
            <ResourceDictionary Source="pack://application:,,,/DataFilter.Wpf;component/Themes/FilterDarkTheme.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

### Resource keys (`FilterThemeResourceKeys.Wpf*`)

| Key | Type | Used by |
|-----|------|---------|
| `FilterPopupBackground` / `FilterPopupBackgroundColor` | Brush / Color | `FilterPopup`, `FilterBar` |
| `FilterPopupForeground` / `FilterPopupForegroundColor` | Brush / Color | Popup text |
| `FilterPopupBorder` / `FilterPopupBorderColor` | Brush / Color | Borders |
| `FilterButtonActiveColor` | Brush | `ColumnFilterButton` active icon |
| `FilterButtonInactiveColor` | Brush | `ColumnFilterButton` idle icon |
| `FilterPopupMaxHeight` | `double` | Popup height cap |
| `FilterButtonStyle` | `Style` | Footer / sort buttons |
| `FilterCheckBoxStyle` | `Style` | Value list checkboxes |
| `FilterSearchBoxStyle` | `Style` | Search `TextBox` |
| `FilterExpanderStyle` | `Style` | Advanced filter expander |
| `FilterSearchBlockStyle` | `Style` | Search label block |

Override globally or per-scope:

```xml
<Color x:Key="FilterPopupBackgroundColor">#FAFAFA</Color>
<SolidColorBrush x:Key="FilterButtonActiveColor" Color="Orange" />
```

### Custom filter icon

`ColumnFilterButton` exposes **`IconTemplate`**, **`ActiveBrush`**, **`InactiveBrush`** — override via style (see demo **Customization**).

---

## Blazor (`DataFilter.Blazor`)

Components use stable **`df-`** CSS classes. All colors flow through **CSS custom properties** defined in `_content/DataFilter.Blazor/DataFilter.css`.

### CSS variables (`FilterThemeResourceKeys.Css*`)

| Variable | Maps to `FilterTheme` |
|----------|----------------------|
| `--df-popup-bg` | `PopupBackground` |
| `--df-popup-fg` | `PopupForeground` |
| `--df-popup-border` | `PopupBorder` |
| `--df-primary-color` | `PrimaryColor` |
| `--df-secondary-bg` | `SecondaryBackground` |
| `--df-secondary-border` | `SecondaryBorder` |
| `--df-header-bg` | `HeaderBackground` |
| `--df-button-active-color` | `ButtonActiveColor` |
| `--df-button-inactive-color` | `ButtonInactiveColor` |
| `--df-btn-primary-fg` | `PrimaryButtonForeground` |
| `--df-overlay-bg` | `OverlayBackground` |
| `--df-filter-bar-cluster-bg` | `FilterBarClusterBackground` |
| `--df-popup-shadow` | `PopupShadow` |
| `--df-resize-handle-color` | `ResizeHandleColor` |
| `--df-font-family` / `--df-font-size` | Typography |

### Three ways to theme

1. **Wrapper class** — add `df-theme-dark` on a parent (`FilterThemeResourceKeys.BlazorDarkThemeClass`).
2. **Override in your stylesheet** — set variables on `:root` or a scope class.
3. **Component parameters** — `DataFilterGrid` / `FilterPopup` accept **`ThemeClass`** and **`Theme`** (`FilterTheme` → inline `style`).

```razor
<div class="my-app @(_dark ? "df-theme-dark" : "")">
    <DataFilterGrid ThemeClass="@(_dark ? "df-theme-dark" : "")"
                    Theme="@FilterTheme.Current"
                    … />
</div>
```

```css
.my-brand {
    --df-primary-color: #e65100;
    --df-popup-bg: #fff8f0;
}
```

---

## WinForms (`DataFilter.WinForms`)

`FilterPopupControl.ApplyTheme(FilterTheme?)` applies **`FilterTheme.Current`** (or a passed instance) to popup surfaces and buttons.

```csharp
using DataFilter.PlatformShared.Theming;
using DataFilter.WinForms.Controls;

FilterTheme.Current = FilterTheme.Dark;
popup.ApplyTheme(); // or popup.ApplyTheme(myTheme)

// bool overload still available
popup.ApplyTheme(isDark: true);
```

Use **`FilterThemeApplier.ToDrawingColor`** for custom host painting.

---

## MAUI (`DataFilter.Maui`)

`FilterPopupView.ApplyTheme(FilterTheme?)` listens to **`FilterTheme.CurrentChanged`**. Overlay and filter-bar borders read **`FilterTheme.Current`**.

```csharp
FilterTheme.Current = FilterTheme.Light.With(primaryColor: "#512BD4");
```

Combine with `Application.Current.UserAppTheme` for system light/dark; `FilterTheme` controls filter-specific colors.

---

## WinUI 3 (`DataFilter.WinUI3`)

By default, `FilterPopupControl` uses **system theme brushes** (`ApplicationPageBackgroundThemeBrush`, `TextFillColorSecondaryBrush`, …) and follows **`ElementTheme`**.

For explicit brand colors:

```csharp
using DataFilter.PlatformShared.Theming;
using DataFilter.WinUI3.Theming;

FilterTheme.Current = myTheme;
popup.ApplyTheme();
```

`FilterBarControl` cluster borders use **`FilterTheme.Current.PopupBorder`**.

---

## Filter bar & header chrome

Filter bar chips and column header indicators reuse the same palette:

- **Blazor**: `df-filter-bar-*`, `df-column-header-filter-active` (`::after` uses `--df-primary-color`)
- **WPF**: `FilterBar.xaml` → `FilterPopupBackground`, `FilterPopupBorder`, …
- **WinForms / MAUI / WinUI 3**: bar controls read `FilterTheme.Current` where borders/overlays are drawn

Header **trigger modes** (`ColumnFilterTriggerMode`) are independent of colors — see per-package README tables.

---

## Localization vs. theming

Popup **text** is localized via **`DataFilter.Localization`** (`LocalizationManager`). **Colors and styles** are never hardcoded in localized strings — override themes as above.
