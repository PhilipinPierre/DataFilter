using DataFilter.PlatformShared.ColumnFilter;
using DataFilter.PlatformShared.ViewModels;
using DataFilter.WinForms.Attach;
using System.ComponentModel;
using System.Windows.Forms;

namespace DataFilter.WinForms.Controls;

public class FilterableDataGrid : DataGridView
{
    private DataGridViewFilterHeaderEventHandler? _headerEvents;

    public FilterableDataGrid()
    {
        EnableHeadersVisualStyles = false;
        _headerEvents = new DataGridViewFilterHeaderEventHandler(this, () => ViewModel, () => InteractionSettings);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IFilterableDataGridViewModel? ViewModel { get; set; }

    public DataFilter.Core.Abstractions.IFilterContext? FilterContext => ViewModel?.Context;

    [DefaultValue(true)]
    public bool AreColumnFiltersEnabled { get; set; } = true;

    [DefaultValue(ColumnFilterTriggerMode.FilterButton)]
    public ColumnFilterTriggerMode ColumnFilterTriggerMode { get; set; } = ColumnFilterTriggerMode.FilterButton;

    private DataGridViewFilterHeaderInteractions.Settings InteractionSettings =>
        new()
        {
            AreColumnFiltersEnabled = AreColumnFiltersEnabled,
            ColumnFilterTriggerMode = ColumnFilterTriggerMode,
        };

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _headerEvents?.Dispose();
            _headerEvents = null;
        }

        base.Dispose(disposing);
    }
}
