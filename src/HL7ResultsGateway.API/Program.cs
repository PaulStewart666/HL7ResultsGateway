using HL7ResultsGateway.Application.UseCases.ConvertJsonToHL7;
using HL7ResultsGateway.Application.UseCases.ProcessHL7Message;
using HL7ResultsGateway.Domain.Services;
using HL7ResultsGateway.Domain.Services.Conversion;
using HL7ResultsGateway.Infrastructure.Logging;
using HL7ResultsGateway.Infrastructure.Services.Conversion;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Configure Application Insights
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Register domain services
builder.Services.AddScoped<IHL7MessageParser, HL7MessageParser>();

// Register infrastructure services
builder.Services.AddScoped<ILoggingService, SerilogLoggingService>();
builder.Services.AddScoped<IJsonHL7Converter, JsonHL7Converter>();

// Register application handlers
builder.Services.AddScoped<IProcessHL7MessageHandler, ProcessHL7MessageHandler>();
builder.Services.AddScoped<IConvertJsonToHL7Handler, ConvertJsonToHL7Handler>();

// Configure logging
builder.Services.AddLogging();

builder.Build().Run();
