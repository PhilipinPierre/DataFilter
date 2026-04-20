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

## How to Run

1. Open the solution in **Visual Studio 2022**.
2. Set `DataFilter.Wpf.Demo` as the **Startup Project**.
3. Press **F5** or click **Start**.

## Demonstration Scenarios

### 1. Local Filtering
Standard in-memory filtering of a collection of `Employee` objects. Showcase how to use `FilterableDataGrid` with simple property binding. This scenario also includes a **filter pipeline JSON** panel: **Sync from grid** exports the current filters to `FilterPipelineSnapshot` JSON (via `CreatePipelineFromCurrentSnapshot` and `FilterPipelineJson`), and **Apply JSON** loads a preset back with `ApplyFilterPipelineAsync`.

### 2. Async Filtering (Remote API Simulation)
Simulates a long-running data fetch from a server. Demonstrates the use of `IAsyncDataProvider` to handle filtering and sorting on the "server side".

### 3. GridView / ListView
Shows how to add filtering capabilities to a standard `ListView` using the `FilterableColumnHeaderBehavior`.

### 4. Custom Styling
Shows how to apply the provided Light and Dark themes to the filter controls.

## Project Structure

- **Views**: Different pages for each demonstration scenario.
- **ViewModels**: Examples of how to structure your ViewModels to handle filtering logic.
- **Services**: Mock services that simulate data providers (`EmployeeDataGenerator`).

## Target Frameworks
- .NET 8.0-windows
- .NET 9.0-windows

## Dependencies
- `DataFilter.Wpf`
- `DataFilter.Core`
- `DataFilter.Filtering.ExcelLike`
- `CommunityToolkit.Mvvm`

## Runtime language switching (localization test)

This demo includes a **Language** dropdown in the top bar to switch the UI culture at runtime.

The available languages come from the satellite resource assemblies embedded in **`DataFilter.Localization`**.
