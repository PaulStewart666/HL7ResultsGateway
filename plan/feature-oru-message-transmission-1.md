---
goal: Implement HL7 ORU Message Transmission System with Multi-Protocol Support
version: 1.0
date_created: 2025-09-16
last_updated: 2025-09-16
owner: Development Team
status: 'Planned'
tags: ['feature', 'hl7', 'transmission', 'architecture', 'clean-architecture']
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

This implementation plan delivers a comprehensive ORU (Observation Result Unsolicited) message transmission system that extends the existing HL7 Results Gateway with the capability to send HL7v2.5.1 compliant messages to external healthcare systems. The feature implements Clean Architecture principles, SOLID design patterns, and provides multi-protocol transmission support (HTTP/HTTPS, MLLP, SFTP) with comprehensive logging, validation, and error handling.

## 1. Requirements & Constraints

- **REQ-001**: Send HL7 ORU^R01 messages following HL7v2.5.1 standard specification
- **REQ-002**: Support multiple transmission protocols (HTTP/HTTPS, MLLP, SFTP)
- **REQ-003**: Implement comprehensive audit logging for all transmission attempts
- **REQ-004**: Provide real-time transmission status and acknowledgment handling
- **REQ-005**: Maintain backward compatibility with existing API structure
- **REQ-006**: Support configurable endpoint destinations with SSL/TLS enforcement
- **SEC-001**: All transmissions must support SSL/TLS encryption
- **SEC-002**: Implement input validation to prevent injection attacks
- **SEC-003**: Secure credential management for endpoint authentication
- **SEC-004**: Audit trail compliance for healthcare data transmission
- **ARCH-001**: Follow existing Clean Architecture patterns (Domain → Application → Infrastructure → API)
- **ARCH-002**: Implement Command/Query Responsibility Segregation (CQRS) pattern
- **ARCH-003**: Use dependency injection throughout all layers
- **CON-001**: Must integrate with existing Azure Functions infrastructure
- **CON-002**: Maintain consistency with current error handling patterns
- **CON-003**: Support Azure Application Insights for monitoring and telemetry
- **GUD-001**: Follow .NET 9 best practices and async/await patterns
- **GUD-002**: Implement comprehensive unit and integration testing
- **PAT-001**: Use Factory pattern for transmission provider selection
- **PAT-002**: Implement Repository pattern for transmission logging
- **PAT-003**: Apply Strategy pattern for different transmission protocols

## 2. Implementation Steps

### Implementation Phase 1: Domain Layer Foundation

- GOAL-001: Establish domain models, value objects, and service interfaces for ORU transmission

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `TransmissionProtocol` enum in `src/HL7ResultsGateway.Domain/ValueObjects/TransmissionProtocol.cs` with HTTP, HTTPS, MLLP, SFTP values | ✅ | 2025-09-16 |
| TASK-002 | Create `HL7TransmissionRequest` record in `src/HL7ResultsGateway.Domain/Models/HL7TransmissionRequest.cs` with endpoint, message, headers, timeout, and protocol properties | ✅ | 2025-09-16 |
| TASK-003 | Create `TransmissionResult` record in `src/HL7ResultsGateway.Domain/Models/TransmissionResult.cs` with success status, transmission ID, error handling, and timing metrics | ✅ | 2025-09-16 |
| TASK-004 | Create `HL7TransmissionLog` entity in `src/HL7ResultsGateway.Domain/Entities/HL7TransmissionLog.cs` for audit trail persistence | ✅ | 2025-09-16 |
| TASK-005 | Create `IHL7TransmissionProvider` interface in `src/HL7ResultsGateway.Domain/Services/Transmission/IHL7TransmissionProvider.cs` with async transmission methods | ✅ | 2025-09-16 |
| TASK-006 | Create `IHL7TransmissionProviderFactory` interface in `src/HL7ResultsGateway.Domain/Services/Transmission/IHL7TransmissionProviderFactory.cs` for provider selection | ✅ | 2025-09-16 |
| TASK-007 | Create `IHL7TransmissionRepository` interface in `src/HL7ResultsGateway.Domain/Services/IHL7TransmissionRepository.cs` for audit logging | ✅ | 2025-09-16 |
| TASK-008 | Create `TransmissionException` custom exception in `src/HL7ResultsGateway.Domain/Exceptions/TransmissionException.cs` for transmission-specific errors | ✅ | 2025-09-16 |

### Implementation Phase 2: Application Layer Commands and Handlers

