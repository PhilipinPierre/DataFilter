using DataFilter.Demo.Shared.Services;
using DataFilter.WinForms.Demo.ViewModels;
using DataFilter.WinForms.Demo.Views;
using DataFilter.Localization;
using System.Globalization;

namespace DataFilter.WinForms.Demo;

public sealed class MainForm : Form
{
    private readonly NumericUpDown _rowCountInput;
    private readonly Button _regenerateBtn;
    private readonly Button _clearFiltersBtn;
    private readonly ComboBox _languageCombo;
    private readonly TabControl _tabControl;

    private readonly LocalFilterScenarioViewModel _localVm;
    private readonly AsyncFilterScenarioViewModel _asyncVm;
    private readonly HybridFilterScenarioViewModel _hybridVm;
    private readonly CustomizationScenarioViewModel _customizationVm;
    private readonly ListViewScenarioViewModel _listViewVm;
    private readonly CollectionViewScenarioViewModel _collectionViewVm;

    // Views
    private readonly LocalFilterView _localView;
    private readonly AttachFilterView _attachView;
    private readonly AsyncFilterView _asyncView;
    private readonly HybridFilterView _hybridView;
    private readonly CustomizationView _customizationView;
    private readonly ListViewFilterView _listViewView;
    private readonly CollectionViewFilterView _collectionViewView;

    public MainForm(
        LocalFilterScenarioViewModel localVm,
        AsyncFilterScenarioViewModel asyncVm,
        HybridFilterScenarioViewModel hybridVm,
        CustomizationScenarioViewModel customizationVm,
        ListViewScenarioViewModel listViewVm,
        CollectionViewScenarioViewModel collectionViewVm,
        LocalFilterView localView,
        AttachFilterView attachView,
        AsyncFilterView asyncView,
        HybridFilterView hybridView,
        CustomizationView customizationView,
        ListViewFilterView listViewView,
        CollectionViewFilterView collectionViewView)
    {
        _localVm = localVm;
        _asyncVm = asyncVm;
        _hybridVm = hybridVm;
        _customizationVm = customizationVm;
        _listViewVm = listViewVm;
        _collectionViewVm = collectionViewVm;

        _localView = localView;
        _attachView = attachView;
        _asyncView = asyncView;
        _hybridView = hybridView;
        _customizationView = customizationView;
        _listViewView = listViewView;
        _collectionViewView = collectionViewView;
        Text = "DataFilter WinForms Demo";
        Width = 1000;
        Height = 600;

        // --- Shell Controls ---
        var topPanel = new Panel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(10, 5, 10, 5) };
        var flowLayout = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        topPanel.Controls.Add(flowLayout);

        var lblRowCount = new Label { Text = "Row Count:", AutoSize = true, Margin = new Padding(0, 7, 5, 0) };
        _rowCountInput = new NumericUpDown { Minimum = 1, Maximum = 1000000000, Value = 1000, Width = 100, Margin = new Padding(0, 3, 10, 0) };
        _regenerateBtn = new Button { Text = "Regenerate Data", AutoSize = true, Margin = new Padding(0, 0, 10, 0) };
        _clearFiltersBtn = new Button { Text = "Clear filters", AutoSize = true };

        var lblLanguage = new Label { Text = "Language:", AutoSize = true, Margin = new Padding(15, 7, 5, 0) };
        _languageCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220, Margin = new Padding(0, 3, 10, 0) };

        var languageOptions = LocalizationManager.GetAvailableCultures()
            .Select(c => new LanguageOption(c))
            .ToList();
        _languageCombo.DataSource = languageOptions;
        _languageCombo.DisplayMember = nameof(LanguageOption.Label);
        _languageCombo.ValueMember = nameof(LanguageOption.Culture);
        _languageCombo.SelectedValue = LocalizationManager.Instance.Culture;

        flowLayout.Controls.AddRange(new Control[] { lblRowCount, _rowCountInput, _regenerateBtn, _clearFiltersBtn, lblLanguage, _languageCombo });

        // --- Tabs ---
        _tabControl = new TabControl { Dock = DockStyle.Fill };

        AddTab("Local Filtering", _localView);
        AddTab("Attach (DataGridView)", _attachView);
        AddTab("Async Filtering", _asyncView);
        AddTab("Hybrid Filtering", _hybridView);
        AddTab("Customization", _customizationView);
        AddTab("ListView Example", _listViewView);
        AddTab("CollectionView Example", _collectionViewView);

        Controls.Add(_tabControl);
        Controls.Add(topPanel);

        // --- Bindings ---
        _localView.Bind(_localVm);
        _attachView.Bind(_localVm);
        _asyncView.Bind(_asyncVm);
        _hybridView.Bind(_hybridVm);
        _customizationView.Bind(_customizationVm);
        _listViewView.Bind(_listViewVm);
        _collectionViewView.Bind(_collectionViewVm);

        // --- Events ---
        _regenerateBtn.Click += (s, e) => Regenerate();
        _clearFiltersBtn.Click += async (s, e) => await ClearFiltersAsync();
        _languageCombo.SelectedValueChanged += (_, __) =>
        {
            if (_languageCombo.SelectedValue is CultureInfo culture)
                LocalizationManager.Instance.SetCulture(culture);
        };
    }

    private sealed class LanguageOption
    {
        public LanguageOption(CultureInfo culture)
        {
            Culture = culture;
            Label = culture == CultureInfo.InvariantCulture ? "Default" : culture.NativeName;
        }

        public CultureInfo Culture { get; }
        public string Label { get; }
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
