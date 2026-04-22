using DataFilter.Blazor.Demo.Wasm.Client.Pages;
using DataFilter.Blazor.Demo.Wasm.Components;
using DataFilter.Demo.Shared;
using DataFilter.Blazor.Demo.Shared.State;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Shared Services
builder.Services.AddDataFilterDemoServices();

// Blazor Shared State
builder.Services.AddScoped<DemoState>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found");
app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseStaticFiles();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(DataFilter.Blazor.Demo.Wasm.Client._Imports).Assembly,
        typeof(DataFilter.Blazor.Demo.Shared.Pages.DemoPage).Assembly,
        typeof(DataFilter.Blazor.Components.FilterPopup).Assembly,
        typeof(DataFilter.Blazor.PopupHost.ColumnFilterButton).Assembly);

app.Run();
