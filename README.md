# DataFilter WPF

A visual data filtering system for WPF, inspired by Excel filtering, with support for asynchronous data loading from external APIs.

## Architecture

The solution is divided into 4 main projects:

1.  **`DataFilter.Core`** (.NET 8 / .NET Standard 2.1)
    - Contains pure filtering logic and abstractions (`IFilterEngine`, `IFilterDescriptor`).
    - UI-independent.
2.  **`DataFilter.Filtering.ExcelLike`** (.NET 8)
    - Implements advanced filtering engine with distinct value selection formatted like Excel.
    - Handles complex composite filters (manual selection + contextual operators).
3.  **`DataFilter.Wpf`** (.NET 8 / 9 Windows)
    - Provides WPF controls (`FilterableDataGrid`, `FilterableGridView`, `FilterPopup`) and attachable behaviors.
    - Supports multi-targeting and modern WPF features.
4.  **`DataFilter.Wpf.Demo`** (.NET 8 / 9 Windows)
    - Demonstration application highlighting various filtering scenarios (Local, Async, Hybrid, Customization, ListView).

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
- **Sub-sorting**: Use the "Add Sub-Sort" commands to define a secondary order (e.g., Order by Name, then by Date).

### 🌐 Asynchronous Data Loading
- **Server-side Filtering**: Implement `IAsyncDataProvider<T>` to offload filtering and sorting to an API or database.
- **On-demand Distinct Values**: Fetch unique values for the filter popup only when needed.

## Quick Start (WPF)

### 1. Simple Local Filtering

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

- [**DataFilter.Wpf**](src/DataFilter.Wpf/README.md): Detailed UI setup, theming, and behaviors.
- [**DataFilter.Core**](src/DataFilter.Core/README.md): Extending the filtering engine.
- [**DataFilter.Wpf.Demo**](src/DataFilter.Wpf.Demo/README.md): Reference implementation.

## Visual Customization

The controls are designed using `Generic.xaml` with no hardcoded styles, making them fully themeable.
Two base themes are provided: `FilterLightTheme.xaml` and `FilterDarkTheme.xaml`.

For more details on customizing colors, icons, and templates, see [CUSTOMIZATION.md](CUSTOMIZATION.md).

## Unit Testing
The solution includes a comprehensive test suite covering:
- Core expression building logic.
- Excel-like descriptor combinations.
- Server-side queryable integration.
- ViewModel commands and state management.
