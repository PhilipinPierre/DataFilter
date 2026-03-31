using DataFilter.Demo.Shared.Services;
using DataFilter.WinForms.Demo.ViewModels;
using DataFilter.WinForms.Demo.Views;

namespace DataFilter.WinForms.Demo;

public sealed class MainForm : Form
{
    private readonly NumericUpDown _rowCountInput;
    private readonly Button _regenerateBtn;
    private readonly Button _clearFiltersBtn;
    private readonly TabControl _tabControl;

    // ViewModels
    private readonly LocalFilterScenarioViewModel _localVm = new();
    private readonly AsyncFilterScenarioViewModel _asyncVm = new();
    private readonly HybridFilterScenarioViewModel _hybridVm = new();
    private readonly CustomizationScenarioViewModel _customizationVm = new();
    private readonly ListViewScenarioViewModel _listViewVm = new();
    private readonly CollectionViewScenarioViewModel _collectionViewVm = new();

    // Views
    private readonly LocalFilterView _localView = new();
    private readonly AsyncFilterView _asyncView = new();
    private readonly HybridFilterView _hybridView = new();
    private readonly CustomizationView _customizationView = new();
    private readonly ListViewFilterView _listViewView = new();
    private readonly CollectionViewFilterView _collectionViewView = new();

    public MainForm()
    {
        Text = "DataFilter WinForms Demo";
        Width = 1000;
        Height = 600;

        // --- Shell Controls ---
        var topPanel = new Panel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(10, 5, 10, 5) };
        var flowLayout = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        topPanel.Controls.Add(flowLayout);

        var lblRowCount = new Label { Text = "Row Count:", AutoSize = true, Margin = new Padding(0, 7, 5, 0) };
        _rowCountInput = new NumericUpDown { Minimum = 1, Maximum = 1000000, Value = 1000, Width = 100, Margin = new Padding(0, 3, 10, 0) };
        _regenerateBtn = new Button { Text = "Regenerate Data", AutoSize = true, Margin = new Padding(0, 0, 10, 0) };
        _clearFiltersBtn = new Button { Text = "Clear filters", AutoSize = true };

        flowLayout.Controls.AddRange(new Control[] { lblRowCount, _rowCountInput, _regenerateBtn, _clearFiltersBtn });

        // --- Tabs ---
        _tabControl = new TabControl { Dock = DockStyle.Fill };

        AddTab("Local Filtering", _localView);
        AddTab("Async Filtering", _asyncView);
        AddTab("Hybrid Filtering", _hybridView);
        AddTab("Customization", _customizationView);
        AddTab("ListView Example", _listViewView);
        AddTab("CollectionView Example", _collectionViewView);

        Controls.Add(_tabControl);
        Controls.Add(topPanel);

        // --- Bindings ---
        _localView.Bind(_localVm);
        _asyncView.Bind(_asyncVm);
        _hybridView.Bind(_hybridVm);
        _customizationView.Bind(_customizationVm);
        _listViewView.Bind(_listViewVm);
        _collectionViewView.Bind(_collectionViewVm);

        // --- Events ---
        _regenerateBtn.Click += (s, e) => Regenerate();
        _clearFiltersBtn.Click += async (s, e) => await ClearFiltersAsync();
    }

    private void AddTab(string title, Control content)
    {
        var page = new TabPage(title);
        content.Dock = DockStyle.Fill;
        page.Controls.Add(content);
        _tabControl.TabPages.Add(page);
    }

    private void Regenerate()
    {
        int count = (int)_rowCountInput.Value;
        EmployeeDataGenerator.Regenerate(count);

        _localVm.Regenerate(count);
        _asyncVm.Regenerate(count);
        _hybridVm.Regenerate(count);
        _customizationVm.Regenerate(count);
        _listViewVm.Regenerate(count);
        _collectionViewVm.Regenerate(count);
    }

    private async Task ClearFiltersAsync()
    {
        // Clear all filters across all scenarios as per DemoFeatures.md
        _localVm.GridViewModel.Context.ClearDescriptors();
        _asyncVm.GridViewModel.Context.ClearDescriptors();
        _hybridVm.GridViewModel.Context.ClearDescriptors();
        _customizationVm.GridViewModel.Context.ClearDescriptors();
        _listViewVm.GridViewModel.Context.ClearDescriptors();
        _collectionViewVm.GridViewModel.Context.ClearDescriptors();

        await Task.WhenAll(
            _localVm.GridViewModel.RefreshDataAsync(),
            _asyncVm.GridViewModel.RefreshDataAsync(),
            _hybridVm.GridViewModel.RefreshDataAsync(),
            _customizationVm.GridViewModel.RefreshDataAsync(),
            _listViewVm.GridViewModel.RefreshDataAsync(),
            _collectionViewVm.GridViewModel.RefreshDataAsync()
        );
    }
}
