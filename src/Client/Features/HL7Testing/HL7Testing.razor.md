# HL7Testing Feature

## Purpose
The HL7Testing feature handles the parsing, validation, and testing of raw HL7 v2 messages. It provides tools for developers and healthcare professionals to validate HL7 messages and understand their structure.

## Components
- `HL7TestingPage.razor` – Main page for testing HL7 messages
- `HL7MessageInput.razor` – Input component for entering raw HL7 messages
- `HL7ParsedOutput.razor` – Display component showing parsed HL7 message structure
- `MessageSummary.razor` – Summary component showing key message details

## State
- No specific state management - uses component-level state for UI interactions
- Message parsing results stored temporarily in component state

## Services
- `HL7MessageService.cs` – Core service for HL7 message processing, injected via DI
- `TestMessageRepository.cs` – Repository for managing test message templates and samples

## Dependencies
- Uses `Core/Logging/SerilogConfiguration` for logging HL7 parsing operations
- Consumes `Shared/Models/HL7Result` for parsed message structure
- Depends on Azure Functions API for message processing

## Notes
Architecture follows Clean Architecture principles with clear separation between UI components and business logic. The HL7MessageService handles all API communication while components focus on presentation. All services are registered through the feature-specific DI extension pattern for maintainability.
