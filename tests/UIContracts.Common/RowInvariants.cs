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
        var firstToken = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? trimmed;
        return prefixes.Any(p => firstToken.StartsWith(p, StringComparison.OrdinalIgnoreCase));
    }

    public static bool SalaryBetween(string value, decimal min, decimal max)
    {
        if (!decimal.TryParse(value.Replace(",", "").Trim(), out var n))
            return false;
        return n >= min && n <= max;
    }

    public static bool HireDateBetweenTicks(string dataValueTicks, long minTicks, long maxTicks)
    {
        if (!long.TryParse(dataValueTicks.Trim(), out var ticks))
            return false;
        return ticks >= minTicks && ticks <= maxTicks;
    }
}
