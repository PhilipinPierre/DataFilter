using System.ComponentModel;
using DataFilter.Demo.Shared.Services;
using DataFilter.Maui.Attach;
using DataFilter.Maui.Demo.ViewModels;

namespace DataFilter.Maui.Demo.Pages;

public partial class AttachFilterPage : ContentPage
{
    private readonly ListViewFilterHeaderAdapter _adapter;

    public AttachFilterPage(LocalFilterScenarioViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        _adapter = ListViewFilterHeaderAdapter.Attach(
            hostPage: this,
            listView: EmployeesList,
            viewModel: viewModel.GridViewModel,
            new ListViewFilterHeaderAdapter.Column("Id", "Id", new GridLength(80)),
            new ListViewFilterHeaderAdapter.Column("Name", "Name", new GridLength(150)),
            new ListViewFilterHeaderAdapter.Column("Dept", "Department", new GridLength(120)),
            new ListViewFilterHeaderAdapter.Column("Country", "Country", new GridLength(120)));

        viewModel.HeaderSettings.PropertyChanged += OnHeaderSettingsChanged;
        ApplyHeaderSettings(viewModel.HeaderSettings);
    }

    private void OnHeaderSettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not DemoHeaderSettings settings)
            return;

        if (e.PropertyName is nameof(DemoHeaderSettings.AreColumnFiltersEnabled)
            or nameof(DemoHeaderSettings.ColumnFilterTriggerMode))
        {
            ApplyHeaderSettings(settings);
        }
    }

    private void ApplyHeaderSettings(DemoHeaderSettings settings)
    {
        _adapter.ApplyHeaderSettings(
            settings.AreColumnFiltersEnabled,
            settings.ColumnFilterTriggerMode);
    }
}
