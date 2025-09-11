using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using HL7ResultsGateway.Client;
using HL7ResultsGateway.Client.Core.DependencyInjection;
using HL7ResultsGateway.Client.Features.HL7Testing.Services;
using HL7ResultsGateway.Client.Features.JsonToHL7.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient for Blazor client (default)
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Configure named HttpClient for Azure Functions API
builder.Services.AddHttpClient("AzureFunctionsApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:7071/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAdB2C", options.ProviderOptions.Authentication);
});

// Add HL7 Testing Services
builder.Services.AddScoped<IHL7MessageService>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("AzureFunctionsApi");
    return new HL7MessageService(httpClient);
});
builder.Services.AddSingleton<ITestMessageRepository, TestMessageRepository>();

// Add JSON to HL7 Conversion Services
builder.Services.AddScoped<JsonToHL7Service>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("AzureFunctionsApi");
    return new JsonToHL7Service(httpClient);
});

builder.Services.AddThemeService();

await builder.Build().RunAsync();
