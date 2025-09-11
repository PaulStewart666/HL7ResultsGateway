# JsonToHL7 Feature

## Purpose
The JsonToHL7 feature handles the conversion of structured JSON data into HL7 v2 messages. It provides a user-friendly interface for healthcare developers to test and validate JSON-to-HL7 conversions for integration scenarios.

## Components
- `JsonToHL7ConversionPage.razor` – Main page for JSON to HL7 conversion workflow
- `JsonInputComponent.razor` – Input form for entering JSON message data with template support
- `HL7ResultsComponent.razor` – Results display showing converted HL7 message and parsed structure

## State
- Component-level state management for conversion workflow
- Form validation state for JSON input fields
- Conversion results cached temporarily for download functionality

## Services
- `JsonToHL7Service.cs` – Service for JSON to HL7 conversion API calls, injected via DI

## Dependencies
- Uses `Core/Logging/SerilogConfiguration` for logging conversion operations
- Consumes `Shared/Models/ApiResponse` for API communication results
- Depends on Azure Functions API (`/api/convert-json-to-hl7` endpoint)
- Uses `Models/JsonTestTemplate.cs` for predefined test scenarios

## Notes
Architecture follows the feature-based organization pattern with clear separation of concerns. The JsonToHL7Service handles all API interactions while components focus on user interface concerns. Template system provides realistic test data for common lab result scenarios. All services registered through feature-specific DI extension for maintainability and testability.