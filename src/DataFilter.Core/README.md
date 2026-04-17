# DataFilter.Core

The foundation of the DataFilter library, providing pure filtering logic, abstractions, and standard data models.

## Structure
- **Abstractions**: Defines `IFilterEngine`, `IFilterDescriptor`, `IFilterContext`, `IAsyncDataProvider`, etc.
- **Engine**: Includes `FilterExpressionBuilder` for standard LINQ filter generation.
- **Models**: Standardized `FilterSnapshot`, `FilterPipelineSnapshot`, `SortDescriptor`, and `PagedResult`.
- **Pipeline** (`Pipeline/`): `FilterPipeline`, `CriterionPipelineNode`, `GroupPipelineNode` — editable graph for UI scenarios (order, named groups, toggles).
- **Services**: `FilterSnapshotBuilder`, `FilterPipelineCompiler`, `FilterPipelineSnapshotMapper`, `FilterPipelineInterop`, `FilterPipelineContextExtensions`.

## Key Concepts

### `FilterSnapshot`
A serializable representation of a filtering state that can be passed between layers (e.g., from Blazor UI to Web API). Describes **what** is filtered using flat or nested `FilterSnapshotEntry` rows (including logical groups for Excel-style column scopes).

### Filter pipeline
For applications that need an **ordered list** of criteria, **named groups**, **enable/disable** flags, and **JSON presets**, use the pipeline model:

| Type | Role |
|------|------|
| `FilterPipeline` | Root list of nodes + `RootCombineOperator` (AND/OR between top-level nodes). |
| `CriterionPipelineNode` | Leaf: property name, operator name (`FilterOperator`), and value (same shapes as `FilterSnapshotEntry`). |
| `GroupPipelineNode` | Named group: `DisplayName`, `CombineOperator`, ordered `Children`. |
| `FilterPipelineCompiler` | Walks enabled nodes and returns `IReadOnlyList<IFilterDescriptor>` (groups become `FilterGroup` with internal keys `__group_{id}`; multiple root OR branches compile to one `FilterGroup` with OR). |
| `FilterPipelineSnapshot` / `FilterPipelineNodeDto` | Versioned DTO for `System.Text.Json` or other serializers. |
| `FilterPipelineSnapshotMapper` | Maps between the graph and the DTO; normalizes `JsonElement` values after deserialization. |
| `FilterPipelineInterop.FromLegacySnapshot` | Builds a `FilterPipeline` from an existing `IFilterSnapshot` (e.g. after using column filters). |

Apply to runtime state with **`FilterContext.ReplaceDescriptors(FilterPipelineCompiler.Compile(pipeline))`** or **`FilterPipelineContextExtensions.ApplyToContext(pipeline, filterContext)`**. This replaces the entire descriptor list; it does **not** merge by property name (unlike `AddOrUpdateDescriptor`).

### `IAsyncDataProvider`
Implement this interface to provide data asynchronously from any source (SQL, NoSQL, API).

```csharp
public interface IAsyncDataProvider<T>
{
    Task<PagedResult<T>> FetchDataAsync(IFilterContext context, CancellationToken cancellationToken = default);
    Task<IEnumerable<object>> FetchDistinctValuesAsync(string propertyName, string searchText = "", CancellationToken cancellationToken = default);
}
```
