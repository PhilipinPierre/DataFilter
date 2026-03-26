using Microsoft.Extensions.Logging;

namespace DataFilter.Maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton<DataFilter.Maui.Demo.Services.IMockEmployeeApiService, DataFilter.Maui.Demo.Services.MockEmployeeApiService>();
            builder.Services.AddSingleton<DataFilter.Maui.Demo.ViewModels.MainViewModel>();

            builder.Services.AddTransient<DataFilter.Maui.Demo.ViewModels.LocalFilterScenarioViewModel>();
            builder.Services.AddTransient<DataFilter.Maui.Demo.ViewModels.AsyncFilterScenarioViewModel>();
            builder.Services.AddTransient<DataFilter.Maui.Demo.ViewModels.HybridFilterScenarioViewModel>();
            builder.Services.AddTransient<DataFilter.Maui.Demo.ViewModels.CustomizationScenarioViewModel>();
            builder.Services.AddTransient<DataFilter.Maui.Demo.ViewModels.ListViewScenarioViewModel>();
            builder.Services.AddTransient<DataFilter.Maui.Demo.ViewModels.CollectionViewScenarioViewModel>();

            builder.Services.AddTransient<DataFilter.Maui.Demo.Pages.LocalFilterPage>();
            builder.Services.AddTransient<DataFilter.Maui.Demo.Pages.AsyncFilterPage>();
            builder.Services.AddTransient<DataFilter.Maui.Demo.Pages.HybridFilterPage>();
            builder.Services.AddTransient<DataFilter.Maui.Demo.Pages.CustomizationPage>();
            builder.Services.AddTransient<DataFilter.Maui.Demo.Pages.ListViewPage>();
            builder.Services.AddTransient<DataFilter.Maui.Demo.Pages.CollectionViewPage>();

            return builder.Build();
        }
    }
}
