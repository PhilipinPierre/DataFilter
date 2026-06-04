namespace UIContracts.Common;

/// <summary>
/// Default column filter cases aligned with demo <c>Employee</c> columns.
/// </summary>
public static class ColumnMatrix
{
    public static readonly IReadOnlyList<string> DefaultPropertyNames =
    [
        "Id",
        "Name",
        "Department",
        "Country",
        "Salary",
        "HireDate"
    ];

    public static IEnumerable<object[]> DefaultPropertyNameTheoryData() =>
        DefaultPropertyNames.Select(p => new object[] { p });

    public static IReadOnlyList<ColumnFilterCase> AttachCustomFilterCases { get; } =
    [
        new("Department", "Equals", "IT", "All visible Department values equal IT"),
        new("Name", "Contains", "a", "All visible Name values contain 'a' (case-insensitive)"),
        new("Country", "Equals", "USA", "All visible Country values equal USA"),
        new("Salary", "GreaterThan", "50000", "All visible Salary values are greater than 50000"),
        new("HireDate", "Between", "2015-01-01|2020-12-31", "All visible HireDate values fall in range")
    ];

    public static IEnumerable<object[]> AttachCustomFilterTheoryData() =>
        AttachCustomFilterCases.Select(c => new object[] { c });
}
