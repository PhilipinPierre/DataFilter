# DataFilter.Filtering.ExcelLike

Implements advanced, Excel-like filtering logic for the DataFilter library. This project provides a sophisticated engine that mimics the user experience of Microsoft Excel's filter menus.

## Overview

This project builds upon `DataFilter.Core` to provide:
- **Multi-value Selection**: Support for selecting multiple distinct values from a checklist.
- **Composite Filtering**: Combining manual value selection with contextual operators (e.g., "Select 'A' and 'B', BUT also any value that 'Starts with C'").
- **State Management**: Complex descriptors that track both the inclusive and exclusive states of various filter criteria.

## Key Components

### Excel Filtering Engine
A specialized implementation of the filtering logic that handles the nuances of composite Excel-like filters, translating them into cohesive LINQ expressions.

### Descriptors
Specialized filter descriptors that hold the unique state required for Excel-like filtering (e.g., lists of selected values, operator-based criteria).

## Target Frameworks
- .NET 8.0
- .NET 9.0

## Dependencies
- `DataFilter.Core`
