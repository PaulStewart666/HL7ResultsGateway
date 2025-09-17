using HL7ResultsGateway.API.Factories;
using HL7ResultsGateway.Application.UseCases.ConvertJsonToHL7;
using HL7ResultsGateway.Application.UseCases.ProcessHL7Message;
using HL7ResultsGateway.Application.UseCases.SendORUMessage;
using HL7ResultsGateway.Application.Validators;
using HL7ResultsGateway.Domain.Services;
using HL7ResultsGateway.Domain.Services.Conversion;
using HL7ResultsGateway.Domain.Services.Transmission;
using HL7ResultsGateway.Infrastructure.Configuration;
using HL7ResultsGateway.Infrastructure.Logging;
using HL7ResultsGateway.Infrastructure.Repositories;
using HL7ResultsGateway.Infrastructure.Services.Conversion;
using HL7ResultsGateway.Infrastructure.Services.Transmission;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Configure Application Insights
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Configure HL7 Transmission options
builder.Services.Configure<HL7TransmissionOptions>(
    builder.Configuration.GetSection(HL7TransmissionOptions.SectionName));

// Register domain services
builder.Services.AddScoped<IHL7MessageParser, HL7MessageParser>();

// Register infrastructure services
builder.Services.AddScoped<ILoggingService, SerilogLoggingService>();
builder.Services.AddScoped<IJsonHL7Converter, JsonHL7Converter>();

// Register transmission services
builder.Services.AddHttpClient<HttpHL7TransmissionProvider>();
builder.Services.AddHttpClient<MLLPTransmissionProvider>();
builder.Services.AddHttpClient<SftpTransmissionProvider>();

builder.Services.AddScoped<HttpHL7TransmissionProvider>();
builder.Services.AddScoped<MLLPTransmissionProvider>();
builder.Services.AddScoped<SftpTransmissionProvider>();
builder.Services.AddScoped<IHL7TransmissionProviderFactory, HL7TransmissionProviderFactory>();

// Register repository - use CosmosDB if available, otherwise in-memory for development
var configuration = builder.Configuration;
var cosmosConnectionString = configuration.GetConnectionString("CosmosDb") ??
                            configuration.GetValue<string>("HL7Transmission:CosmosDb:ConnectionString");

if (!string.IsNullOrWhiteSpace(cosmosConnectionString))
{
    // Production: Use Cosmos DB
    builder.Services.AddSingleton<CosmosClient>(serviceProvider =>
    {
        return new CosmosClient(cosmosConnectionString);
    });
    builder.Services.AddScoped<IHL7TransmissionRepository, CosmosHL7TransmissionRepository>();
}
else
{
    // Development: Use in-memory implementation
    builder.Services.AddSingleton<IHL7TransmissionRepository, InMemoryHL7TransmissionRepository>();
}

// Register application handlers
builder.Services.AddScoped<IProcessHL7MessageHandler, ProcessHL7MessageHandler>();
builder.Services.AddScoped<IConvertJsonToHL7Handler, ConvertJsonToHL7Handler>();
builder.Services.AddScoped<ISendORUMessageHandler, SendORUMessageHandler>();

// Register validators
builder.Services.AddScoped<SendORURequestValidator>();

// Register API factories
builder.Services.AddScoped<IResponseDTOFactory, ResponseDTOFactory>();

// Configure logging
builder.Services.AddLogging();

builder.Build().Run();
