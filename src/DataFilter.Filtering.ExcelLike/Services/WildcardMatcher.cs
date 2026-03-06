using DataFilter.Filtering.ExcelLike.Abstractions;
using System.Text.RegularExpressions;

namespace DataFilter.Filtering.ExcelLike.Services;

/// <summary>
/// Matches strings against wildcard patterns where <c>*</c> matches any sequence
/// of characters and <c>?</c> matches exactly one character.
/// The matching is case-insensitive.
/// </summary>
public sealed class WildcardMatcher : IWildcardMatcher
{
    /// <inheritdoc />
    public bool IsMatch(string input, string pattern)
    {
        if (input == null || pattern == null) return false;

        string regexPattern = BuildRegexPattern(pattern);
        return Regex.IsMatch(input, regexPattern, RegexOptions.IgnoreCase);
    }

    /// <inheritdoc />
    public bool ContainsWildcard(string input)
    {
        return input != null && (input.Contains('*') || input.Contains('?'));
    }

    private static string BuildRegexPattern(string wildcardPattern)
    {
        // Escape all regex special chars except * and ?, then replace them.
        string escaped = Regex.Escape(wildcardPattern)
            .Replace("\\*", ".*")    // * → any sequence
            .Replace("\\?", ".");    // ? → any single char

        return $"^{escaped}$";
    }
}
