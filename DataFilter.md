# META-PROMPT — Système de Filtrage de Données WPF (ExcelLike + API Async)

---

## 🎯 Contexte & Objectif

Tu es un expert en développement .NET/WPF, architecture logicielle et conception de bibliothèques réutilisables.  
Tu dois générer une **solution Visual Studio complète**, structurée en plusieurs projets, implémentant un **système de filtrage de données visuel pour WPF**, inspiré du filtrage Excel, avec support du chargement asynchrone de données depuis une API externe.

---

## 📁 Structure de la Solution

La solution doit s'appeler `DataFilter.sln` et contenir les projets suivants :

```
DataFilter.sln
│
├── src/
│   ├── DataFilter.Core/                  # Bibliothèque .NET Standard/Net8 — logique pure, sans UI
│   ├── DataFilter.Filtering.ExcelLike/   # Bibliothèque .NET — moteur de filtrage style Excel
│   ├── DataFilter.Wpf/                   # Bibliothèque WPF — contrôles, styles, comportements
│   └── DataFilter.Wpf.Demo/             # Application WPF de démonstration
│
└── tests/
    ├── DataFilter.Core.Tests/
    ├── DataFilter.Filtering.ExcelLike.Tests/
    └── DataFilter.Wpf.Tests/
```

---

## 📦 Projet 1 — `DataFilter.Core`

**Type :** Class Library (.NET 8 / .NET Standard 2.1)  
**Rôle :** Contient toutes les abstractions et la logique de filtrage indépendante de toute UI ou framework de données.

### Éléments à implémenter :

#### Interfaces
- `IFilterDescriptor` — contient `PropertyName`, `FilterOperator`, `Value`, méthode `IsMatch(object item)`
- `IFilterGroup` — agrège plusieurs `IFilterDescriptor` avec un opérateur logique (`And` / `Or`)
- `IFilterEngine<T>` — applique une liste de descripteurs sur `IEnumerable<T>`, retourne `IEnumerable<T>`
- `IAsyncDataProvider<T>` — définit `Task<IEnumerable<T>> FetchAsync(FilterContext context, CancellationToken ct)`
- `IFilterContext` — encapsule l'état courant du filtrage : descripteurs actifs, pagination, tri

#### Classes
- `FilterDescriptor` — implémentation par défaut de `IFilterDescriptor`
- `FilterGroup` — implémentation par défaut de `IFilterGroup`
- `FilterContext` — état du filtrage (descripteurs, sort, page, pageSize)
- `FilterOperator` — enum : `Contains`, `StartsWith`, `EndsWith`, `Equals`, `NotEquals`, `GreaterThan`, `LessThan`, `In`, `NotIn`, `IsNull`, `IsNotNull`
- `FilterChangedEventArgs` — événement déclenché lors de la modification d'un filtre

#### Utilitaires
- `ReflectionFilterEngine<T>` — implémentation de `IFilterEngine<T>` basée sur la réflexion et les expressions compilées (`Expression<Func<T, bool>>`)
- `FilterExpressionBuilder` — construit des `Expression<Func<T, bool>>` depuis un `IFilterDescriptor`

---

## 📦 Projet 2 — `DataFilter.Filtering.ExcelLike`

**Type :** Class Library (.NET 8)  
**Dépendances :** `DataFilter.Core`  
**Rôle :** Implémente la logique du filtrage style Excel (multi-valeurs, checkboxes, recherche texte).

### Éléments à implémenter :

#### Modèles
- `ExcelFilterState` — état du filtre pour une colonne : `SearchText`, `SelectedValues`, `SelectAll`, `DistinctValues`
- `ExcelFilterDescriptor : IFilterDescriptor` — génère un `IFilterDescriptor` composite depuis un `ExcelFilterState`

#### Services
- `DistinctValuesExtractor` — extrait les valeurs distinctes d'une propriété depuis une collection locale (`IEnumerable<T>`)
- `ExcelFilterEngine<T> : IFilterEngine<T>` — applique la logique Excel (combines text search + selected values)
- `ExcelLikeAsyncFilter<T>` — orchestre la récupération async des `DistinctValues` via `IAsyncDataProvider<T>` et applique le filtre

#### Logique spécifique Excel
- Gestion du **"Sélectionner tout"** (toggle all checkboxes)
- Filtre de **recherche textuelle** au sein des valeurs distinctes
- Support du filtre combiné **texte + valeurs sélectionnées**
- Tri des valeurs distinctes (alphabétique, numérique, date)

