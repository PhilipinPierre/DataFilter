## Objectif
Comparer la **consistance** des comportements (popup/positionnement/scroll/filtrage) entre les hôtes UI supportés par ce repo.

L’approche recommandée est une “contract suite” commune (mêmes scénarios, mêmes tolérances), implémentée par stack.

## Contract suite (scénarios communs)
- **PopupOpenClose**: ouvrir via un bouton de colonne, fermer via click outside / escape.
- **AnchoredPositioning**: popup ancrée sous le bouton, clamp dans le viewport, pas de position “fantôme”.
- **ScrollKeepsPopupAnchored**: après scroll/resize, le delta (popup ↔ anchor) reste stable à \(\pm\) quelques pixels.
- **FilteringAffectsRows**: appliquer un filtre (ex: `Department == IT`) change effectivement le dataset visible.

### Métriques / tolérances
- **Tolérance position**: 4–8 px (selon DPI / arrondis CSS).
- **Clamp viewport**: popup `Left/Top` et `Right/Bottom` restent dans l’écran (hors \(\pm\) 1 px).
- **Stabilité scroll**: variation du gap \(\le\) 6 px.

## Shared library
- **`tests/UIContracts.Common`**: `ColumnMatrix`, `FilterPipelinePresets` (JSON embarqué), `DemoViewCatalog`, `RowInvariants`.

## Blazor (Server + WASM hosted)
- **Framework**: Playwright E2E (Chromium) — `demo/DataFilter.Blazor.Demo.PlaywrightTests`
- **Routes**: `/demo/attach`, `/demo/local` (pipeline JSON), `/demo/async`, `/demo/hybrid`, `/demo/collectionview`
- **CI**: job `ui-blazor-playwright` (matrix `DF_DEMO_HOST=server|wasm`)

```powershell
$env:DF_DEMO_HOST = "server"   # ou wasm
dotnet test demo/DataFilter.Blazor.Demo.PlaywrightTests/DataFilter.Blazor.Demo.PlaywrightTests.csproj -c Release
```

## WPF / WinForms / WinUI 3 (desktop Windows)
### Option recommandée: FlaUI (UIA3)
- **Pourquoi**: UI Automation mature, accès aux **bounding rectangles** (positionnement), actions click/keyboard, robuste en CI Windows.
- **Avantages**: bonne granularité, pas besoin d’instrumenter l’app, bien adapté à la mesure de layout.
- **Limites**: UIA peut être sensible au timing; prévoir des waits explicites.

### Alternative: WinAppDriver / Appium (Windows)
- **Pourquoi**: approche WebDriver-like si tu veux aligner le style Playwright/WebDriver.
- **Avantages**: modèle “driver” standardisé.
- **Limites**: plus lourd à configurer; WinAppDriver n’est plus aussi “vivant” selon les teams.

## MAUI
### Option recommandée: Appium
- **Pourquoi**: solution cross-platform (Android/iOS/Windows) la plus standard.
- **Avantages**: si tu veux étendre ensuite la contract suite à Android/iOS, tu gardes le même framework.
- **Limites**: setup CI plus long (émulateurs/simulateurs). Pour MAUI Windows-only, FlaUI/WinAppDriver peut suffire.

## Desktop (FlaUI)
- **Projet**: `tests/UIContracts.FlaUI.Tests` (WPF, WinUI 3, WinForms smoke)
- **CI**: job `desktop-ui-contracts` sur runner **self-hosted** Windows (session interactive)
- **Prérequis local**: `dotnet build demo/DataFilter.Wpf.Demo -c Release` puis `dotnet test tests/UIContracts.FlaUI.Tests -c Release`

## MAUI (Appium)
- **Projet**: `tests/UIContracts.Appium.Tests`
- **Variables**: `UICT_APP_PLATFORM`, `UICT_APPIUM_SERVER`, `UICT_APP_PATH`
- **CI**: job `ui-maui-appium` (matrix Android/iOS, `continue-on-error` tant qu’Appium n’est pas câblé)

```powershell
$env:UICT_APP_PLATFORM = "android"
$env:UICT_APPIUM_SERVER = "http://127.0.0.1:4723/"
$env:UICT_APP_PATH = "path\to\app.apk"
dotnet test tests/UIContracts.Appium.Tests/UIContracts.Appium.Tests.csproj -c Release
```

## Structuration (parité contract)
- `TestUIContract.md` / `UIMatrixContract.md` — spécification
- `tests/UIContracts.Common` — données partagées
- `demo/DataFilter.Blazor.Demo.PlaywrightTests` — Blazor
- `tests/UIContracts.FlaUI.Tests` — desktop
- `tests/UIContracts.Appium.Tests` — MAUI

L’objectif est que chaque implémentation valide les mêmes invariants (filtrage visible, pipeline JSON, multi-colonnes, localisation).

