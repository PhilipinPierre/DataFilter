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
