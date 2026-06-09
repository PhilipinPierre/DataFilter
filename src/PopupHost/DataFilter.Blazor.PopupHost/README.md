# DataFilter.Blazor.PopupHost

Hosts and positions the Blazor filter popup (anchored fixed-position overlay, LTR/RTL anchoring, viewport clamping, outside click).

**Visual customization:** [CUSTOMIZATION.md — Blazor](../../../CUSTOMIZATION.md#blazor-datafilterblazor)

## NuGet integration

### Install the package

```bash
dotnet add package DataFilter.Blazor
```

Referenced **transitively** by `DataFilter.Blazor`. Add explicitly only if you build a custom Blazor integration without `DataFilter.Blazor`.

### Target framework

`net8.0`

### Dependencies

- `DataFilter.PlatformShared`

### Quick start

When using **`DataFilter.Blazor`**, include interop assets in your host:

```html
<script src="_content/DataFilter.Blazor/DataFilterInterops.js"></script>
```

Custom hosts can call the PopupHost JS interop directly to anchor a **`FilterPopup`** component to a header button (see `DataFilter.Blazor.PopupHost` sources and demo `/demo/attach`).

## What this package contains

Popup **positioning and lifecycle** only. Popup **UI** (`FilterPopup`, CSS) lives in **`DataFilter.Blazor`**.

## Theming

Positioning is theme-agnostic. Style popups via Blazor **`ThemeClass`** / **`Theme`** on **`FilterPopup`** or CSS `--df-*` variables — [CUSTOMIZATION.md — Blazor](../../../CUSTOMIZATION.md#blazor-datafilterblazor).
