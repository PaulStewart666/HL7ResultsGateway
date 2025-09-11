````instructions
# Copilot Instructions for HL7 Results Gateway

## Architecture Overview
This is a **Clean Architecture** HL7 message processing system with **Azure Functions API** and **Blazor WASM frontend**:

- **Domain** (`src/HL7ResultsGateway.Domain/`): Core entities, value objects, and domain services
- **Application** (`src/HL7ResultsGateway.Application/`): Use cases and handlers (follows CQRS pattern)
- **API** (`src/HL7ResultsGateway.API/`): Azure Functions HTTP triggers (.NET 9.0 Isolated Worker)
- **Infrastructure** (`src/HL7ResultsGateway.Infrastructure/`): External services, logging abstraction (ILoggingService), and converters
- **Client** (`src/Client/HL7ResultsGateway.Client/`): Blazor WASM frontend (.NET 10.0 Preview) with feature-based organization

## Key Patterns & Conventions

### Infrastructure Layer Patterns
- **Logging Abstraction**: `ILoggingService` in `/Logging/` for swappable logging implementations (Serilog default)
- **Service Implementations**: Domain service interfaces implemented here (e.g., `JsonHL7Converter`)
- **SOLID Compliance**: All external dependencies injected via interfaces from Domain layer
- **Null Safety**: Use null-coalescing operators (`??`) for nullable reference handling

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
- **Feature Organization**: `/Features/{FeatureName}/{Models|Services|Components|Pages|Extensions}/` structure
- **Feature Documentation**: Each feature folder MUST contain `{Feature}.razor.md` documentation file
- **Feature DI Pattern**: Each feature MUST have `Extensions/{FeatureName}ServiceExtensions.cs` for DI registration
- **HTTP Client Pattern**: Named HttpClient ("AzureFunctionsApi") for API calls, registered in `Program.cs`
- **Authentication**: Azure AD B2C with MSAL
- **PWA Support**: Service worker configured for offline scenarios
- **Deployment**: Azure Static Web Apps
- **UI Framework**: Bootstrap with custom styling, bilingual English/Welsh support
- **Architecture**: Component-based with clean separation of concerns
- **Testing**: Client tests located in `/tests/HL7ResultsGateway.Client/Tests/`

## Component File Organization

Each Blazor component should follow this file organization pattern:

```
ComponentName/
├── ComponentName.razor          # Markup and basic data binding
├── ComponentName.razor.cs       # Code-behind with component logic
├── ComponentName.razor.css      # Scoped CSS styling
└── ComponentName.razor.js       # Scoped JavaScript (when needed)
```

## Feature-Based Organization (MANDATORY)

Each feature folder MUST follow this structure and include required documentation:

```
Features/
└── {FeatureName}/
    ├── {Feature}.razor.md       # MANDATORY: Feature documentation
    ├── Extensions/
    │   └── {FeatureName}ServiceExtensions.cs  # MANDATORY: DI registration
    ├── Models/
    ├── Services/
    ├── Components/
    └── Pages/
```

**Required Feature Documentation** (`{Feature}.razor.md`):
```markdown
# {FeatureName} Feature

## Purpose
Brief description of what this feature handles.

## Components
- `Component1.razor` – description of component
- `Component2.razor` – description of component

## State
- `FeatureState.cs` – description of state management

## Services
- `FeatureService.cs` – description of service, injected via DI

## Dependencies
- Uses `Core/Logging/SerilogConfiguration`
- Consumes `Shared/Models/ApiResponse`

## Notes
Architecture notes and design decisions.
```

**Required DI Extensions** (`Extensions/{FeatureName}ServiceExtensions.cs`):
```csharp
using Microsoft.Extensions.DependencyInjection;

namespace HL7ResultsGateway.Client.Features.{FeatureName}.Extensions;

public static class {FeatureName}ServiceExtensions
{
    public static IServiceCollection Add{FeatureName}Services(this IServiceCollection services)
    {
        // Register feature-specific services
        services.AddScoped<I{FeatureName}Service, {FeatureName}Service>();

        // Register any HTTP clients specific to this feature
        services.AddHttpClient("{FeatureName}Client", client =>
        {
            client.BaseAddress = new Uri("http://localhost:7071/");
        });

        return services;
    }
}
```

**Program.cs Registration Pattern**:
```csharp
// Feature registration - one line per feature
builder.Services.AddAuthenticationServices();
builder.Services.AddJsonToHL7Services();
builder.Services.AddHL7TestingServices();
```

## Scoped CSS Guidelines

**Scoped CSS (.razor.css) Benefits:**
- Automatic CSS isolation prevents style bleeding
- Component-specific styling without global conflicts
- Better maintainability and debugging
- Automatic generation of unique CSS selectors

**Scoped CSS Best Practices:**
```css
/* ComponentName.razor.css */

/* Component root styling */
.component-container {
    padding: 1rem;
    border-radius: 0.375rem;
    background-color: var(--bs-light);
}

/* Use CSS custom properties for theming */
.primary-button {
    background-color: var(--bs-primary);
    border-color: var(--bs-primary);
}

/* Deep selectors for child component styling */
::deep .child-component {
    margin-bottom: 1rem;
}

/* Responsive design within component */
@media (max-width: 768px) {
    .component-container {
        padding: 0.5rem;
    }
}

/* Component state classes */
.loading-state {
    opacity: 0.6;
    pointer-events: none;
}

.error-state {
    border-left: 4px solid var(--bs-danger);
    background-color: rgba(220, 53, 69, 0.1);
}
```

## Blazor Code Style and Structure

