````instructions
# Copilot Instructions for HL7 Results Gateway

## Architecture Overview
This is a **Clean Architecture** HL7 message processing system with **Azure Functions API** and **Blazor WASM frontend**:

- **Domain** (`src/HL7ResultsGateway.Domain/`): Core entities, value objects, and domain services
- **Application** (`src/HL7ResultsGateway.Application/`): Use cases and handlers (follows CQRS pattern)
- **API** (`src/HL7ResultsGateway.API/`): Azure Functions HTTP triggers (.NET 9.0 Isolated Worker)
- **Infrastructure** (`src/HL7ResultsGateway.Infrastructure/`): External concerns (placeholder)
- **Client** (`src/Client/HL7ResultsGateway.Client/`): Blazor WASM frontend (.NET 10.0 Preview)

## Key Patterns & Conventions

### Testing Standards
- **All tests use FluentAssertions** - Replace `Assert.Equal()` with `.Should().Be()`
- **xUnit with Moq** for mocking dependencies
- **Test structure**: Arrange/Act/Assert with clear comments
- **Clean Architecture test separation**: All tests live in `/tests/` folder outside `/src/`
- **Test organization**: Mirror source structure (e.g., `/tests/HL7ResultsGateway.Client/Tests/` for client tests)
- **Example pattern**:
```csharp
result.Should().NotBeNull();
result.Success.Should().BeTrue();
result.ParsedMessage!.MessageType.Should().Be(HL7MessageType.ORU_R01);
```

### Code Style (.editorconfig enforced)
- **4-space indentation** for C#
- **Using directives**: Sort system first, separate groups with blank lines
- **Explicit accessibility modifiers** required (public/private/internal)
- **File-scoped namespaces** preferred
- **Remove unnecessary usings** (IDE0005 warning enabled)

### Domain Layer Patterns
- **HL7MessageParser**: Parses raw HL7 strings into strongly-typed `HL7Result` entities
- **Value Objects**: `HL7MessageType`, `Gender`, `ObservationStatus` (immutable enums)
- **Entities**: `HL7Result`, `Patient`, `Observation` (mutable domain objects)
- **Custom Exceptions**: `HL7ParseException` for domain-specific errors

### Application Layer (CQRS)
- **Commands**: `ProcessHL7MessageCommand` with message content and source
- **Handlers**: Implement `IProcessHL7MessageHandler` interface
- **Results**: Return success/failure objects with parsed data and timestamps
- **DI Pattern**: Constructor injection for all dependencies

### Azure Functions Specifics
- **Isolated Worker Model** (.NET 9.0 due to Azure Functions compatibility)
- **HTTP Triggers**: Use `[HttpTrigger(AuthorizationLevel.Function)]` for API endpoints
- **DI Registration**: Configure in `Program.cs` using `builder.Services.AddScoped<>()`
- **Health Check**: Anonymous auth level, returns service status with timestamp
- **Error Handling**: Return appropriate HTTP status codes (400/500) with error objects

### Blazor WASM Client Specifics
- **Target Framework**: .NET 10.0 Preview for latest features
- **Authentication**: Azure AD B2C with MSAL
- **PWA Support**: Service worker configured for offline scenarios
- **Deployment**: Azure Static Web Apps
- **UI Framework**: Bootstrap with custom styling, bilingual English/Welsh support
- **Architecture**: Component-based with clean separation of concerns
- **Testing**: Client tests located in `/tests/HL7ResultsGateway.Client/Tests/`

## Development Workflow

### Build & Test Commands
```bash
# Build entire solution
dotnet build

# Run all tests
dotnet test

# Start Azure Functions locally
cd src/HL7ResultsGateway.API
func start

# Start Blazor WASM client locally (development)
cd src/Client
dotnet run

# Clean build (resolves file handle conflicts)
# Use the PowerShell script for automated clean/build:
.\scripts\clean-build.ps1
```

### Testing Endpoints
- **API Tests**: Use `.http` files in API project: `health-check.http`, `process-hl7-message.http`
- **Health check**: `GET http://localhost:7071/api/health`
- **HL7 processing**: `POST http://localhost:7071/api/hl7/process` (Content-Type: text/plain)
- **Client**: `https://localhost:5001` (or configured port) for Blazor WASM frontend

### Adding New Features
1. **Domain First**: Create entities/value objects in Domain layer
2. **Application Logic**: Add use case handlers in Application layer
3. **API Endpoints**: Create HTTP trigger functions in API layer
4. **Frontend Integration**: Add Blazor components and services to consume API
5. **Tests**: Write tests for each layer using FluentAssertions (in `/tests/` folder)
6. **HTTP Tests**: Add endpoint tests to appropriate `.http` file
7. **Client Tests**: Add UI/integration tests in `/tests/HL7ResultsGateway.Client/Tests/`

## Project Dependencies
- **Target Framework**:
  - .NET 9.0 (API, Domain, Application, Infrastructure, all tests)
  - .NET 10.0 Preview (Blazor WASM Client only)
- **Azure Functions**: Isolated Worker model with Core Tools v4.2.2+
- **Blazor WASM**: PWA-enabled with Azure AD B2C authentication
- **Testing**: xUnit + FluentAssertions + Moq + Coverlet
- **Logging**: Microsoft.Extensions.Logging with Application Insights
- **Frontend**: Bootstrap, Bootstrap Icons, Google Fonts, Font Awesome (free)

## Critical Integration Points
- **DI Container**: Services registered in `Program.cs` must match interface implementations
- **HL7 Parser**: `IHL7MessageParser` is the core domain service for message processing
- **Layer Dependencies**: API → Application → Domain (never reverse)
- **Client-API Communication**: Blazor client consumes Azure Functions API via HTTP calls
- **Test Projects**: Mirror source structure and reference appropriate layers
- **Solution Structure**: Client project in separate folder under `/src/Client/`

## Development Environment Setup
- **VS Code Settings**: File watchers exclude `bin/` and `obj/` to prevent conflicts
- **PowerShell Scripts**: Use `scripts/clean-build.ps1` for automated clean/build workflow
- **Solution Warning**: Azure Functions SDK may generate `WorkerExtensions` - this is normal

## Common Gotchas
- **Azure Functions**: Use .NET 9.0, not .NET 10+ (compatibility limitation)
- **Blazor Client**: Uses .NET 10.0 Preview for latest features
- **EditorConfig**: Auto-format on save to match 4-space indentation rules
- **FluentAssertions**: Always prefer `.Should()` syntax over Assert methods
- **Null Safety**: Enable nullable reference types, use `!` operator when needed for tests
- **File Watchers**: Build/clean may trigger false VS Code file watcher alerts - this is normal
- **Solution Warnings**: `WorkerExtensions` duplicate warnings from Azure Functions SDK are safe to ignore

## Planning and Documentation
- **Implementation Plans**: All feature plans in `/plan/` folder with machine-readable format
- **GitHub Issues**: Use PowerShell scripts in `/scripts/` to generate GitHub issues from plans
- **Status Tracking**: Plans include task tables with completion status and dates
- **Clean Architecture**: Test separation documented in plan rationale sections
````
