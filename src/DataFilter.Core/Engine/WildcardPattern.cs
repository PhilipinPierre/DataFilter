using System.Text.RegularExpressions;

namespace DataFilter.Core.Engine;

/// <summary>
/// Wildcard pattern matching where <c>*</c> matches any sequence and <c>?</c> matches one character.
/// Matching is case-insensitive.
/// </summary>
public static class WildcardPattern
{
    public static bool ContainsWildcard(string? pattern)
        => !string.IsNullOrEmpty(pattern) && (pattern.Contains('*') || pattern.Contains('?'));

    public static bool IsMatch(string? input, string? pattern)
    {
        if (input == null || pattern == null) return false;

        string escaped = Regex.Escape(pattern)
            .Replace("\\*", ".*")
            .Replace("\\?", ".");

        return Regex.IsMatch(input, $"^{escaped}$", RegexOptions.IgnoreCase);
    }
}