- Write idiomatic and efficient Blazor and C# code.
- Follow .NET and Blazor conventions.
- Use Razor Components appropriately for component-based UI development.
- Prefer inline functions for smaller components but separate complex logic into code-behind or service classes.
- Async/await should be used where applicable to ensure non-blocking UI operations.

## Naming Conventions

- Follow PascalCase for component names, method names, and public members.
- Use camelCase for private fields and local variables.
- Prefix interface names with "I" (e.g., IUserService).

## Blazor and .NET Specific Guidelines

- Utilize Blazor's built-in features for component lifecycle (e.g., OnInitializedAsync, OnParametersSetAsync).
- Use data binding effectively with @bind.
- Leverage Dependency Injection for services in Blazor.
- Structure Blazor components and services following Separation of Concerns.
- Always use the latest version C#, currently C# 13 features like record types, pattern matching, and global usings.

## Error Handling and Validation

- Implement proper error handling for Blazor pages and API calls.
- Use logging for error tracking in the backend and consider capturing UI-level errors in Blazor with tools like ErrorBoundary.
- Implement validation using FluentValidation or DataAnnotations in forms.

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
- **API Tests**: Use `.http` files in API project: `health-check.http`, `process-hl7-message.http`, `convert-json-to-hl7.http`
- **Health check**: `GET http://localhost:7071/api/health`
- **HL7 processing**: `POST http://localhost:7071/api/hl7/process` (Content-Type: text/plain)
- **JSON to HL7**: `POST http://localhost:7071/api/convert-json-to-hl7` (Content-Type: application/json)
- **Client**: `https://localhost:5001` (or configured port) for Blazor WASM frontend
- **Azure Functions Start**: Must run `func start` from `src/HL7ResultsGateway.API/` directory

### Adding New Features
1. **Domain First**: Create entities/value objects in Domain layer
2. **Application Logic**: Add use case handlers in Application layer
3. **Infrastructure Implementation**: Implement domain interfaces in Infrastructure layer
4. **API Endpoints**: Create HTTP trigger functions in API layer
5. **Frontend Integration**: Add Blazor components and services in feature directories
6. **Tests**: Write tests for each layer using FluentAssertions (in `/tests/` folder)
7. **HTTP Tests**: Add endpoint tests to appropriate `.http` file
8. **DI Registration**: Update `Program.cs` in both API and Client projects
9. **Client Integration**: Add feature directory under `Features/` with Models/Services/Components/Pages/Extensions
10. **MANDATORY**: Create `{Feature}.razor.md` documentation file
11. **MANDATORY**: Create `Extensions/{FeatureName}ServiceExtensions.cs` for DI registration
12. **Program.cs**: Register feature with single line: `builder.Services.Add{FeatureName}Services()`

## Project Dependencies
- **Target Framework**:
  - .NET 9.0 (API, Domain, Application, Infrastructure, all tests)
  - .NET 10.0 Preview (Blazor WASM Client only)
- **Azure Functions**: Isolated Worker model with Core Tools v4.2.2+
- **Blazor WASM**: PWA-enabled with Azure AD B2C authentication
- **Testing**: xUnit + FluentAssertions + Moq + Coverlet
- **Logging**: Serilog via ILoggingService abstraction with Microsoft.Extensions.Logging
- **Frontend**: Bootstrap, Bootstrap Icons, Google Fonts, Font Awesome (free)

## Critical Integration Points
- **DI Container**: Services registered in `Program.cs` must match interface implementations
- **HL7 Parser**: `IHL7MessageParser` is the core domain service for message processing
- **Layer Dependencies**: API → Application → Domain (never reverse)
- **Client-API Communication**: Blazor client consumes Azure Functions API via HTTP calls
- **Test Projects**: Mirror source structure and reference appropriate layers
- **Solution Structure**: Client project in separate folder under `/src/Client/`

## Common Gotchas
- **Azure Functions**: Use .NET 9.0, not .NET 10+ (compatibility limitation)
- **Blazor Client**: Uses .NET 10.0 Preview for latest features
- **EditorConfig**: Auto-format on save to match 4-space indentation rules
- **FluentAssertions**: Always prefer `.Should()` syntax over Assert methods
- **Null Safety**: Enable nullable reference types, use `!` operator when needed for tests
- **File Watchers**: Build/clean may trigger false VS Code file watcher alerts - this is normal
- **Solution Warnings**: `WorkerExtensions` duplicate warnings from Azure Functions SDK are safe to ignore

## Development Environment Setup
- **VS Code Settings**: File watchers exclude `bin/` and `obj/` to prevent conflicts
- **PowerShell Scripts**: Use `scripts/clean-build.ps1` for automated clean/build workflow
- **Solution Warning**: Azure Functions SDK may generate `WorkerExtensions` - this is normal

## Planning and Documentation
- **Implementation Plans**: All feature plans in `/plan/` folder with machine-readable format
- **GitHub Issues**: Use PowerShell scripts in `/scripts/` to generate GitHub issues from plans
- **Status Tracking**: Plans include task tables with completion status and dates
- **Clean Architecture**: Test separation documented in plan rationale sections

## Development Workflow
- **TDD Approach**: Write tests first, then implementation (see `JsonHL7Converter` feature as example)
- **Branch Strategy**: Create feature branches for new development (`feature/json-to-hl7-converter`)
- **Commit Messages**: Use conventional format with clear scope (`fix:`, `feat:`, `test:`)
- **Clean Build Script**: Use `.\scripts\clean-build.ps1` to resolve file handle conflicts in VS Code
````
