# DataFilter.Core

The foundation of the DataFilter library, providing core abstractions, models, and filtering engines. This library is pure .NET logic and is completely independent of any UI framework.

## Overview

`DataFilter.Core` defines the contracts and base implementations for the entire system. It focuses on:
- **Abstractions**: Core interfaces like `IFilterEngine`, `IFilterDescriptor`, and `IAsyncDataProvider`.
- **Engine**: The basic filtering engine that processes descriptors and builds LINQ expressions.
- **Models**: Data structures for filter states, sorting, and pagination.
- **Enums**: Common filtering operations (Contains, Equals, GreaterThan, etc.) and sort directions.

## Key Components

### Abstractions
Provides the essential interfaces needed to implement or extend the filtering system, ensuring decoupling between logic and representation.

### Engine
Contains the default implementation of the filtering engine, responsible for converting high-level filter descriptors into executable expressions or predicates.

### Models
Defines the `FilterContext` and various `FilterDescriptor` types that hold the state of the user's filtering choices.

## Target Frameworks
- .NET 8.0
- .NET 9.0
- .NET Standard 2.0
- .NET Standard 2.1
