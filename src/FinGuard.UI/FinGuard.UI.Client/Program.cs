using FinGuard.UI.Client;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddClientFeatureServices(builder.Configuration);

await builder.Build().RunAsync();
