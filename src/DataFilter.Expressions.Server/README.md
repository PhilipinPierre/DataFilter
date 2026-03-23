# DataFilter.Expressions.Server

Lightweight extension for converting `DataFilter` snapshots into strongly-typed LINQ Expressions.

## Features
- Translates `FilterSnapshot` to `Expression<Func<T, bool>>`.
- Supports complex logical groups (AND / OR).
- Full compatibility with Entity Framework and other LINQ-to-SQL providers.
- Handles all standard operators (Contains, GreaterThan, etc.).

## Usage

```csharp
using DataFilter.Expressions.Server;

// 1. Receive snapshot from UI (WPF or Blazor)
FilterSnapshot mySnapshot = ...;

// 2. Convert to Expression
var predicate = mySnapshot.ToExpression<MyDataModel>();

// 3. Apply to IQueryable
var filteredData = dbContext.MyTable.Where(predicate).ToList();
```
