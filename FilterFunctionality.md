# Guide Complet des Fonctionnalités du Menu de Filtre Excel

Le menu contextuel de filtrage d'Excel (accessible via la flèche en en-tête de colonne) est un centre de contrôle complet pour organiser et isoler vos données. Voici l'inventaire de ses fonctionnalités.

---

## 1. Fonctions de Tri (Sorting)
Permet d'organiser l'ordre d'affichage des lignes sans masquer de données :
* **Tri de A à Z / Du plus petit au plus grand :** Classement croissant.
* **Tri de Z à A / Du plus grand au plus petit :** Classement décroissant.
* **Trier par couleur :** Remonte en haut de liste les cellules selon leur couleur de remplissage ou la couleur de la police.

---

## 2. Sélection Manuelle et Recherche
Idéal pour les manipulations rapides sur des valeurs spécifiques :
* **Barre de recherche :** Permet de taper une partie d'un mot ou d'un chiffre pour filtrer instantanément.
* **Ajouter la sélection actuelle au filtre :** Permet de cumuler plusieurs résultats de recherches successives.
* **Sélectionner tout :** Cocher ou décocher l'intégralité des valeurs visibles.
* **(Vides) :** Option située en bas de liste pour n'afficher que les lignes dont la cellule est vide.

---

## 3. Filtres par Mise en Forme
Indispensable si vous utilisez des codes couleurs ou des mises en forme conditionnelles :
* **Filtrer par couleur de cellule :** N'affiche que les lignes ayant une couleur de fond spécifique.
* **Filtrer par couleur de police :** N'affiche que les lignes dont le texte est d'une couleur précise.
* **Filtrer par icône de condition :** Si des jeux d'icônes (feux, flèches) sont appliqués, permet de filtrer par symbole.

---

## 4. Filtres Contextuels (selon le type de données)
Excel adapte les options du menu selon le contenu détecté dans la colonne.

### A. Filtres Textuels
| Option | Description |
| :--- | :--- |
| **Égal à / Différent de** | Correspondance exacte. |
| **Commence par / Se termine par** | Utile pour les préfixes ou suffixes (ex: codes produits). |
| **Contient / Ne contient pas** | Recherche de mots-clés à l'intérieur d'une cellule. |
| **Filtre personnalisé** | Permet de combiner deux critères avec les opérateurs "ET" ou "OU". |

### B. Filtres Numériques
| Option | Description |
| :--- | :--- |
| **Supérieur / Inférieur à** | Pour isoler des valeurs au-dessus ou en dessous d'un seuil. |
| **Entre** | Pour définir une plage de valeurs (ex: entre 100 et 500). |
| **10 premiers...** | Affiche les X valeurs les plus hautes (ou basses), en nombre ou en %. |
| **Au-dessus / En dessous de la moyenne** | Calculé dynamiquement sur l'ensemble de la colonne. |

### C. Filtres Chronologiques (Dates)
Excel regroupe les dates par hiérarchie (Année > Mois > Jour).
* **Périodes dynamiques :** Aujourd'hui, Hier, Demain, La semaine dernière, Ce mois-ci, Ce trimestre, l'Année dernière, etc.
* **Toutes les dates de la période :** Permet de filtrer, par exemple, tous les mois de "Mai" quelle que soit l'année.
* **Plages temporelles :** Avant le..., Après le..., ou Entre deux dates.

---

## 5. Gestion et Maintenance du Filtre
* **Effacer le filtre de [Nom de la colonne] :** Réinitialise la colonne sans impacter les filtres des autres colonnes.
* **Réappliquer :** Met à jour le filtrage si des données ont été modifiées ou ajoutées après l'activation du filtre.

---

## 💡 Astuce d'expert : Caractères génériques
Dans la barre de recherche, vous pouvez utiliser :
* `*` (astérisque) pour remplacer une suite de caractères (ex: `*nord` trouvera "Grand-Nord" et "Sud-Nord").
* `?` (point d'interrogation) pour remplacer un seul caractère (ex: `p?rt` trouvera "part" et "port").