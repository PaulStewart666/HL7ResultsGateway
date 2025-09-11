using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using HL7ResultsGateway.Client;
using HL7ResultsGateway.Client.Core.DependencyInjection;
using HL7ResultsGateway.Client.Features.HL7Testing.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAdB2C", options.ProviderOptions.Authentication);
});

// Add HL7 Testing Services
builder.Services.AddScoped<IHL7MessageService, HL7MessageService>();
builder.Services.AddSingleton<ITestMessageRepository, TestMessageRepository>();

builder.Services.AddThemeService();

await builder.Build().RunAsync();