---

## 📦 Projet 3 — `DataFilter.Wpf`

**Type :** WPF Class Library (.NET 8-windows)  
**Dépendances :** `DataFilter.Core`, `DataFilter.Filtering.ExcelLike`  
**Rôle :** Fournit les contrôles WPF, les comportements, les styles et les templates customisables.

### Éléments à implémenter :

#### Contrôles WPF

**`FilterableDataGrid : DataGrid`**
- Hérite de `DataGrid`
- Propriétés : `IAsyncDataProvider AsyncDataProvider`, `bool EnableAsyncFetch`, `FilterContext FilterContext`
- Injecte automatiquement le bouton de filtre dans les headers de colonnes
- Expose des `DependencyProperty` pour la customisation

**`FilterableGridView : GridView`** (pour `ListView`)
- Même concept appliqué aux `GridViewColumn`

**`ColumnFilterButton : Button`**
- Bouton affiché à droite du texte d'en-tête de colonne
- Icône configurable (funnel/entonnoir par défaut)
- État visuel : filtre actif (coloré) / inactif (grisé)
- Déclenche l'ouverture du `FilterPopup`

**`FilterPopup : Popup` (ou `UserControl`)**
- Panneau de filtrage Excel-like
- Zones : barre de recherche, liste de checkboxes avec valeurs, boutons OK/Annuler/Effacer
- Supporte le chargement async des valeurs distinctes avec indicateur de chargement (`BusyIndicator`)
- Scrollable si la liste est longue

**`FilterValueItem`** — item de la liste de checkboxes (valeur + état coché)

#### Behaviors (System.Windows.Interactivity / Microsoft.Xaml.Behaviors)
- `FilterableColumnHeaderBehavior` — Behavior attachable sur n'importe quel `DataGrid` ou `ListView` existant, sans héritage
- `AsyncFilterBehavior` — gère les appels async et le debounce lors de la saisie dans la barre de recherche

#### Converters
- `BoolToVisibilityConverter`
- `FilterActiveToColorConverter` — change la couleur du bouton selon l'état actif du filtre
- `NullOrEmptyToVisibilityConverter`

#### Styles & Templates (fichier `Themes/Generic.xaml`)
Tous les styles doivent être **surchargeables** via les ressources de l'application :
- `FilterButtonStyle` — style du bouton entonnoir
- `FilterPopupStyle` — style du panneau popup
- `FilterSearchBoxStyle` — style de la barre de recherche
- `FilterCheckBoxStyle` — style des checkboxes
- `FilterValueItemContainerStyle` — style des items de liste
- `BusyIndicatorStyle` — style du loader async

Chaque contrôle doit exposer des `DependencyProperty` permettant de surcharger :
- Couleurs (actif, inactif, hover, fond)
- Icône du bouton (accepte un `DataTemplate`)
- Template de l'item de liste (pour afficher des avatars, couleurs, badges, etc.)

#### ViewModel (optionnel MVVM support)
- `ColumnFilterViewModel` — ViewModel du popup de filtrage, expose `SearchText`, `FilterValues`, `SelectAll`, commandes
- `FilterableDataGridViewModel<T>` — ViewModel parent qui orchestre l'ensemble

---

## 📦 Projet 4 — `DataFilter.Wpf.Demo`

**Type :** Application WPF (.NET 8-windows)  
**Rôle :** Démontre toutes les fonctionnalités du système.

### Contenu attendu :

