---
goal: Add Blazor WASM Client as Azure Static Web App Frontend for HL7ResultsGateway API
version: 1.0
date_created: 2025-09-02
last_updated: 2025-09-02
owner: Genomics-Partnership-Wales
status: 'Planned'
tags: [feature, frontend, blazor, wasm, azure, staticwebapp, authentication, localization, ci-cd, testing]
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

This plan describes the implementation of a Blazor WebAssembly (WASM) client project to serve as the frontend for the HL7ResultsGateway Azure Functions API. The client will be deployed as an Azure Static Web App, use Azure AD B2C authentication, consume all API endpoints, support offline/PWA scenarios, provide bilingual English/Welsh UI, and follow a clean, modern Bootstrap-based design. Automated testing and CI/CD will be included.

**Test Location Rationale:**
Following Clean Architecture, all automated tests for the Blazor WASM client will be placed in `/tests/HL7ResultsGateway.Client/Tests/` (outside the production code tree). This ensures strict separation of concerns, prevents test artifacts from leaking into production builds, and supports scalable, maintainable test infrastructure. This approach is preferred for larger solutions and aligns with Clean Architecture principles of isolating test code and dependencies from application code.

## 1. Requirements & Constraints

- **REQ-001**: Blazor WASM client project must be created using .NET CLI scaffolding
- **REQ-002**: Integrate Azure AD B2C authentication
- **REQ-003**: Deploy as Azure Static Web App
- **REQ-004**: Consume all existing API endpoints
- **REQ-005**: Use plain Blazor components (no third-party UI frameworks)
- **REQ-006**: Support offline/PWA scenarios
- **REQ-007**: Provide bilingual English and Welsh localization
- **REQ-008**: Implement automated UI/unit/integration tests
- **REQ-009**: Use Bootstrap, Bootstrap Icons, Google Fonts, and Font Awesome (free)
- **REQ-010**: Implement CI/CD pipeline for build, test, and deploy
- **SEC-001**: Secure authentication and API calls via Azure AD B2C
- **CON-001**: No file upload/large payload support in MVP
- **GUD-001**: Follow accessibility and WCAG 2.2 AA guidelines
- **PAT-001**: Use clean, modern UI patterns with responsive design

## 2. Implementation Steps

### Implementation Phase 1

- GOAL-001: Scaffold Blazor WASM client and configure Azure AD B2C authentication

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Scaffold Blazor WASM client project using .NET CLI |  |  |
| TASK-002 | Add Azure AD B2C authentication via .NET CLI |  |  |
| TASK-003 | Configure project for Azure Static Web Apps deployment |  |  |
| TASK-004 | Set up Bootstrap, Bootstrap Icons, Google Fonts, Font Awesome |  |  |
| TASK-005 | Implement basic routing and navigation |  |  |

### Implementation Phase 2

- GOAL-002: Integrate API endpoints, localization, PWA/offline support, and CI/CD/testing

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006 | Implement API service to consume all HL7ResultsGateway API endpoints |  |  |
| TASK-007 | Add bilingual English/Welsh localization (resource files, UI toggle) |  |  |
| TASK-008 | Implement offline/PWA support (service worker, manifest) |  |  |
| TASK-009 | Build clean, modern UI with accessibility features |  |  |
| TASK-010 | Set up automated UI/unit/integration tests |  |  |
| TASK-011 | Configure CI/CD pipeline for build, test, deploy to Azure Static Web Apps |  |  |

## 3. Alternatives

- **ALT-001**: Use third-party UI frameworks (e.g., MudBlazor, Radzen) — not chosen to keep dependencies minimal and use plain Blazor components
- **ALT-002**: Host client together with API — not chosen; static site hosting preferred for scalability and separation

## 4. Dependencies

- **DEP-001**: .NET 9 SDK
- **DEP-002**: Azure AD B2C tenant
- **DEP-003**: Azure Static Web Apps service
- **DEP-004**: HL7ResultsGateway API endpoints
- **DEP-005**: Bootstrap, Bootstrap Icons, Google Fonts, Font Awesome (free)

## 5. Files

- **FILE-001**: /src/HL7ResultsGateway.Client/ (new Blazor WASM project)
- **FILE-002**: /src/HL7ResultsGateway.Client/wwwroot/manifest.json (PWA)
- **FILE-003**: /src/HL7ResultsGateway.Client/wwwroot/service-worker.js (offline support)
- **FILE-004**: /src/HL7ResultsGateway.Client/Resources/ (localization)
- **FILE-005**: /src/HL7ResultsGateway.Client/Pages/ (UI pages)
- **FILE-006**: /src/HL7ResultsGateway.Client/Services/ApiService.cs (API integration)
- **FILE-007**: /tests/HL7ResultsGateway.Client/Tests/ (automated tests)
- **FILE-008**: /.github/workflows/client-ci.yml (CI/CD pipeline)

## 6. Testing

- **TEST-001**: Unit tests for API service and authentication
- **TEST-002**: UI tests for navigation, localization, and accessibility
- **TEST-003**: Integration tests for API calls and offline scenarios
- **TEST-004**: CI pipeline validation (build, test, deploy)

## 7. Risks & Assumptions

- **RISK-001**: Azure AD B2C configuration complexity may delay authentication setup
- **RISK-002**: Localization may require additional translation resources
- **RISK-003**: PWA/offline support may introduce caching issues
- **ASSUMPTION-001**: All API endpoints are stable and documented
- **ASSUMPTION-002**: Azure resources (AD B2C, Static Web Apps) are available

## 8. Related Specifications / Further Reading

- [Microsoft Docs: Blazor WebAssembly](https://learn.microsoft.com/en-us/aspnet/core/blazor/?view=aspnetcore-9.0)
- [Microsoft Docs: Azure Static Web Apps](https://learn.microsoft.com/en-us/azure/static-web-apps/)
- [Microsoft Docs: Azure AD B2C](https://learn.microsoft.com/en-us/azure/active-directory-b2c/overview)
- [WCAG 2.2 Guidelines](https://www.w3.org/TR/WCAG22/)
