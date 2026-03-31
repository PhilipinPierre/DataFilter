using DataFilter.Blazor.Demo.Shared.State;
using DataFilter.Demo.Shared;
using Microsoft.Extensions.Logging;

namespace DataFilter.Blazor.Demo.Hybrid;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		try
		{
			var builder = MauiApp.CreateBuilder();
			builder
				.UseMauiApp<App>()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				});

			builder.Services.AddMauiBlazorWebView();

			// Shared Demo Services
			builder.Services.AddDataFilterDemoServices();

			// Blazor Shared State
			builder.Services.AddScoped<DemoState>();

#if DEBUG
			builder.Services.AddBlazorWebViewDeveloperTools();
			builder.Logging.AddDebug();
#endif

			return builder.Build();
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine(ex);
			try
			{
				string logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "startup_crash.txt");
				System.IO.File.WriteAllText(logPath, ex.ToString());
			}
			catch { }
			throw;
		}
	}
}
