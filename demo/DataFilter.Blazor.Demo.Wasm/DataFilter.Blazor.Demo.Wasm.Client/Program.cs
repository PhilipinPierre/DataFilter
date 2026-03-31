using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using DataFilter.Demo.Shared;
using DataFilter.Blazor.Demo.Shared.State;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Shared Services
builder.Services.AddDataFilterDemoServices();

// Blazor Shared State
builder.Services.AddSingleton<DemoState>();

await builder.Build().RunAsync();
