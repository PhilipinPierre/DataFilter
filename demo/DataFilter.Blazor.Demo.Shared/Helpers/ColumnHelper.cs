using DataFilter.Demo.Shared.Models;
using DataFilter.Blazor.Components;
using System;
using System.Collections.Generic;

namespace DataFilter.Blazor.Demo.Shared.Helpers;

public static class ColumnHelper
{
    public static List<DataFilterGrid<Employee>.ColumnDefinition<Employee>> GetDefaultColumns()
    {
        return new List<DataFilterGrid<Employee>.ColumnDefinition<Employee>>
        {
            new() { Id = "1", Title = "ID", PropertyName = "Id", Selector = e => e.Id, PropertyType = typeof(int) },
            new() { Id = "2", Title = "Name", PropertyName = "Name", Selector = e => e.Name, PropertyType = typeof(string) },
            new() { Id = "3", Title = "Department", PropertyName = "Department", Selector = e => e.Department, PropertyType = typeof(string) },
            new() { Id = "4", Title = "Country", PropertyName = "Country", Selector = e => e.Country, PropertyType = typeof(string) },
            new() { Id = "5", Title = "Salary", PropertyName = "Salary", Selector = e => e.Salary, PropertyType = typeof(decimal) },
            new() { Id = "6", Title = "Hire Date", PropertyName = "HireDate", Selector = e => e.HireDate, PropertyType = typeof(DateTime) }
        };
    }

    public static List<DataFilterGrid<Employee>.ColumnDefinition<Employee>> GetListViewColumns()
    {
        return new List<DataFilterGrid<Employee>.ColumnDefinition<Employee>>
        {
            new() { Id = "1", Title = "ID", PropertyName = "Id", Selector = e => e.Id, PropertyType = typeof(int) },
            new() { Id = "2", Title = "Full Name", PropertyName = "Name", Selector = e => e.Name, PropertyType = typeof(string) },
            new() { Id = "3", Title = "Dept", PropertyName = "Department", Selector = e => e.Department, PropertyType = typeof(string) },
            new() { Id = "4", Title = "Region", PropertyName = "Country", Selector = e => e.Country, PropertyType = typeof(string) },
            new() { Id = "6", Title = "Joined", PropertyName = "HireDate", Selector = e => e.HireDate, PropertyType = typeof(DateTime) }
        };
    }
}
