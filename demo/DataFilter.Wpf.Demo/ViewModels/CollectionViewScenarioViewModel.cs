using System.Collections;
using System.ComponentModel;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using DataFilter.Wpf.Adapters;
using DataFilter.Demo.Shared.Models;
using DataFilter.Demo.Shared.Services;
using DataFilter.Wpf.ViewModels;

namespace DataFilter.Wpf.Demo.ViewModels;

public sealed partial class CollectionViewScenarioViewModel : ObservableObject, IDemoItem
{
    [ObservableProperty]
    private ICollectionView _collectionView;

    [ObservableProperty]
    private IFilterableDataGridViewModel<Employee> _gridViewModel;

    public CollectionViewScenarioViewModel()
    {
        var employees = EmployeeDataGenerator.Employees;
        _collectionView = CollectionViewSource.GetDefaultView(employees);
        _gridViewModel = new CollectionViewFilterAdapter<Employee>(_collectionView);
    }

    public void Regenerate(int count)
    {
        var employees = EmployeeDataGenerator.Employees;
        CollectionView = CollectionViewSource.GetDefaultView(employees);
        GridViewModel = new CollectionViewFilterAdapter<Employee>(CollectionView);
    }
}
