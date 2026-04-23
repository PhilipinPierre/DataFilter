# UI Matrix Contract (Cross-Framework)

Ce document est la **matrice complète** des contrats décrits dans `TestUIContract.md`, enrichie avec les besoins projet suivants :
- **Placement de la popup** selon la position du bouton + support **LTR/RTL**
- **Scroll** : la popup **suit** le bouton ; si le bouton sort du viewport, la popup reste **clampée à l’écran**
- **Popup redimensionnable**
- **Filtrage** : couverture des cas sur les données affichées
- **Tri** : couverture des cas sur les données affichées

Objectif : **alignement de comportement** entre stacks (pas de pixel-perfect).

---

## Légende

- **Frameworks**
  - **Blazor** : Playwright E2E sur `demo/DataFilter.Blazor.Demo.*` (référence : `demo/DataFilter.Blazor.Demo.PlaywrightTests`)
  - **Desktop** : UIA via **FlaUI** (WPF / WinForms / WinUI3) — tests `tests/UIContracts.FlaUI.Tests`
  - **MAUI** : **Appium** — tests `tests/UIContracts.Appium.Tests`
- **Niveau**
  - **Mandatory** : requis “toutes stacks” (dans `TestUIContract.md`)
  - **Recommended** : à implémenter quand faisable (dans `TestUIContract.md`)
  - **Project-required** : requis pour ce projet (peut s’appuyer sur “Recommended”)
- **Selectors / IDs**
  - Blazor : `data-testid` et `id` stables (ex : `df-filter-btn-Department`, `df-filter-popup-Department`)
  - WPF/WinUI3/MAUI : `AutomationId` sur l’ancre et la racine de popup
  - WinForms : si owner-drawn, interaction possible par coordonnées, mais assertions identiques

---

## Matrice des contrats

> Chaque ligne décrit : **intention**, **assertions**, et **stratégie par framework**.

### Contrats Mandatory (toutes stacks)

| ID | Contrat | Niveau | Assertions minimales | Blazor (Playwright) | Desktop (FlaUI) | MAUI (Appium) |
|---:|---|---|---|---|---|---|
| A | PopupOpenClose | Mandatory | Popup apparaît et est interactable, puis se ferme via au moins 1 mécanisme supporté (outside click / bouton / Esc) | `GetByTestId(df-filter-btn-*)` + attendre `#df-filter-popup-*` attach/detach | Cliquer sur l’ancre (AutomationId), attendre popup (AutomationId), fermer (click outside/Esc/bouton) | Taper/click sur ancre, attendre popup, fermer |
| B | AnchoredPositioning (Viewport/WorkArea Safe) | Mandatory + Project-required (LTR/RTL) | Popup positionnée relativement à l’ancre, **clampée** au viewport/work-area, tolérance px/DPI | Comparer bbox popup à `DataFilterInterops.getAnchoredPopupPosition(...)` (tolérance 1–3 px) | Vérifier : (1) alignement relatif ancre/popup (tolérance DPI), (2) popup entièrement visible dans work-area | Vérifier : alignement + clamp dans viewport device |
| C | ScrollKeepsPopupAnchored | Mandatory + Project-required (clamp offscreen) | Après scroll, popup suit l’ancre (tolérance). Si l’ancre sort du viewport, popup reste visible (clamp) | `window.scrollBy(...)` + recalcul bbox + comparer via interop ; ajouter cas “anchor offscreen => popup within viewport” | Faire défiler conteneur/hôte ; vérifier reposition & clamp via rectangles UIA + work-area | Scroll (gesture/programmatic), vérifier reposition & clamp |
| D | FilteringAffectsRows/Items | Mandatory + Project-required (“tous cas”) | Appliquer un filtre qui garantit un sous-ensemble : le dataset visible respecte un invariant, Clear restaure | Appliquer via UI popup, puis vérifier invariants sur DOM table (`tbody tr`) | Appliquer via UI, puis vérifier valeurs cellules/list-items | Appliquer via UI, vérifier items affichés |
| E | NoUnhandledErrors | Mandatory | Aucun “unhandled error”/écran d’erreur framework, pas de fatal runtime observable | Vérifier `#blazor-error-ui` non visible + console errors/fail requests | Vérifier absence de dialogs d’erreur/trace UI, et que l’app reste interactive | Idem (pas de crash, pas d’alerte error visible) |

### Contrats Recommended (à implémenter quand faisable)

