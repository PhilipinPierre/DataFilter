using DataFilter.Demo.Shared.Models;
using DataFilter.Demo.Shared.Services;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace DataFilter.Blazor.Demo.Shared.State;

public class DemoState
{
    public int RowCount { get; set; } = 1000;

    public bool IsRtl { get; set; }

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
