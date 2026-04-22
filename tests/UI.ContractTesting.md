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

## Blazor (Server + WASM hosted)
- **Framework**: Playwright E2E (Chromium).
- **Démo cible**: `/demo/attach` (headless table + `ColumnFilterButton` + `FilterPopup`).
- **Principe**: exécuter **la même suite** contre:
  - Host Server
  - Host WASM hosted

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

## Structuration proposée (parité contract)
- `contracts/` (description des scénarios, tolérances, dataset attendu)
- `demo/*PlaywrightTests` (implémentation Blazor)
- `tests/DesktopContracts.*` (implémentation FlaUI/WinAppDriver)
- `tests/MauiContracts.*` (implémentation Appium)

L’objectif est que chaque implémentation sorte un résultat comparable (mêmes noms de tests/scénarios, mêmes assertions qualitatives).

