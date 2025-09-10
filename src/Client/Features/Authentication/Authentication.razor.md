# Authentication Feature

## Purpose
Handles user authentication including login, logout, and user session management for the HL7 Results Gateway application.

## Components
- `LoginDisplay.razor` – displays current user status and logout option
- `RedirectToLogin.razor` – redirects unauthenticated users to login page
- `Authentication.razor` – main authentication page

## State
- Uses ASP.NET Core Authentication via `CascadingAuthenticationState`
- Integrates with Azure AD B2C for user management

## Services
- Leverages built-in `AuthenticationStateProvider`
- Integration with Microsoft Identity platform

## Dependencies
- Uses `Microsoft.AspNetCore.Components.Authorization`
- Integrates with `Core/Configuration` for auth settings
- May consume `Shared/Models` for user data transfer objects

## Notes
- Keep authentication components stateless where possible
- Follow UI → AuthenticationStateProvider → Identity Provider flow
- Ensure proper error handling for authentication failures
- Components should handle both authenticated and unauthenticated states gracefully
