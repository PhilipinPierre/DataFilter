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
            new ListViewFilterHeaderAdapter.Column("Dept", "Department", new GridLength(150)));
    }
}

