# DataFilter.Expressions.Server

Extension for applying DataFilter criteria to `IQueryable<T>` with compiled LINQ expression trees (EF Core, Dapper, etc.).

## NuGet integration

### Install the package

```bash
dotnet add package DataFilter.Expressions.Server
```

### Target frameworks

`net8.0`, `net9.0`

### Dependencies

- `DataFilter.Core`

Use alongside your UI stack or API layer; no UI references.

### Quick start

```csharp
using DataFilter.Core.Enums;
using DataFilter.Core.Models;
using DataFilter.Core.Pipeline;
using DataFilter.Core.Services;
using DataFilter.Expressions.Server.Services;

// Build or restore filter context from UI snapshot
var context = new FilterContext();
context.AddOrUpdateDescriptor(new FilterDescriptor("Department", FilterOperator.Equals, "IT"));

// Apply filters + multi-column sort to IQueryable
var engine = new QueryableFilterEngine<Employee>();
var query = engine.Apply(dbContext.Employees, context);

// From a filter pipeline preset
var pipeline = FilterPipelineSnapshotMapper.ToPipeline(pipelineSnapshot);
context.ReplaceDescriptors(FilterPipelineCompiler.Compile(pipeline));
var filtered = engine.Apply(dbContext.Employees, context).ToList();
```

## Features

- **`QueryableFilterEngine<T>`** — applies `IFilterContext.Descriptors` and **`SortDescriptors`** to an `IQueryable<T>`.
- Uses **`FilterExpressionBuilder`** from Core (logical groups, wildcards in text patterns, all standard operators).
- **`TopNFilter`**, **`AverageFilter`** — optional aggregate helpers for advanced scenarios.

## Usage

```csharp
using DataFilter.Core.Models;
using DataFilter.Core.Services;
using DataFilter.Expressions.Server.Services;

// 1. Receive snapshot from UI (WPF, Blazor, or API body)
FilterSnapshot snapshot = DeserializeFromClient();

// 2. Restore into a FilterContext
var context = new FilterContext();
new FilterSnapshotBuilder().RestoreSnapshot(context, snapshot);

// 3. Apply to IQueryable (filter + sort)
var engine = new QueryableFilterEngine<MyDataModel>();
var filteredData = engine.Apply(dbContext.MyTable, context).ToList();
```

For pipeline presets, map **`FilterPipelineSnapshot`** → **`FilterPipeline`**, compile with **`FilterPipelineCompiler`**, then call **`context.ReplaceDescriptors(...)`** before **`Apply`**.
