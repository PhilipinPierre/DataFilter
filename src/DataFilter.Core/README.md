# DataFilter.Core

The foundation of the DataFilter library, providing pure filtering logic, abstractions, and standard data models.

## Structure
- **Abstractions**: Defines `IFilterEngine`, `IFilterDescriptor`, `IAsyncDataProvider`, etc.
- **Engine**: Includes `FilterExpressionBuilder` for standard LINQ filter generation.
- **Models**: Standardized `FilterSnapshot`, `SortDescriptor`, and `PagedResult`.

## Key Concepts

### `FilterSnapshot`
A serializable representation of a filtering state that can be passed between layers (e.g., from Blazor UI to Web API).

### `IAsyncDataProvider`
Implement this interface to provide data asynchronously from any source (SQL, NoSQL, API).

```csharp
public interface IAsyncDataProvider<T>
{
    Task<PagedResult<T>> GetDataAsync(IFilterContext context, CancellationToken ct);
    Task<IEnumerable<object>> GetDistinctValuesAsync(string propertyName, string searchText, CancellationToken ct);
}
```
