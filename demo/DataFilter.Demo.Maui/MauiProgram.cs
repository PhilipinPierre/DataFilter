using Microsoft.Extensions.Logging;
using DataFilter.Demo.Shared;
using DataFilter.Maui.Demo.ViewModels;
using DataFilter.Maui.Demo.Pages;

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

            // Shared Services
            builder.Services.AddDataFilterDemoServices();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            // MAUI ViewModels
            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddSingleton<LocalFilterScenarioViewModel>();
            builder.Services.AddSingleton<AsyncFilterScenarioViewModel>();
            builder.Services.AddSingleton<HybridFilterScenarioViewModel>();
            builder.Services.AddSingleton<CustomizationScenarioViewModel>();
            builder.Services.AddSingleton<ListViewScenarioViewModel>();
            builder.Services.AddSingleton<CollectionViewScenarioViewModel>();

            // MAUI Pages
            builder.Services.AddTransient<LocalFilterPage>();
            builder.Services.AddTransient<AsyncFilterPage>();
            builder.Services.AddTransient<HybridFilterPage>();
            builder.Services.AddTransient<CustomizationPage>();
            builder.Services.AddTransient<ListViewPage>();
            builder.Services.AddTransient<CollectionViewPage>();

            return builder.Build();
        }
    }
}
