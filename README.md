# DataFilter WPF

A visual data filtering system for WPF, inspired by Excel filtering, with support for asynchronous data loading from external APIs.

The solution is divided into several main projects:

1.  **`DataFilter.Core`** (.NET 8 / 9 / .NET Standard 2.0 & 2.1)
    - Contains pure filtering logic and abstractions (`IFilterEngine`, `IFilterDescriptor`).
    - UI-independent.
2.  **`DataFilter.Filtering.ExcelLike`** (.NET 8 / 9)
    - Implements advanced filtering engine with distinct value selection formatted like Excel.
    - Handles complex composite filters (manual selection + contextual operators).
3.  **`DataFilter.Wpf`** (.NET 8 / 9 Windows)
    - Provides WPF controls (`FilterableDataGrid`, `FilterableGridView`, `FilterPopup`) and attachable behaviors.
4.  **`DataFilter.Blazor`** (.NET 8 / 9)
    - Provides Blazor components (`ColumnFilterButton`, `FilterPopup`) for WebAssembly, Server-side, and Hybrid.
    - Modern and fully customizable UI via CSS classes.
5.  **`DataFilter.Expressions.Server`** (.NET 8 / 9)
    - Extension for converting filter snapshots into LINQ Expressions for server-side evaluation.

## Key Features

### 🚀 Advanced Filtering
- **Excel-like Selection**: Multi-select checkboxes with hierarchical support (e.g., Dates grouped by Year/Month/Day).
- **Advanced Synchronization**: Changing a custom operator (like "Contains") automatically updates the selection list in real-time.
- **Contextual Operators**: 
  - **Text**: Contains, Not Contains, Starts with, Ends with, Equals, Not Equals.
  - **Numbers/Dates/Time**: Greater than, Less than, Between, Equals, Not Equals.
- **Additive & Refinement Modes**: 
  - **Union (Additive)**: Merges new matches with the current selection (Logical OR).
  - **Intersection (Refinement)**: Keeps only items that match BOTH the current selection and the new criteria (Logical AND).
- **Cumulative Filtering**: "Add to current selection" mode allows merging successive search results.

### 📶 Multi-Column Sorting
- **Sub-sorting**: Define secondary and tertiary order (e.g., Order by Name, then by Date).

### 🌐 Asynchronous Data Loading
- **Server-side Filtering**: Implement `IAsyncDataProvider<T>` to offload filtering and sorting to an API or database.
- **On-demand Distinct Values**: Fetch unique values for the filter popup only when needed.

## Quick Start (WPF)

```xml
<controls:FilterableDataGrid ItemsSource="{Binding FilteredItems}" 
                             FilterContext="{Binding GridViewModel.Context}" />
```

```csharp
public class MyViewModel : ObservableObject
{
    public FilterableDataGridViewModel<MyItem> GridViewModel { get; }

    public MyViewModel()
    {
        GridViewModel = new FilterableDataGridViewModel<MyItem>
        {
            LocalDataSource = _myFullCollection
        };
        // Initialization
        _ = GridViewModel.RefreshDataAsync();
    }
}
```

### 2. Manual Integration (e.g. into GridView)

```xml
<GridViewColumn Header="Name" 
                DisplayMemberBinding="{Binding Name}"
                behaviors:FilterableColumnHeaderBehavior.IsFilterable="True" />
```

## Detailed Usage & Examples

For more in-depth examples and configuration options, please refer to the project-specific documentation:

- [**DataFilter.Wpf**](src/DataFilter.Wpf/README.md): Detailed UI setup, theming, and behaviors for WPF.
- [**DataFilter.Blazor**](src/DataFilter.Blazor/README.md): Component usage, styling, and host configuration for Blazor.
- [**DataFilter.Core**](src/DataFilter.Core/README.md): Extending the filtering engine and understanding abstractions.
- [**DataFilter.Expressions.Server**](src/DataFilter.Expressions.Server/README.md): Integrating server-side filtering with LINQ.

## Visual Customization

### WPF
The WPF controls are designed using `Generic.xaml` with no hardcoded styles.
Two base themes are provided: `FilterLightTheme.xaml` and `FilterDarkTheme.xaml`.

### Blazor
The Blazor components use modern Vanilla CSS with explicit classes (prefix `df-`).
Customization is done by overriding these classes in your app's stylesheet.

See [CUSTOMIZATION.md](CUSTOMIZATION.md) for full details on both platforms.

## Unit Testing
The solution includes a comprehensive test suite (75+ tests) covering:
- Core expression building logic.
- Excel-like descriptor combinations.
- Server-side queryable integration.
- WPF & Blazor ViewModel commands and state management.

To run all tests:
```bash
dotnet test
```
