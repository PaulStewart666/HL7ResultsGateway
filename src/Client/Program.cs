using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using HL7ResultsGateway.Client;
using HL7ResultsGateway.Client.Core.Extensions;
using HL7ResultsGateway.Client.Features.Dashboard.Extensions;
using HL7ResultsGateway.Client.Features.HL7Testing.Extensions;
using HL7ResultsGateway.Client.Features.JsonToHL7.Extensions;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Core Services (includes configuration, HTTP client, authentication, themes)
builder.Services.AddCoreServices(builder.Configuration);

// Feature Services
builder.Services.AddDashboardServices();
builder.Services.AddHL7TestingServices();
builder.Services.AddJsonToHL7Services();

await builder.Build().RunAsync();
