using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

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
            // Use IConfiguration directly to avoid scoped service resolution in root scope
            var config = sp.GetRequiredService<IConfiguration>();
            var hostEnvironment = sp.GetRequiredService<IWebAssemblyHostEnvironment>();

            var configuredBaseUrl = config["Api:BaseUrl"];
            var timeoutSeconds = config.GetValue<int>("Api:TimeoutSeconds", 30);

            Uri baseAddress;

            // Handle different BaseUrl configurations
            if (string.IsNullOrWhiteSpace(configuredBaseUrl))
            {
                // Empty or null: Use current host (works for custom domains and Azure Static Web Apps)
                baseAddress = new Uri(hostEnvironment.BaseAddress);
            }
            else if (configuredBaseUrl.StartsWith("http"))
            {
                // Absolute URL: Use as-is (for local development or different API hosts)
                baseAddress = new Uri(configuredBaseUrl);
            }
            else
            {
                // Relative URL: Combine with current host
                var hostUri = new Uri(hostEnvironment.BaseAddress);
                baseAddress = new Uri(hostUri, configuredBaseUrl.TrimStart('/'));
            }

            client.BaseAddress = baseAddress;
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);

            // Add default headers
            client.DefaultRequestHeaders.Add("User-Agent", "HL7ResultsGateway-Client/1.0");
        });

        return services;
    }
}
