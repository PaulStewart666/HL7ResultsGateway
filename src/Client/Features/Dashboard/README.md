# Dashboard Feature

## Purpose
Provides the main dashboard interface for viewing HL7 message processing statistics and system status.

## Components
- `Home.razor` – main dashboard page displaying key metrics and recent activity

## State
- Dashboard state will be managed through dedicated state management when implemented
- Real-time data updates for message processing statistics

## Services
- `DashboardService.cs` – (to be implemented) handles API calls for dashboard data
- Integration with HL7 processing APIs for metrics

## Dependencies
- Uses `Core/Configuration` for API endpoint configuration
- Consumes `Shared/Models/ApiResponse` for standardized API responses
- May use `Core/Logging` for diagnostic information

## Notes
- Dashboard should update in real-time or near real-time
- Keep dashboard components focused on data presentation
- Follow UI → Service → API flow for data retrieval
- Consider caching strategies for frequently accessed data