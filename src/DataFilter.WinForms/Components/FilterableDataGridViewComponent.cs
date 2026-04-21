using DataFilter.PlatformShared.ViewModels;
using DataFilter.WinForms.Attach;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Windows.Forms;

namespace DataFilter.WinForms.Components;

/// <summary>
/// Designer-friendly component that attaches DataFilter column filtering to existing <see cref="DataGridView"/> instances.
/// </summary>
[ProvideProperty("ViewModel", typeof(DataGridView))]
[ProvideProperty("AttachEnabled", typeof(DataGridView))]
public sealed class FilterableDataGridViewComponent : Component, IExtenderProvider
{
    private readonly ConcurrentDictionary<DataGridView, DataGridViewFilterAdapter> _adapters = new();
    private readonly ConcurrentDictionary<DataGridView, IFilterableDataGridViewModel?> _viewModels = new();
    private readonly ConcurrentDictionary<DataGridView, bool> _enabled = new();

    public bool CanExtend(object extendee) => extendee is DataGridView;

    [Category("DataFilter")]
    [DefaultValue(null)]
    public IFilterableDataGridViewModel? GetViewModel(DataGridView grid)
        => _viewModels.TryGetValue(grid, out var vm) ? vm : null;

    public void SetViewModel(DataGridView grid, IFilterableDataGridViewModel? value)
    {
        _viewModels[grid] = value;
        EnsureAdapter(grid);
    }

    [Category("DataFilter")]
    [DefaultValue(true)]
    public bool GetAttachEnabled(DataGridView grid)
        => _enabled.TryGetValue(grid, out var v) ? v : true;

    public void SetAttachEnabled(DataGridView grid, bool value)
    {
        _enabled[grid] = value;
        EnsureAdapter(grid);
    }

    private void EnsureAdapter(DataGridView grid)
    {
        if (grid == null) return;

        bool enabled = GetAttachEnabled(grid);
        var vm = GetViewModel(grid);

        if (!enabled || vm == null)
        {
            if (_adapters.TryRemove(grid, out var existing))
                existing.Dispose();
            return;
        }

        if (_adapters.ContainsKey(grid))
            return;

        var adapter = new DataGridViewFilterAdapter(grid, vm);
        _adapters[grid] = adapter;

        // Auto-detach when the grid is disposed.
        grid.Disposed += (_, _) =>
        {
            if (_adapters.TryRemove(grid, out var a))
                a.Dispose();
            _viewModels.TryRemove(grid, out _);
            _enabled.TryRemove(grid, out _);
        };
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var kvp in _adapters)
            {
                kvp.Value.Dispose();
            }
            _adapters.Clear();
            _viewModels.Clear();
            _enabled.Clear();
        }

        base.Dispose(disposing);
    }
}

