using System.Globalization;
using System.Resources;

namespace DataFilter.Localization.Resources;

/// <summary>
/// Thin wrapper over the RESX ResourceManager.
/// </summary>
public static class FilterResources
{
    private static readonly ResourceManager _resourceManager =
        new("DataFilter.Localization.Resources.FilterResources", typeof(FilterResources).Assembly);

    public static ResourceManager ResourceManager => _resourceManager;

    /// <summary>
    /// Explicit culture to use for lookups. When null, callers should use <see cref="CultureInfo.CurrentUICulture"/>.
    /// </summary>
    public static CultureInfo? Culture { get; set; }
}

