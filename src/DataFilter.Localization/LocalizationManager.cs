using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using DataFilter.Localization.Resources;

namespace DataFilter.Localization;

/// <summary>
/// Central runtime localization entry point.
/// </summary>
public sealed class LocalizationManager : INotifyPropertyChanged
{
    private static readonly Lazy<LocalizationManager> _instance = new(() => new LocalizationManager());
    public static LocalizationManager Instance => _instance.Value;

    private CultureInfo? _cultureOverride;
    private int _version;
    private static IReadOnlyList<CultureInfo>? _availableCultures;

    private LocalizationManager()
    {
        // Initialize from the current UI culture.
        FilterResources.Culture = CultureInfo.CurrentUICulture;
    }

    /// <summary>
    /// Returns the list of cultures that have localized resources available in <c>DataFilter.Localization</c>.
    /// Includes <see cref="CultureInfo.InvariantCulture"/> to represent the fallback (neutral) resources.
    /// </summary>
    public static IReadOnlyList<CultureInfo> GetAvailableCultures()
    {
        if (_availableCultures is not null)
            return _availableCultures;

        var assembly = typeof(FilterResources).Assembly;
        var cultures = new List<CultureInfo> { CultureInfo.InvariantCulture };

        // Scan all known cultures and keep the ones that have a satellite assembly.
        // This matches what's actually deployed with the app (e.g., culture folders next to the app).
        foreach (var culture in CultureInfo.GetCultures(CultureTypes.NeutralCultures | CultureTypes.SpecificCultures))
        {
            if (culture.Equals(CultureInfo.InvariantCulture))
                continue;

            try
            {
                _ = assembly.GetSatelliteAssembly(culture);
                cultures.Add(culture);
            }
            catch (FileNotFoundException)
            {
                // No satellite resources for this culture.
            }
            catch (CultureNotFoundException)
            {
                // Ignore invalid cultures on some platforms.
            }
        }

        _availableCultures = cultures
            .Distinct()
            .OrderBy(c => c == CultureInfo.InvariantCulture ? string.Empty : c.NativeName)
            .ToArray();

        return _availableCultures;
    }

    /// <summary>
    /// A monotonically increasing version number that changes whenever the effective culture changes.
    /// Useful for UI frameworks that need a binding "tick" to re-run converters/templates.
    /// </summary>
    public int Version
    {
        get => _version;
        private set
        {
            if (_version == value) return;
            _version = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the effective culture used to resolve resources.
    /// </summary>
    public CultureInfo Culture => _cultureOverride ?? FilterResources.Culture ?? CultureInfo.CurrentUICulture;

    /// <summary>
    /// Sets a culture override for resource lookup. Pass null to revert to <see cref="CultureInfo.CurrentUICulture"/>.
    /// </summary>
    public void SetCulture(CultureInfo? culture)
    {
        _cultureOverride = culture;

        var effective = _cultureOverride ?? CultureInfo.CurrentUICulture;
        FilterResources.Culture = effective;

        // Some UI stacks rely on these too.
        CultureInfo.CurrentUICulture = effective;
        CultureInfo.CurrentCulture = effective;

        Version++;
        OnPropertyChanged(nameof(Culture));
        OnPropertyChanged("Item[]");

        CultureChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Allows indexed bindings, e.g. WPF: Path=[Ok].
    /// </summary>
    public string this[string key] => FilterResources.ResourceManager.GetString(key, Culture) ?? key;

    public event EventHandler? CultureChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

