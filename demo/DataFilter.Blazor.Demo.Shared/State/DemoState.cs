using DataFilter.Demo.Shared.Models;
using DataFilter.Demo.Shared.Services;
using DataFilter.PlatformShared.ColumnFilter;
using Microsoft.AspNetCore.Components;

namespace DataFilter.Blazor.Demo.Shared.State;

public class DemoState
{
    private readonly DemoHeaderSettings _headerSettings;

    public DemoState(DemoHeaderSettings headerSettings)
    {
        _headerSettings = headerSettings;
        _headerSettings.PropertyChanged += (_, _) => NotifyStateChanged();
    }

    public int RowCount { get; set; } = 1000;

    public bool IsRtl { get; set; }

    public bool AreColumnFiltersEnabled
    {
        get => _headerSettings.AreColumnFiltersEnabled;
        set => _headerSettings.AreColumnFiltersEnabled = value;
    }

    public ColumnFilterTriggerMode ColumnFilterTriggerMode
    {
        get => _headerSettings.ColumnFilterTriggerMode;
        set => _headerSettings.ColumnFilterTriggerMode = value;
    }

    public IReadOnlyList<ColumnFilterTriggerMode> ColumnFilterTriggerModes => DemoHeaderSettings.GridTriggerModes;

    public void SetDirection(bool isRtl)
    {
        if (IsRtl == isRtl) return;
        IsRtl = isRtl;
        NotifyStateChanged();
    }

    public void Regenerate()
    {
        EmployeeDataGenerator.Regenerate(RowCount);
        NotifyStateChanged();
    }

    public event Action? OnChange;

    private void NotifyStateChanged() => OnChange?.Invoke();
}
