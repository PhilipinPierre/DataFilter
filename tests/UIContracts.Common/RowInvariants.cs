namespace UIContracts.Common;

/// <summary>
/// Shared row/cell invariant checks for visible grid data in UI contract tests.
/// </summary>
public static class RowInvariants
{
    public static bool DepartmentEquals(string value, string expected) =>
        string.Equals(value.Trim(), expected, StringComparison.OrdinalIgnoreCase);

    public static bool NameContains(string value, string substring) =>
        value.Contains(substring, StringComparison.OrdinalIgnoreCase);

    public static bool CountryEquals(string value, string expected) =>
        string.Equals(value.Trim(), expected, StringComparison.OrdinalIgnoreCase);

    public static bool SalaryGreaterThan(string value, decimal threshold) =>
        decimal.TryParse(value.Replace(",", "").Trim(), out var n) && n > threshold;

    public static bool NameStartsWithAny(string value, params string[] prefixes)
    {
        var trimmed = value.Trim();
        return prefixes.Any(p => trimmed.StartsWith(p, StringComparison.OrdinalIgnoreCase));
    }
}