#### Scénarios démontrés
1. **Mode local** — DataGrid chargé avec des données en mémoire, filtrage Excel-like pur
2. **Mode async** — DataGrid avec `IAsyncDataProvider` simulant un appel HTTP (mock), valeurs distinctes récupérées depuis l'API
3. **Mode hybride** — filtrage local + rechargement async lors de la modification du filtre texte
4. **Customisation** — démonstration du remplacement des styles (thème sombre, icône personnalisée, template d'item custom)
5. **ListView + GridView** — même système appliqué à un `ListView` avec `GridViewColumn`

#### Données de démo
- Classe `Employee` : `Id`, `Name`, `Department`, `Country`, `Salary`, `HireDate`, `IsActive`
- `MockEmployeeApiService : IAsyncDataProvider<Employee>` — simule un appel API avec `Task.Delay` et retourne des données filtrées

#### UI Demo
- Onglets ou régions pour chaque scénario
- Panneau de customisation live (sliders, pickers de couleur) pour modifier les styles en temps réel

---

## 🔌 Contrat `IAsyncDataProvider<T>`

```csharp
public interface IAsyncDataProvider<T>
{
    /// <summary>
    /// Récupère les données filtrées depuis la source externe (API, BDD, etc.)
    /// </summary>
    Task<PagedResult<T>> FetchDataAsync(FilterContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère les valeurs distinctes d'une colonne pour le panneau de filtrage
    /// </summary>
    Task<IEnumerable<object>> FetchDistinctValuesAsync(string propertyName, string searchText, CancellationToken cancellationToken = default);
}

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
```

---

## ⚙️ Contraintes Techniques

| Contrainte | Détail |
|---|---|
| Framework | .NET 8, WPF (windows target), .NET Standard 2.1 pour Core |
| Pattern UI | MVVM (CommunityToolkit.Mvvm recommandé) |
| Async | `async/await`, `CancellationToken`, debounce sur la recherche (300ms) |
| Customisation | Tous les templates en `Generic.xaml`, surchargeables via `Application.Resources` |
| NuGet | Pas de dépendances lourdes, préférer les APIs natives .NET |
| Tests | xUnit + Moq, couverture des cas de filtrage Core et ExcelLike |
| Nullable | `#nullable enable` dans tous les projets |
| Documentation | Commentaires XML sur toutes les interfaces et membres publics |

---

## 🎨 Règles de Customisation (point fondamental)

Chaque contrôle WPF **doit** respecter ces règles :

1. **Aucun style hardcodé** — tout passe par des ressources nommées ou des `DependencyProperty`
2. **`ControlTemplate` remplaçable** — chaque contrôle expose un `Template` standard et un exemple de remplacement complet
3. **`DataTemplate` injectables** — le bouton de filtre accepte un `DataTemplate` pour l'icône ; la liste de valeurs accepte un `ItemTemplate`
4. **Héritage de `ResourceDictionary`** — fournir un `FilterDarkTheme.xaml` et un `FilterLightTheme.xaml` prêts à l'emploi
5. **Propriétés de style exposées sur le contrôle parent** :
   - `FilterButtonActiveColor`
   - `FilterButtonInactiveColor`
   - `FilterPopupBackground`
   - `FilterPopupMaxHeight`
   - `FilterButtonIconTemplate`

---

## 🧪 Cas de Tests Attendus

### Core
- `FilterExpressionBuilder` génère des expressions valides pour chaque `FilterOperator`
- `ReflectionFilterEngine` filtre correctement une liste d'objets
- `FilterGroup` combine correctement des descripteurs en AND et OR

### ExcelLike
- `DistinctValuesExtractor` retourne les bonnes valeurs distinctes triées
- `ExcelFilterEngine` applique correctement texte + sélection
- Le toggle "Sélectionner tout" met à jour tous les items

### WPF (tests d'intégration UI)
- Le bouton de filtre apparaît dans chaque header de colonne
- Le popup s'ouvre au clic
- L'état actif du filtre change la couleur du bouton
- Le chargement async affiche bien l'indicateur de chargement

---

## 📋 Livrables Attendus

Pour chaque projet, génère :
- [ ] Tous les fichiers `.cs` avec le code complet et fonctionnel
- [ ] Les fichiers `.xaml` complets (styles, templates, fenêtres demo)
- [ ] Les fichiers `.csproj` avec les bonnes références et `PackageReference`
- [ ] Le fichier `DataFilter.sln`
- [ ] Un `README.md` global expliquant l'architecture et comment utiliser la bibliothèque dans un projet externe
- [ ] Un `CUSTOMIZATION.md` dédié à la customisation visuelle avec exemples XAML complets

---

## 🚀 Instructions de Génération

1. **Commence par** générer la structure de solution et tous les `.csproj`
2. **Ensuite** génère `DataFilter.Core` en intégralité (interfaces, classes, moteur)
3. **Ensuite** génère `DataFilter.Filtering.ExcelLike` en intégralité
4. **Ensuite** génère `DataFilter.Wpf` : commence par `Generic.xaml` puis les contrôles
5. **Ensuite** génère `DataFilter.Wpf.Demo`
6. **Enfin** génère les projets de tests
7. À chaque étape, **vérifie la cohérence** des références inter-projets avant de passer à la suivante
8. **Ne tronque aucun fichier** — le code doit être complet et compilable

---

*Ce meta-prompt est destiné à un LLM spécialisé en génération de code .NET/WPF. Toutes les décisions d'architecture décrites ici sont des contraintes fermes, non des suggestions.*
