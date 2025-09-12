using HL7ResultsGateway.Client.Features.HL7Testing.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HL7ResultsGateway.Client.Features.HL7Testing.Extensions;

/// <summary>
/// Extension methods for configuring HL7Testing feature services.
/// </summary>
public static class HL7TestingServiceExtensions
{
    /// <summary>
    /// Adds HL7Testing feature services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddHL7TestingServices(this IServiceCollection services)
    {
        // Register HL7Testing services
        services.AddScoped<IHL7MessageService>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("AzureFunctionsApi");
            return new HL7MessageService(httpClient);
        });
        
        services.AddScoped<TestMessageRepository>();

        return services;
    }
}
