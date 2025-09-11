using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using HL7ResultsGateway.Client.Core.Configuration;

namespace HL7ResultsGateway.Client.Core.Extensions;

/// <summary>
/// Extension methods for configuring HttpClient services.
/// </summary>
public static class HttpClientServiceExtensions
{
    /// <summary>
    /// Adds HttpClient services with centralized configuration.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The configuration root.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddHttpClientServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure default HttpClient for Blazor client
        services.AddScoped(sp => 
        {
            var hostEnvironment = sp.GetRequiredService<IWebAssemblyHostEnvironment>();
            return new HttpClient { BaseAddress = new Uri(hostEnvironment.BaseAddress) };
        });

        // Configure named HttpClient for Azure Functions API
        services.AddHttpClient("AzureFunctionsApi", (sp, client) =>
        {
            var apiConfig = sp.GetRequiredService<IOptionsSnapshot<ApiConfiguration>>().Value;
            
            client.BaseAddress = new Uri(apiConfig.BaseUrl);
            client.Timeout = apiConfig.Timeout;
            
            // Add default headers
            client.DefaultRequestHeaders.Add("User-Agent", "HL7ResultsGateway-Client/1.0");
        });
        
        return services;
    }
}