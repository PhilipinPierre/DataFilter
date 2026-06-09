using DataFilter.Demo.Shared.Services;
using DataFilter.PlatformShared.ViewModels;
using DataFilter.WinForms.Attach;
using DataFilter.WinForms.Demo.Services;
using DataFilter.WinForms.Demo.ViewModels;

namespace DataFilter.WinForms.Demo.Views;

public sealed class AttachFilterView : UserControl, IDemoHeaderSettingsView
{
    private readonly DataGridView _grid;
    private DataGridViewFilterAdapter? _adapter;

    public AttachFilterView()
    {
        var title = new Label
        {
            Text = "Attach demo — DataGridView (no control replacement)",
            Dock = DockStyle.Top,
            Height = 30,
            Font = new Font(Font.FontFamily, 11, FontStyle.Bold),
            Padding = new Padding(10, 5, 0, 0)
        };

        var banner = new Label
        {
            Text = "All data is loaded locally. Filtering is attached via DataGridViewFilterAdapter.",
            Dock = DockStyle.Top,
            Height = 35,
            BackColor = Color.OldLace,
            ForeColor = Color.SaddleBrown,
            Font = new Font(Font.FontFamily, 9, FontStyle.Regular),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10, 0, 0, 0)
        };

        _grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = true,
            EnableHeadersVisualStyles = false
        };

        Controls.Add(_grid);
        Controls.Add(banner);
        Controls.Add(title);
    }

    public void Bind(LocalFilterScenarioViewModel viewModel)
    {
        _adapter?.Dispose();
        _adapter = DataGridViewFilterAdapter.Attach(_grid, viewModel.GridViewModel);

        viewModel.GridViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(viewModel.GridViewModel.FilteredItems))
            {
                if (IsHandleCreated)
                    BeginInvoke(() => _grid.DataSource = viewModel.GridViewModel.FilteredItems.ToList());
            }
        };

        _grid.DataSource = viewModel.GridViewModel.FilteredItems.ToList();
        ApplyHeaderSettings(viewModel.HeaderSettings);
    }

    public void ApplyHeaderSettings(DemoHeaderSettings settings)
    {
        if (_adapter == null)
            return;

        _adapter.AreColumnFiltersEnabled = settings.AreColumnFiltersEnabled;
        _adapter.ColumnFilterTriggerMode = settings.ColumnFilterTriggerMode;
        _grid.Invalidate();
    }
}

