using System.ComponentModel;
using System.Runtime.CompilerServices;
using DataFilter.PlatformShared.ColumnFilter;

namespace DataFilter.Demo.Shared.Services;

/// <summary>
/// Shared demo shell settings for column filter header chrome (all UI stacks).
/// </summary>
public sealed class DemoHeaderSettings : INotifyPropertyChanged
{
    public static IReadOnlyList<ColumnFilterTriggerMode> GridTriggerModes { get; } =
        Enum.GetValues<ColumnFilterTriggerMode>()
            .Where(m => m != ColumnFilterTriggerMode.Inherit)
            .ToArray();

    private bool _areColumnFiltersEnabled = true;
    private ColumnFilterTriggerMode _columnFilterTriggerMode = ColumnFilterTriggerMode.FilterButton;

    public bool AreColumnFiltersEnabled
    {
        get => _areColumnFiltersEnabled;
        set
        {
            if (_areColumnFiltersEnabled == value)
                return;
            _areColumnFiltersEnabled = value;
            OnPropertyChanged();
        }
    }

    public ColumnFilterTriggerMode ColumnFilterTriggerMode
    {
        get => _columnFilterTriggerMode;
        set
        {
            if (_columnFilterTriggerMode == value)
                return;
            _columnFilterTriggerMode = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
