using DataFilter.WinUI3.Attach;
using Microsoft.UI.Xaml.Controls;

namespace DataFilter.WinUI3.Demo.Pages;

public sealed partial class AttachFilterPage : Page
{
    private ListViewFilterHeaderAdapter? _adapter;

    public ViewModels.LocalFilterScenarioViewModel ViewModel { get; } = new();

    public AttachFilterPage()
    {
        InitializeComponent();

        _adapter = ListViewFilterHeaderAdapter.Attach(
            EmployeesList,
            ViewModel.GridViewModel,
            new ListViewFilterHeaderAdapter.Column("Id", "Id", 80),
            new ListViewFilterHeaderAdapter.Column("Name", "Name", 150),
            new ListViewFilterHeaderAdapter.Column("Department", "Department", 150),
            new ListViewFilterHeaderAdapter.Column("Country", "Country", 150));
    }
}

