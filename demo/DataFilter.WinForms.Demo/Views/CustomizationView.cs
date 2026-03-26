using DataFilter.WinForms.Controls;
using DataFilter.WinForms.Demo.ViewModels;
using System.Drawing;

namespace DataFilter.WinForms.Demo.Views;

public partial class CustomizationView : UserControl
{
    private readonly FilterableDataGrid _grid;
    private readonly CheckBox _themeToggle;

    public CustomizationView()
    {
        var sidePanel = new Panel { Dock = DockStyle.Left, Width = 200, Padding = new Padding(10) };
        var title = new Label { Text = "Customization", Font = new Font(Font.FontFamily, 12, FontStyle.Bold), Dock = DockStyle.Top, Height = 30 };
        _themeToggle = new CheckBox { Text = "Dark Theme", Dock = DockStyle.Top, Height = 30 };
        var helpText = new Label { Text = "Change the toggle to customize colors dynamically.", ForeColor = Color.Gray, Dock = DockStyle.Top, Height = 60 };

        sidePanel.Controls.Add(helpText);
        sidePanel.Controls.Add(_themeToggle);
        sidePanel.Controls.Add(title);

        _grid = new FilterableDataGrid
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = true
        };

        Controls.Add(_grid);
        Controls.Add(sidePanel);
    }

    public void Bind(CustomizationScenarioViewModel viewModel)
    {
        _grid.ViewModel = viewModel.GridViewModel;
        
        viewModel.GridViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(viewModel.GridViewModel.FilteredItems))
            {
                if (IsHandleCreated) BeginInvoke(() => _grid.DataSource = viewModel.GridViewModel.FilteredItems.ToList());
            }
        };
        _grid.DataSource = viewModel.GridViewModel.FilteredItems.ToList();

        // Two-way property binding
        _themeToggle.DataBindings.Add("Checked", viewModel, nameof(viewModel.IsDarkTheme), false, DataSourceUpdateMode.OnPropertyChanged);
        
        viewModel.IsDarkThemeChanged += (s, e) => ApplyTheme(viewModel.IsDarkTheme);
        ApplyTheme(viewModel.IsDarkTheme);
    }

    private void ApplyTheme(bool isDark)
    {
        if (isDark)
        {
            _grid.BackgroundColor = Color.FromArgb(30, 30, 30);
            _grid.DefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48);
            _grid.DefaultCellStyle.ForeColor = Color.White;
            _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(60, 60, 60);
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _grid.EnableHeadersVisualStyles = false;
        }
        else
        {
            _grid.BackgroundColor = SystemColors.AppWorkspace;
            _grid.DefaultCellStyle.BackColor = SystemColors.Window;
            _grid.DefaultCellStyle.ForeColor = SystemColors.WindowText;
            _grid.ColumnHeadersDefaultCellStyle.BackColor = SystemColors.Control;
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = SystemColors.WindowText;
            _grid.EnableHeadersVisualStyles = true;
        }
    }
}
