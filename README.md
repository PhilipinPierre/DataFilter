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
- **Excel-like Selection**: Checkboxes for selecting specific values from the dataset.
- **Contextual Operators**: 
  - **Text**: Contains, Starts with, Ends with, Equals, etc.
  - **Numbers**: Greater than, Less than, Between, Equals.
  - **Dates**: Before, After, Between, and dynamic periods (Today, This Week, Last Month, etc.).
- **Cumulative Filtering**: Option to "Add to existing filter" to combine successive search results without resetting previous selections.

### 📶 Multi-Column Sorting
- Support for complex sorting scenarios.
- **Sub-sorting**: Use the "Add Sub-Sort" commands to define a secondary order (e.g., Order by *Name*, then by *Date*).

### 🌐 Asynchronous Data Loading
- **Server-side Filtering**: Implement `IAsyncDataProvider<T>` to offload filtering and sorting to an API or database.
- **On-demand Distinct Values**: Fetch unique values for the filter popup only when needed.

## Quick Start (WPF)

### 1. Add `FilterableDataGrid` to your XAML

```xml
<Window xmlns:controls="clr-namespace:DataFilter.Wpf.Controls;assembly=DataFilter.Wpf">
    <controls:FilterableDataGrid ItemsSource="{Binding FilteredItems}" 
                                 FilterContext="{Binding FilterContext}" />
</Window>
```

### 2. Connect your ViewModel

```csharp
public class MyViewModel
{
    public IFilterableDataGridViewModel<MyModel> GridViewModel { get; }

    public MyViewModel()
    {
        GridViewModel = new FilterableDataGridViewModel<MyModel>
        {
            LocalDataSource = _myList // or assign an AsyncDataProvider
        };
        GridViewModel.RefreshData();
    }
}
```

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