| ID | Contrat | Niveau | Assertions minimales | Blazor (Playwright) | Desktop (FlaUI) | MAUI (Appium) |
|---:|---|---|---|---|---|---|
| F | OutsideClickDoesNotClickThrough | Recommended | Clic outside ferme la popup **sans action non liée** derrière | Clic coords hors overlay ; vérifier pas de sélection/tri involontaire | Clic hors popup ; vérifier pas d’événement “behind” | Tap hors popup ; vérifier pas d’action derrière |
| G | ResizeBehavior | Recommended + Project-required | Resize handle fonctionne, popup reste clampée, ancrage stable | Drag `.df-resize-handle` ; vérifier bbox change + clamp + interop position OK | Si handle exposé : drag ; sinon skip/coord ; vérifier taille + clamp | Drag handle/gesture ; vérifier taille + clamp |
| H | RTL layout | Recommended + Project-required | En RTL : ancrage + clamp restent corrects, popup utilisable | Basculer direction RTL (demo), rejouer B + C + interactions de base | Exécuter sur host RTL (si support) | Exécuter sur host RTL (si support) |
| I | Localization | Recommended | Changer culture => libellés représentatifs localisés | Utiliser `LanguagePicker` si présent, vérifier texte (Clear/Advanced filter/Sort) | Changer culture (si exposé), vérifier texte | Idem |

---

## Couverture “Filtrage” (contrat D) — plan de cas à automatiser

> Objectif : couvrir les opérateurs majeurs par **type de colonne** sur les données réellement affichées.
> Dataset Blazor demo : `Employee` (`Id`, `Name`, `Department`, `Country`, `Salary`, `HireDate`, `Time`, `IsActive`).

### D-STRING (ex: `Department`, `Name`, `Country`)
- **Equals** (ex : `Department == "IT"`) : invariant “toutes les lignes visibles ont Department == IT”.
- **Contains** (ex : `Name contains "Alice"`) : invariant “toutes les lignes visibles contiennent Alice”.
- **StartsWith / EndsWith** : invariants correspondants.
- **Wildcards** (si exposé via UI) : `A*`, `?lice*` : invariant matching.
- **NotEquals** (si exposé) : invariant “aucune ligne visible n’a la valeur interdite”.

### D-NUMBER (ex: `Salary`, `Id`)
- **Equals**
- **GreaterThan / LessThan**
- **Between** (inclure un test où \(min < max\) et un test où l’entrée est invalide et n’applique pas silencieusement)

### D-DATE (ex: `HireDate`)
- **Before / After** (ou équivalent dans la liste d’opérateurs)
- **Between** (dates)

### D-BOOL (ex: `IsActive`) (si UI custom le permet)
- True / False (ou “Equals true/false”)

### D-UX/STATE (Excel-like)
- **Search + SelectAll => pattern** (si c’est le comportement attendu pour préserver l’intention)
- **AddToExistingFilter** : Union & Intersection (2 actions successives) + invariant sur résultat
- **Clear** : restaure le dataset non filtré

---

## Couverture “Tri” (dans la popup) — plan de cas à automatiser

> Objectif : vérifier que les boutons de tri modifient bien l’ordre visible, y compris le multi-tri.

### S1 — Tri simple (asc/desc)
- **String** : `Department` Asc puis Desc : vérifier monotonicité lexicographique des valeurs affichées.
- **Number** : `Salary` Asc puis Desc : vérifier monotonicité numérique.
- **Date** : `HireDate` Asc/Desc : vérifier monotonicité temporelle.

### S2 — Sub-sort (multi-clés)
- **Primary + AddSubSort** : ex `Department` Asc + `AddSubSortAscending` sur `Name` (ou sur une 2e colonne) :
  - vérifier que le regroupement par clé primaire est respecté,
  - et qu’à clé primaire égale, la 2e clé est triée.

### S3 — Tri + Filtre (interaction)
- Appliquer un filtre (ex Dept=IT), puis un tri : vérifier que **le tri s’applique au sous-ensemble filtré**.

---

## Scénarios de positionnement (contrats B/C/H) — jeux de viewport et bords

Pour chaque framework, viser au moins ces scénarios :
- **Viewport standard** (ex 1280×720)
- **Viewport réduit** (ex 800×600) pour forcer le clamp
- **Anchor near bottom** : forcer ouverture “au-dessus” si la place manque (ou clamp)
- **Anchor near right/left edge** : clamp horizontal
- **LTR & RTL** : répéter les scénarios

---

## Exigences de testabilité (à maintenir)

Pour minimiser la flakiness et aligner les stacks :
- Exposer des IDs stables (Blazor `data-testid`, Desktop/MAUI `AutomationId`)
- Pour les surfaces owner-drawn (WinForms), documenter une stratégie stable (zones de clic + DPI)
- Toujours nettoyer : fermer popup, fermer app, tuer process en `finally` (cf. `TestUIContract.md`)

