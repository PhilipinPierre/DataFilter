using DataFilter.Maui.Demo.ViewModels;

namespace DataFilter.Maui
{
    public partial class AppShell : Shell
    {
        public AppShell(MainViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
