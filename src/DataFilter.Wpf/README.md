# DataFilter.Wpf

A comprehensive set of WPF controls, ViewModels, and behaviors to integrate the DataFilter system into Windows desktop applications.

## Overview

`DataFilter.Wpf` provides a rich UI layer that connects to the underlying filtering engines. It is designed with MVVM patterns in mind and is highly customizable.

## Key Features

- **Controls**: 
  - `FilterableDataGrid`: A drop-in replacement for the standard WPF DataGrid with built-in filtering headers.
  - `FilterPopup`: The actual UI menu used for choosing filter criteria.
- **ViewModels**: Robust ViewModels (using CommunityToolkit.Mvvm) that manage the filter state and communicate with the data source.
- **Behaviors**: Attachable behaviors to add filtering capabilities to existing controls like `ListView` or `GridView`.
- **Theming**: Fully themeable using XAML resources. Supports Light and Dark modes out of the box.

## Usage Examples

### 1. Simple DataGrid Integration

```xml
<controls:FilterableDataGrid ItemsSource="{Binding FilteredItems}" 
                             FilterContext="{Binding GridViewModel.Context}" />
```

### 2. ListView/GridView Integration

Use the `FilterableColumnHeaderBehavior` to add Excel-like filtering to any `GridViewColumn`.

```xml
<ListView ItemsSource="{Binding FilteredItems}">
    <ListView.View>
        <GridView>
            <GridViewColumn Header="Name" 
                            DisplayMemberBinding="{Binding Name}"
                            behaviors:FilterableColumnHeaderBehavior.IsFilterable="True" />
        </GridView>
    </ListView.View>
</ListView>
```

## Advanced Features

### Real-time Selection Synchronization

Changing a custom operator (like **"Contains"**) automatically updates the selection list in real-time. This synchronization works recursively for tree-based filters (like Dates).

### Accumulation (Add to Current Selection)

Users can build complex filters by checking "Add current selection to filter". This merges successive filter results into a static selection list.

## Components

### Controls & Themes
Contains the XAML templates and custom control logic for the UI.

### ViewModels
Provides `FilterableDataGridViewModel` and related classes that bridge the UI to the `DataFilter.Core` logic.

### Behaviors & Converters
Utilities and XAML behaviors to simplify integration into existing WPF views.

## Target Frameworks
- .NET 8.0-windows
- .NET 9.0-windows

## Dependencies
- `DataFilter.Core`
- `DataFilter.Filtering.ExcelLike`
- `CommunityToolkit.Mvvm`
- `Microsoft.Xaml.Behaviors.Wpf`
