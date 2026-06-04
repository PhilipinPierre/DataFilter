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
- **Projet**: `tests/UIContracts.FlaUI.Tests` (WPF, WinUI 3, WinForms)
- **CI**: job `desktop-ui-contracts` **uniquement sur déclenchement manuel** (`workflow_dispatch` + case « Run FlaUI desktop UI contracts »). Nécessite un runner **self-hosted** (`self-hosted`, `Windows`, `X64`). Les PR/push ne lancent pas ce job (pas de runner personnel requis pour merger).
- **Prérequis local**:

```powershell
dotnet build demo/DataFilter.Wpf.Demo/DataFilter.Wpf.Demo.csproj -c Release
dotnet build demo/DataFilter.WinForms.Demo/DataFilter.WinForms.Demo.csproj -c Release
dotnet build demo/DataFilter.WinUI3.Demo/DataFilter.WinUI3.Demo.csproj -c Release -p:Platform=x64
dotnet test tests/UIContracts.FlaUI.Tests/UIContracts.FlaUI.Tests.csproj -c Release
```

### Configurer un runner self-hosted (GitHub Actions)

1. Sur une VM/workstation **Windows 10/11** connectée (session utilisateur, pas Session 0 seule).
2. Installer .NET SDK (voir `global.json`), workloads MAUI si besoin, **Windows App Runtime** pour WinUI3.
3. Enregistrer le runner avec les labels : `self-hosted`, `Windows`, `X64`.
4. Vérifier : `dotnet test tests/UIContracts.FlaUI.Tests -c Release` passe en local sur la machine.
5. Les jobs `desktop-ui-contracts` échouent sur `windows-latest` (pas d’UIA interactive fiable).

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

