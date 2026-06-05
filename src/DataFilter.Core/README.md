# DataFilter.Core

The foundation of the DataFilter library, providing pure filtering logic, abstractions, and standard data models.

## NuGet integration

### Install the package

```bash
dotnet add package DataFilter.Core
```

### Target frameworks

`net8.0`, `net9.0`, `netstandard2.0`, `netstandard2.1`

### Dependencies

None (UI-independent). Optional companion packages: `DataFilter.Filtering.ExcelLike` for Excel-style descriptors, `DataFilter.Expressions.Server` for LINQ translation.

### Quick start

```csharp
using DataFilter.Core.Abstractions;
using DataFilter.Core.Enums;
using DataFilter.Core.Models;
using DataFilter.Core.Pipeline;
using DataFilter.Core.Services;

// In-memory filtering
var context = new FilterContext();
context.AddOrUpdateDescriptor(new FilterDescriptor
{
    PropertyName = "Department",
    Operator = FilterOperator.Equals,
    Value = "IT"
});

var engine = new ReflectionFilterEngine();
var filtered = engine.Apply(employees, typeof(Employee), context.Descriptors);

// Filter pipeline preset (ordered criteria + multi-column sort)
var snapshot = new FilterPipelineSnapshot();
FilterPipelineSnapshotEditor.AddRootCriterion(snapshot, "IsActive", nameof(FilterOperator.Equals), true);
FilterPipelineSnapshotEditor.AddSort(snapshot, "Name", isDescending: false);

var pipeline = FilterPipelineSnapshotMapper.ToPipeline(snapshot);
context.ReplaceDescriptors(FilterPipelineCompiler.Compile(pipeline));
```

## Structure

- **Abstractions**: `IFilterEngine`, `IFilterDescriptor`, `IFilterContext`, `IAsyncDataProvider`, etc.
- **Engine**: `FilterExpressionBuilder`, `ReflectionFilterEngine`, `FilterEvaluator` (wildcard text matching).
- **Models**: `FilterSnapshot`, `FilterPipelineSnapshot`, `SortDescriptor`, `PagedResult`.
- **Pipeline** (`Pipeline/`): `FilterPipeline`, `CriterionPipelineNode`, `GroupPipelineNode` — editable graph for UI scenarios (order, named groups, toggles).
- **Services**: `FilterSnapshotBuilder`, `FilterPipelineCompiler`, `FilterPipelineSnapshotMapper`, `FilterPipelineSnapshotEditor`, `FilterPipelineInterop`, `FilterPipelineContextExtensions`.

## Key Concepts

### Wildcards in text operators (`*`, `?`)

Core text operators (`Equals`, `NotEquals`, `Contains`, `NotContains`, `StartsWith`, `EndsWith`) support wildcard patterns:

- `*` matches any sequence of characters
- `?` matches a single character

This behavior is implemented in `FilterEvaluator` and `FilterExpressionBuilder`, so it applies consistently to in-memory filtering and compiled expressions. Persisted presets that store a text operator + pattern replay correctly without UI-specific logic.

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
| `FilterPipelineSnapshot` / `FilterPipelineNodeDto` | Versioned DTO for presets. **`Nodes`** and **`SortEntries`** are mutable lists — edit in memory and apply without JSON. |
| `FilterPipelineSnapshotMapper` | Maps between the graph and the DTO; normalizes `JsonElement` values after deserialization. |
| `FilterPipelineSnapshotEditor` | Client-side CRUD on snapshot criteria and sort (`AddRootCriterion`, `AddSort`, `RemoveNode`, `MoveSort`, `Clone`, …). |
| `FilterPipelineInterop.FromLegacySnapshot` | Builds a `FilterPipeline` from an existing `IFilterSnapshot` (e.g. after using column filters). |

Apply to runtime state with **`FilterContext.ReplaceDescriptors(FilterPipelineCompiler.Compile(pipeline))`** or **`FilterPipelineContextExtensions.ApplyToContext(pipeline, filterContext)`**. This replaces the entire descriptor list; it does **not** merge by property name (unlike `AddOrUpdateDescriptor`).

```csharp
// Edit snapshot in memory, then compile and apply
var snapshot = FilterPipelineSnapshotEditor.Clone(existing);
FilterPipelineSnapshotEditor.AddRootCriterion(snapshot, "Name", nameof(FilterOperator.StartsWith), "A");
var pipeline = FilterPipelineSnapshotMapper.ToPipeline(snapshot);
context.ReplaceDescriptors(FilterPipelineCompiler.Compile(pipeline));
```

### `IAsyncDataProvider`

Implement this interface to provide data asynchronously from any source (SQL, NoSQL, API).

```csharp
public interface IAsyncDataProvider<T>
{
    Task<PagedResult<T>> FetchDataAsync(IFilterContext context, CancellationToken cancellationToken = default);
    Task<IEnumerable<object>> FetchDistinctValuesAsync(string propertyName, string searchText = "", CancellationToken cancellationToken = default);
}
```
