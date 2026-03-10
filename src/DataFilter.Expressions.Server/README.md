# DataFilter.Expressions.Server

Provides server-side expression generation and `IQueryable` filtering capabilities for the DataFilter library. This project is designed to bridge the gap between client-side filter descriptors and server-side data providers like EF Core.

## Overview

`DataFilter.Expressions.Server` extends the core filtering logic to work efficiently with data sources that support LINQ expressions.

## Key Components

### Queryable Engine
Implementations that allow applying `IFilterDescriptor` directly to an `IQueryable<T>` source, enabling efficient database-level filtering.

### Specialized Filters
- **Top N Filter**: Filters results to only show the top N items based on specified criteria.
- **Average Filter**: Filtering logic based on average values of fields.

## Target Frameworks
- .NET 8.0
- .NET 9.0

## Dependencies
- `DataFilter.Core`
