using Blazored.LocalStorage;
using BlazorRTC.UI;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7282/") });
builder.Services.AddMudServices();
builder.Services.AddScoped<AppStateManager>();
builder.Services.AddBlazoredLocalStorage();

await builder.Build().RunAsync();
