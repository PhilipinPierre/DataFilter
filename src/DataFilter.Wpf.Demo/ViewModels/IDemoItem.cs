using DataFilter.Wpf.Demo.Models;
using DataFilter.Wpf.ViewModels;

namespace DataFilter.Wpf.Demo.ViewModels
{
    public interface IDemoItem
    {
        public IFilterableDataGridViewModel<Employee> GridViewModel { get; }
    }
}
