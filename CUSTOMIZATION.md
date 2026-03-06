# Guide de Personnalisation Visuelle (WPF)

Le module `DataFilter.Wpf` a été conçu pour être entièrement modifiable sans retoucher le code C#.

## 1. Définition des Thèmes

Les styles sont regroupés dans des **Resource Dictionaries**. Le projet fournit deux thèmes de base prêts à l'emploi :
- `pack://application:,,,/DataFilter.Wpf;component/Themes/FilterLightTheme.xaml`
- `pack://application:,,,/DataFilter.Wpf;component/Themes/FilterDarkTheme.xaml`

### Remplacer le thème globalement (App.xaml)
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

## 2. Remplacer les Couleurs

Vous pouvez écraser localement ou globalement les clés de couleurs définies par le thème.

```xml
<Color x:Key="FilterPopupBackgroundColor">#FAFAFA</Color>
<Color x:Key="FilterPopupForegroundColor">#333333</Color>
<Color x:Key="FilterPopupBorderColor">#E0E0E0</Color>

<!-- Pinceaux utilisés par les boutons d'entonnoir -->
<SolidColorBrush x:Key="FilterButtonActiveColor" Color="Orange" />
<SolidColorBrush x:Key="FilterButtonInactiveColor" Color="LightGray" />
```

## 3. Remplacer l'Icône de Filtre

Par défaut, l'icône est un entonnoir dessiné en `Path` SVG. Le contrôle `ColumnFilterButton` accepte une propriété `IconTemplate` que vous pouvez modifier via un style WPF.

```xml
<Style TargetType="controls:ColumnFilterButton" BasedOn="{StaticResource {x:Type controls:ColumnFilterButton}}">
    <Setter Property="IconTemplate">
        <Setter.Value>
            <DataTemplate>
                <!-- Remplacez le Path par l'icône de votre choix, ou FontAwesome, etc. -->
                <TextBlock Text="🔍" FontSize="14" 
                           Foreground="{Binding RelativeSource={RelativeSource AncestorType=controls:ColumnFilterButton}, Path=InactiveBrush}" />
            </DataTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

Pour gérer la couleur active/inactive, liez votre couleur au `ActiveBrush` ou `InactiveBrush` exposé par le `ColumnFilterButton` (qui est lui-même rattaché au converter d'état du ViewModel).

## 4. Templating Personnalisé des Valeurs (Checkboxes)

Pour afficher un visuel personnalisé pour les checkboxes de sélection de valeur distincte (ex: afficher l'avatar d'un utilisateur au lieu de juste son nom), vous pouvez surcharger le `ItemTemplate` au niveau du contrôle `FilterPopup`.

```xml
<Style TargetType="ListBoxItem" x:Key="FilterValueItemContainerStyle">
    <!-- Logique personnalisée -->
</Style>
```
