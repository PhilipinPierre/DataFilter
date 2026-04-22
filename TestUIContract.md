# UI Contract Test Specification (Cross-Framework)

This document defines the **UI behavior contracts** that all “visual/UI automation” test suites must validate across target frameworks:
- Blazor (Server + WASM hosted) via Playwright
- WPF / WinForms / WinUI 3 via UI Automation (FlaUI)
- MAUI via Appium

The goal is **behavior alignment** across frameworks (not pixel-perfect screenshots).

## Core principles

- **Same user intent, same outcome**: a user action should produce equivalent filtering and popup behavior regardless of UI technology.
- **Contracts over implementation**: tests must assert externally observable behavior, not internal view model details.
- **Deterministic selectors**: prefer stable IDs / `data-testid` / `AutomationId` over brittle UI tree traversal.
- **No leftovers**: tests must always close the demo application/process in `finally` (best-effort `Close()` then `Kill()`).

## Definitions

- **Anchor**: the UI element that triggers a column popup (typically a filter button in a header).
- **Popup**: the filtering UI surface anchored to the column header (can be a DOM element, a WPF `Popup`, a WinForms `ContextMenuStrip`, etc.).
- **Attach scenario**: “attach/adapt to an existing control” integration (WPF DataGrid attach, WinForms DataGridView attach, WinUI3 ListView header adapter, Blazor attach/headless demo page).

## Mandatory contracts (all frameworks)

### Contract A — PopupOpenClose

**Intent**: The user can open a column popup, then close it.

**Given**
- The demo is on an “Attach” page/view with at least one filterable column (e.g. `Department`).

**When**
- The user activates the column filter anchor (click/tap).

**Then**
- A popup becomes present and interactable.
- The popup can be closed via at least one supported close mechanism:
  - outside click/tap, or
  - explicit close button, or
  - Escape key (if supported by the stack).

**Notes**
- If the framework uses an owner-drawn header button (e.g. WinForms DataGridView), the test may use coordinate-based clicking on the header “hit target”, but the *contract remains the same*.

### Contract B — AnchoredPositioning (Viewport/WorkArea Safe)

**Intent**: The popup is anchored to the correct column and stays within a visible safe area.

**Then**
- The popup is positioned relative to the anchor in the expected direction (typically below the anchor unless there’s not enough space).
- The popup is clamped within the viewport/work area margins (no off-screen unusable placement).

**Tolerance**
- Use a small pixel tolerance to account for DPI, fonts, and subpixel layout.
- If the platform has a canonical positioning function (e.g. Blazor interop), tests should assert against that canonical source to maximize alignment.

### Contract C — ScrollKeepsPopupAnchored

**Intent**: After scrolling the host container/window, the popup remains anchored correctly.

**When**
- The host content scrolls (mouse wheel, scrollbar, programmatic scroll).

**Then**
- The popup’s anchored position is updated so it remains aligned with the anchor (within tolerance).

### Contract D — FilteringAffectsRows/Items

**Intent**: Applying a filter changes the visible dataset consistently.

**When**
- The user applies a filter that is guaranteed to match a known subset (example: `Department == IT` in the demo dataset).

**Then**
- The list/grid shows only matching items.
- Clearing or disabling the filter restores the unfiltered set.

**Implementation guidance**
- Prefer verifying **a stable subset invariant** (e.g. “all visible rows have Department == IT”) rather than an exact count, unless the demo dataset is fixed.

### Contract E — NoUnhandledErrors

**Intent**: UI interaction does not break the runtime (no silent UI failure).

**Then**
- No framework-level “unhandled error” UI is displayed (e.g. Blazor error UI).
- No fatal runtime error is logged by the host during the scenario.

## Recommended contracts (implement per framework as feasible)

### Contract F — OutsideClickDoesNotClickThrough

**Intent**: Clicking outside closes the popup without triggering unintended actions “behind” it.

**Then**
- Popup closes.
- No unrelated selection/navigation is triggered by the outside click.

### Contract G — ResizeBehavior (if supported)

**Intent**: If the popup is resizable, resizing is stable and does not detach it from the anchor.

**Then**
- Resize handle works.
- Popup remains within viewport/work area constraints.

### Contract H — RTL layout

**Intent**: In RTL mode, anchoring and clamping still behave correctly.

**Then**
- Popup remains usable and anchored (no off-screen placement).

### Contract I — Localization

**Intent**: Popup strings reflect the selected UI culture.

**Then**
- A representative label/button text is localized (e.g. “Clear”, “Advanced filter”, sort buttons).

## Cross-framework testability requirements

To make contracts executable across stacks, demos should expose stable hooks:

- **Blazor**
  - deterministic anchor IDs and `data-testid` (e.g. `df-filter-btn-Department`, `df-filter-popup-Department`)
- **WPF / WinUI 3 / MAUI**
  - `AutomationProperties.AutomationId` set on the anchor and popup root
- **WinForms**
  - if owner-drawn: define a stable header hit target width and ensure click detection is consistent across DPI

If a stack cannot expose stable element identifiers (typical for owner-drawn surfaces), tests may fallback to **coordinate-based** interaction, but should still validate the same end-user contract outcomes.

## Framework-specific notes (non-normative)

- **WinUI 3**: running unpackaged requires Windows App Runtime. If the runtime is missing, tests should be skipped/no-op rather than failing the entire suite.
- **MAUI (Appium)**: tests should be environment-driven (Appium server + device/emulator + app bundle path).

## Cleanup requirements (mandatory)

Every visual/UI test must:
- close the popup it opened (if still open), and
- close the application window, and
- ensure the process is terminated (best-effort `Close()` then `Kill()` in `finally`).

This prevents leaving windows open and reduces flakiness between runs.

