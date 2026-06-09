namespace DataFilter.Demo.Shared.Services;

/// <summary>
/// Demo scenario view models expose the shared shell header settings instance.
/// </summary>
public interface IDemoHeaderSettingsHost
{
    DemoHeaderSettings HeaderSettings { get; }
}
