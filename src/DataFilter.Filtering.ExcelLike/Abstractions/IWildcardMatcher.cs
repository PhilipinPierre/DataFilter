namespace DataFilter.Filtering.ExcelLike.Abstractions;

/// <summary>
/// Matches a string value against a wildcard pattern.
/// Supported wildcards:
/// <list type="bullet">
///   <item><c>*</c> — replaces any sequence of characters (including empty).</item>
///   <item><c>?</c> — replaces exactly one character.</item>
/// </list>
/// </summary>
public interface IWildcardMatcher
{
    /// <summary>
    /// Returns <c>true</c> if <paramref name="input"/> matches the <paramref name="pattern"/>.
    /// Matching is case-insensitive.
    /// </summary>
    /// <param name="input">The string to test.</param>
    /// <param name="pattern">The wildcard pattern (may contain <c>*</c> and <c>?</c>).</param>
    bool IsMatch(string input, string pattern);

    /// <summary>
    /// Returns <c>true</c> if <paramref name="input"/> contains a wildcard character (<c>*</c> or <c>?</c>).
    /// </summary>
    bool ContainsWildcard(string input);
}
