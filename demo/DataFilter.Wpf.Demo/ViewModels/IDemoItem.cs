using DataFilter.Demo.Shared.Models;
using DataFilter.Wpf.ViewModels;

namespace DataFilter.Wpf.Demo.ViewModels
{
    public interface IDemoItem
    {
        public IFilterableDataGridViewModel<Employee> GridViewModel { get; }
    }
}
