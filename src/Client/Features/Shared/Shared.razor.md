# Shared Feature

## Purpose
Provides shared components, models, services, and layouts used across multiple features in the HL7 Results Gateway application.

## Components
- `MainLayout.razor` – primary application layout
- `NotFound.razor` – 404 error page
- Additional shared UI components as needed

## Models
- `ApiResponse.cs` – (to be implemented) standardized API response wrapper
- Common data transfer objects shared across features

## Services
- `HttpClientFactoryExtensions.cs` – (to be implemented) HTTP client configuration
- Shared service utilities and extensions

## Layout
- `MainLayout.razor` – defines the overall application shell
- Responsive design supporting mobile and desktop
- Consistent navigation and branding

## Dependencies
- Uses `Core/Configuration` for application settings
- Integrates with `Core/ErrorHandling` for global error boundaries
- May consume `Core/Logging` for diagnostic purposes

## Notes
- Keep shared components generic and reusable
- Avoid feature-specific business logic in shared components
- Maintain consistent styling and UX patterns
- Document breaking changes to shared components carefully
