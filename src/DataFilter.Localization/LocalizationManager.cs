using System;
using System.ComponentModel;
using System.Globalization;
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

    private LocalizationManager()
    {
        // Initialize from the current UI culture.
        FilterResources.Culture = CultureInfo.CurrentUICulture;
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