- GOAL-002: Implement CQRS pattern with command handlers for ORU message transmission

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | Create `SendORUMessageCommand` record in `src/HL7ResultsGateway.Application/UseCases/SendORUMessage/SendORUMessageCommand.cs` with validation attributes | ✅ | 2025-09-17 |
| TASK-010 | Create `SendORUMessageResult` record in `src/HL7ResultsGateway.Application/UseCases/SendORUMessage/SendORUMessageResult.cs` with comprehensive response data | ✅ | 2025-09-17 |
| TASK-011 | Create `ISendORUMessageHandler` interface in `src/HL7ResultsGateway.Application/UseCases/SendORUMessage/ISendORUMessageHandler.cs` | ✅ | 2025-09-17 |
| TASK-012 | Implement `SendORUMessageHandler` class in `src/HL7ResultsGateway.Application/UseCases/SendORUMessage/SendORUMessageHandler.cs` with full business logic | ✅ | 2025-09-17 |
| TASK-013 | Create `SendORURequestDTO` class in `src/HL7ResultsGateway.Application/DTOs/SendORURequestDTO.cs` for API request deserialization | ✅ | 2025-09-17 |
| TASK-014 | Create `SendORUResponseDTO` class in `src/HL7ResultsGateway.Application/DTOs/SendORUResponseDTO.cs` for structured API responses | ✅ | 2025-09-17 |
| TASK-015 | Implement FluentValidation validator `SendORURequestValidator` in `src/HL7ResultsGateway.Application/Validators/SendORURequestValidator.cs` | ✅ | 2025-09-17 |

### Implementation Phase 3: Infrastructure Layer Implementations

- GOAL-003: Implement concrete transmission providers and repository for data persistence

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-016 | Create base `BaseHL7TransmissionProvider` abstract class in `src/HL7ResultsGateway.Infrastructure/Services/Transmission/BaseHL7TransmissionProvider.cs` | ✅ | 2025-09-17 |
| TASK-017 | Implement `HttpHL7TransmissionProvider` in `src/HL7ResultsGateway.Infrastructure/Services/Transmission/HttpHL7TransmissionProvider.cs` for HTTP/HTTPS transmission | ✅ | 2025-09-17 |
| TASK-018 | Implement `MLLPTransmissionProvider` in `src/HL7ResultsGateway.Infrastructure/Services/Transmission/MLLPTransmissionProvider.cs` for MLLP protocol | ✅ | 2025-09-17 |
| TASK-019 | Implement `SftpTransmissionProvider` in `src/HL7ResultsGateway.Infrastructure/Services/Transmission/SftpTransmissionProvider.cs` for SFTP file transfer | ✅ | 2025-09-17 |
| TASK-020 | Implement `HL7TransmissionProviderFactory` in `src/HL7ResultsGateway.Infrastructure/Services/Transmission/HL7TransmissionProviderFactory.cs` with provider selection logic | ✅ | 2025-09-17 |
| TASK-021 | Implement `CosmosHL7TransmissionRepository` in `src/HL7ResultsGateway.Infrastructure/Repositories/CosmosHL7TransmissionRepository.cs` for Azure Cosmos DB persistence | ✅ | 2025-09-17 |
| TASK-022 | Create `HL7TransmissionOptions` configuration class in `src/HL7ResultsGateway.Infrastructure/Configuration/HL7TransmissionOptions.cs` with validation attributes | ✅ | 2025-09-17 |
| TASK-023 | Create `EndpointConfiguration` class in `src/HL7ResultsGateway.Infrastructure/Configuration/EndpointConfiguration.cs` for endpoint-specific settings | ✅ | 2025-09-17 |

### Implementation Phase 4: API Layer Azure Function

- GOAL-004: Create Azure Function endpoint with comprehensive error handling and response formatting

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-024 | Create `SendORUMessage` Azure Function in `src/HL7ResultsGateway.API/SendORUMessage.cs` with proper dependency injection | |  |
| TASK-025 | Create `IResponseDTOFactory` interface in `src/HL7ResultsGateway.API/Factories/IResponseDTOFactory.cs` for consistent response formatting | |  |
| TASK-026 | Implement `ResponseDTOFactory` class in `src/HL7ResultsGateway.API/Factories/ResponseDTOFactory.cs` with standardized response patterns | |  |
| TASK-027 | Create `ApiResponse<T>` generic class in `src/HL7ResultsGateway.API/Models/ApiResponse.cs` for structured API responses | |  |
| TASK-028 | Update `Program.cs` in `src/HL7ResultsGateway.API/Program.cs` to register all new dependencies and services | |  |
| TASK-029 | Create HTTP test file `send-oru-message.http` in `src/HL7ResultsGateway.API/` for endpoint testing | |  |

### Implementation Phase 5: Testing Implementation

