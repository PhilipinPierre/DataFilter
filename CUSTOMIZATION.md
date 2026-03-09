# Visual Customization Guide (WPF)

The `DataFilter.Wpf` module has been designed to be fully customizable without touching the C# code.

## 1. Theme Definition

Styles are grouped in **Resource Dictionaries**. The project provides two ready-to-use base themes:
- `pack://application:,,,/DataFilter.Wpf;component/Themes/FilterLightTheme.xaml`
- `pack://application:,,,/DataFilter.Wpf;component/Themes/FilterDarkTheme.xaml`

### Replace the theme globally (App.xaml)
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

## 2. Replacing Colors

You can override color keys defined by the theme either locally or globally.

```xml
<Color x:Key="FilterPopupBackgroundColor">#FAFAFA</Color>
<Color x:Key="FilterPopupForegroundColor">#333333</Color>
<Color x:Key="FilterPopupBorderColor">#E0E0E0</Color>

<!-- Brushes used by funnel buttons -->
<SolidColorBrush x:Key="FilterButtonActiveColor" Color="Orange" />
<SolidColorBrush x:Key="FilterButtonInactiveColor" Color="LightGray" />
```

## 3. Replacing the Filter Icon

By default, the icon is a funnel drawn as an SVG `Path`. The `ColumnFilterButton` control accepts an `IconTemplate` property that you can modify via a WPF style.

```xml
<Style TargetType="controls:ColumnFilterButton" BasedOn="{StaticResource {x:Type controls:ColumnFilterButton}}">
    <Setter Property="IconTemplate">
        <Setter.Value>
            <DataTemplate>
                <!-- Replace the Path with the icon of your choice, or FontAwesome, etc. -->
                <TextBlock Text="🔍" FontSize="14" 
                           Foreground="{Binding RelativeSource={RelativeSource AncestorType=controls:ColumnFilterButton}, Path=InactiveBrush}" />
            </DataTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

To handle the active/inactive color, bind your color to the `ActiveBrush` or `InactiveBrush` exposed by the `ColumnFilterButton` (which is itself attached to the state converter of the ViewModel).

## 4. Custom Value Templating (Checkboxes)

To display a custom visual for distinct value selection checkboxes (e.g., displaying a user's avatar instead of just their name), you can override the `ItemTemplate` at the `FilterPopup` control level.

```xml
<Style TargetType="ListBoxItem" x:Key="FilterValueItemContainerStyle">
    <!-- Custom logic -->
</Style>
```
