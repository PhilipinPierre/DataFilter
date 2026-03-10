# DataFilter.Wpf.Demo

A demonstration application showcasing the full capabilities of the DataFilter library in various real-world scenarios.

## Overview

This project serves as both a showcase and a reference implementation for developers using the DataFilter library. It demonstrates how to wire up the controls with different types of data sources and requirements.

## Features Demonstrated

- **Local Filtering**: Filtering data directly in memory from a collection.
- **Async Filtering**: Simulating data fetching from a remote API with server-side filtering and sorting.
- **Hybrid Modes**: Combining local and remote data processing.
- **UI Customization**: Examples of how to style and customize the filter popups and data grid headers.
- **Control Integration**: Shows filtering integrated with `DataGrid`, `ListView`, and other WPF collection controls.

## Project Structure

- **Views**: Different pages for each demonstration scenario.
- **ViewModels**: Examples of how to structure your ViewModels to handle filtering logic.
- **Services**: Mock services that simulate data providers.

## Target Frameworks
- .NET 8.0-windows
- .NET 9.0-windows

## Dependencies
- `DataFilter.Wpf`
- `DataFilter.Core`
- `DataFilter.Filtering.ExcelLike`
- `CommunityToolkit.Mvvm`