- GOAL-005: Implement comprehensive unit and integration tests for all components

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-030 | Create unit tests `SendORUMessageHandlerTests.cs` in `tests/HL7ResultsGateway.Application.Tests/UseCases/SendORUMessage/` | |  |
| TASK-031 | Create unit tests `HttpHL7TransmissionProviderTests.cs` in `tests/HL7ResultsGateway.Infrastructure.Tests/Services/Transmission/` | |  |
| TASK-032 | Create unit tests `HL7TransmissionProviderFactoryTests.cs` in `tests/HL7ResultsGateway.Infrastructure.Tests/Services/Transmission/` | |  |
| TASK-033 | Create integration tests `SendORUMessageTests.cs` in `tests/HL7ResultsGateway.API.Tests/` for end-to-end API testing | |  |
| TASK-034 | Create unit tests `CosmosHL7TransmissionRepositoryTests.cs` in `tests/HL7ResultsGateway.Infrastructure.Tests/Repositories/` | |  |
| TASK-035 | Create validation tests `SendORURequestValidatorTests.cs` in `tests/HL7ResultsGateway.Application.Tests/Validators/` | |  |

### Implementation Phase 6: Configuration and Documentation

- GOAL-006: Complete configuration setup, documentation, and deployment readiness

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-036 | Update `appsettings.json` template with HL7Transmission configuration section | |  |
| TASK-037 | Update `local.settings.json` in API project with development configuration | |  |
| TASK-038 | Create API documentation `docs/api/send-oru-message.md` with request/response examples | |  |
| TASK-039 | Update main README.md with ORU transmission feature documentation | |  |
| TASK-040 | Create troubleshooting guide `docs/troubleshooting/oru-transmission.md` | |  |
| TASK-041 | Update Azure deployment scripts to include new configuration requirements | |  |

## 3. Alternatives

- **ALT-001**: **Monolithic Service Approach** - Implement all transmission logic in a single service class instead of using multiple providers. Rejected due to violation of Single Responsibility Principle and difficulty in testing/maintaining different protocols.
- **ALT-002**: **Direct Database Access** - Skip repository pattern and access database directly from handlers. Rejected to maintain clean architecture separation and enable easier testing.
- **ALT-003**: **Synchronous Processing** - Implement transmission as synchronous operations. Rejected due to potential timeout issues and blocking behavior in Azure Functions.
- **ALT-004**: **Queue-Based Processing** - Use Azure Service Bus for asynchronous message processing. Considered for future enhancement but rejected for initial implementation to maintain simplicity.
- **ALT-005**: **Generic Message Sender** - Create a generic message transmission system for all HL7 message types. Rejected to maintain focused scope and clear separation of concerns.

## 4. Dependencies

- **DEP-001**: Azure Cosmos DB SDK (`Microsoft.Azure.Cosmos`) - For transmission audit logging
- **DEP-002**: FluentValidation (`FluentValidation.AspNetCore`) - For request validation
- **DEP-003**: SSH.NET (`SSH.NET`) - For SFTP transmission provider implementation
- **DEP-004**: System.Net.Http - For HTTP/HTTPS transmission (already available in .NET)
- **DEP-005**: System.Net.Sockets - For MLLP transmission implementation (already available in .NET)
- **DEP-006**: Microsoft.Extensions.Options.DataAnnotations - For configuration validation
- **DEP-007**: Microsoft.Extensions.Http - For HTTP client factory pattern
- **DEP-008**: Existing domain entities (HL7Result, Patient, Observation) - For message construction

## 5. Files

