using DataFilter.Maui.Demo.ViewModels;

namespace DataFilter.Maui
{
    public partial class App : Application
    {
        public static MainViewModel? MainViewModel { get; private set; }

        public App(MainViewModel viewModel)
        {
            InitializeComponent();
            MainViewModel = viewModel;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell(MainViewModel!));
        }
    }
}