- **FILE-001**: `src/HL7ResultsGateway.Domain/ValueObjects/TransmissionProtocol.cs` - Enum defining supported transmission protocols
- **FILE-002**: `src/HL7ResultsGateway.Domain/Models/HL7TransmissionRequest.cs` - Request model for transmission operations
- **FILE-003**: `src/HL7ResultsGateway.Domain/Models/TransmissionResult.cs` - Result model containing transmission outcomes
- **FILE-004**: `src/HL7ResultsGateway.Domain/Entities/HL7TransmissionLog.cs` - Entity for audit trail storage
- **FILE-005**: `src/HL7ResultsGateway.Domain/Services/Transmission/IHL7TransmissionProvider.cs` - Provider interface for transmission implementations
- **FILE-006**: `src/HL7ResultsGateway.Domain/Services/Transmission/IHL7TransmissionProviderFactory.cs` - Factory interface for provider selection
- **FILE-007**: `src/HL7ResultsGateway.Domain/Services/IHL7TransmissionRepository.cs` - Repository interface for audit logging
- **FILE-008**: `src/HL7ResultsGateway.Application/UseCases/SendORUMessage/SendORUMessageCommand.cs` - CQRS command for sending ORU messages
- **FILE-009**: `src/HL7ResultsGateway.Application/UseCases/SendORUMessage/SendORUMessageHandler.cs` - Command handler implementation
- **FILE-010**: `src/HL7ResultsGateway.Infrastructure/Services/Transmission/HttpHL7TransmissionProvider.cs` - HTTP/HTTPS transmission implementation
- **FILE-011**: `src/HL7ResultsGateway.Infrastructure/Services/Transmission/MLLPTransmissionProvider.cs` - MLLP transmission implementation
- **FILE-012**: `src/HL7ResultsGateway.Infrastructure/Services/Transmission/SftpTransmissionProvider.cs` - SFTP transmission implementation
- **FILE-013**: `src/HL7ResultsGateway.Infrastructure/Repositories/CosmosHL7TransmissionRepository.cs` - Cosmos DB repository implementation
- **FILE-014**: `src/HL7ResultsGateway.API/SendORUMessage.cs` - Azure Function endpoint for ORU transmission
- **FILE-015**: `src/HL7ResultsGateway.API/Factories/ResponseDTOFactory.cs` - Factory for standardized API responses

## 6. Testing

- **TEST-001**: Unit tests for `SendORUMessageHandler` covering all success and error scenarios
- **TEST-002**: Unit tests for each transmission provider (HTTP, MLLP, SFTP) with mocked dependencies
- **TEST-003**: Unit tests for `HL7TransmissionProviderFactory` testing provider selection logic
- **TEST-004**: Unit tests for `CosmosHL7TransmissionRepository` with in-memory test database
- **TEST-005**: Integration tests for `SendORUMessage` Azure Function with test containers
- **TEST-006**: Validation tests for `SendORURequestValidator` covering all validation rules
- **TEST-007**: End-to-end tests with mock external HL7 endpoints for complete workflow validation
- **TEST-008**: Performance tests for transmission operations to ensure acceptable response times
- **TEST-009**: Security tests for input validation and injection attack prevention
- **TEST-010**: Configuration tests for appsettings validation and dependency injection setup

## 7. Risks & Assumptions

- **RISK-001**: **External Endpoint Availability** - Third-party HL7 endpoints may be unreliable or have different authentication requirements. Mitigation: Implement comprehensive retry logic and detailed error reporting.
- **RISK-002**: **Network Connectivity** - Azure Functions may experience network issues affecting transmission. Mitigation: Implement timeout handling and proper error classification.
- **RISK-003**: **Protocol Compatibility** - Different healthcare systems may have varying implementations of HL7 standards. Mitigation: Provide configurable message formatting options and validation settings.
- **RISK-004**: **Performance Impact** - Synchronous transmission calls may affect API response times. Mitigation: Implement proper timeout values and consider async processing for future versions.
- **RISK-005**: **Security Vulnerabilities** - Handling healthcare data requires strict security measures. Mitigation: Implement comprehensive input validation, secure credential storage, and audit logging.
- **ASSUMPTION-001**: Azure Cosmos DB will be used for audit log storage and is available in the deployment environment
- **ASSUMPTION-002**: External HL7 endpoints will provide proper acknowledgment messages following HL7 standards
- **ASSUMPTION-003**: SSL/TLS certificates will be properly configured for secure transmission protocols
- **ASSUMPTION-004**: Development team has sufficient knowledge of HL7 standards and healthcare integration patterns
- **ASSUMPTION-005**: Existing Azure Functions infrastructure can handle additional load from transmission operations

## 8. Related Specifications / Further Reading

- [HL7 Version 2.5.1 Standard](http://www.hl7.org/implement/standards/product_brief.cfm?product_id=144)
- [MLLP (Minimal Lower Layer Protocol) Specification](https://www.hl7.org/documentcenter/public/wg/inm/mllp_transport_specification.PDF)
- [Azure Functions Best Practices](https://docs.microsoft.com/en-us/azure/azure-functions/functions-best-practices)
- [Clean Architecture in .NET](https://docs.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures#clean-architecture)
- [CQRS Pattern Implementation](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs)
- [Healthcare Data Security Requirements](https://www.hhs.gov/hipaa/for-professionals/security/index.html)
- [Azure Cosmos DB Best Practices](https://docs.microsoft.com/en-us/azure/cosmos-db/best-practice-dotnet)
- [Existing JSON to HL7 Converter Implementation](./feature-json-to-hl7-converter-1.md)
- [Existing HL7 Processing Architecture](./feature-azure-function-api-1.md)
