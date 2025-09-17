---
goal: Add GPW Branded Default Theme (colors, logo palette) and register in client theming system
version: 1.0
date_created: 2025-09-15
last_updated: 2025-09-15
owner: UI/UX & Frontend
status: Completed
tags: [feature, theming, ui]
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Implement a new default theme named `gpw` (Genomics Partnership Wales) aligned with provided logo palette (deep teal #0f2f39, light teal #6ab9b2, white, supportive mid teal #2d8f83). Make it the default selected theme, add stylesheet, expose in `ThemeService.AvailableThemes`, update icon mapping, and ensure accessibility (contrast > 4.5:1 for text vs background). No logo file integration required at this stage—color system only.

## 1. Requirements & Constraints

- **REQ-001**: Provide new CSS theme file at `src/Client/wwwroot/css/themes/gpw.css`.
- **REQ-002**: Theme variables must mirror existing pattern (background, text, semantic colors, font families).
- **REQ-003**: Register `gpw` in `ThemeService.AvailableThemes` as first entry and default.
- **REQ-004**: Update `ThemeSwitcher` icon mapping to show distinct icon (use FontAwesome `fa-dna`).
- **REQ-005**: Maintain WCAG AA contrast for primary text (#0f2f39 on white, white on #0f2f39 navbar).
- **REQ-006**: Preserve existing themes (light, dark, medical) unchanged.
- **REQ-007**: Fallback logic on JS failure must default to `gpw`.
- **SEC-001**: No introduction of external network calls or insecure content.
- **A11-001**: Focus outlines must remain visible (add explicit focus outline with acceptable contrast #6ab9b2 on white & dark surfaces).
- **CON-001**: Do not modify global variable file structure; additive only.
- **GUD-001**: Use consistent naming prefix `theme-gpw` per existing convention.
- **PAT-001**: Follow existing theme CSS specificity pattern `body.theme-<name>`.

## 2. Implementation Steps

### Implementation Phase 1

- GOAL-001: Add GPW theme assets and service registration.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `gpw.css` with brand palette and component overrides | ✅ | 2025-09-15 |
| TASK-002 | Update `ThemeService` default `_currentTheme` to `gpw` & add to `AvailableThemes` | ✅ | 2025-09-15 |
| TASK-003 | Adjust fallback logic to default to `gpw` | ✅ | 2025-09-15 |

### Implementation Phase 2

- GOAL-002: UI integration & accessibility adjustments.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-004 | Update `ThemeSwitcher` icon mapping with `gpw` => `fa-dna` | ✅ | 2025-09-15 |
| TASK-005 | Add focus outline & link hover color in `gpw.css` | ✅ | 2025-09-15 |
| TASK-006 | Verify contrast (manual check > 4.5:1) for primary text and link states | ✅ | 2025-09-15 |

## 3. Alternatives

- **ALT-001**: Override variables in existing `light.css` (rejected: breaks separation of concerns, harder to toggle).
- **ALT-002**: Implement CSS variable theme only via root toggling (rejected: current pattern relies on body-scoped classes with direct overrides for specificity vs Bootstrap).

## 4. Dependencies

- **DEP-001**: Existing FontAwesome icon set (already present) for `fa-dna`.
- **DEP-002**: Bootstrap baseline styles (unchanged).

## 5. Files

- **FILE-001**: `src/Client/wwwroot/css/themes/gpw.css` (new stylesheet).
- **FILE-002**: `src/Client/Core/Theming/ThemeService.cs` (theme registration & default change).
- **FILE-003**: `src/Client/Features/Shared/Components/ThemeSwitcher.razor` (icon mapping update).

## 6. Testing

- **TEST-001**: Launch client; confirm `body` has `theme-gpw` class after initialization.
- **TEST-002**: Switch to other themes via UI then back to GPW; confirm persistence across refresh (LocalStorage via existing JS interop).
- **TEST-003**: Inspect computed styles: headings use `--color-primary` (#0f2f39), links hover to #4da199.
- **TEST-004**: Accessibility: Focus outline visible on buttons/links in GPW theme.

## 7. Risks & Assumptions

- **RISK-001**: Potential icon mismatch if `fa-dna` not loaded—fallback handled by existing generic palette icon mapping default.
- **ASSUMPTION-001**: FontAwesome already loaded in application (observed in existing code).

## 8. Related Specifications / Further Reading

- Existing theming guide: `src/Client/Resources/Styles/theme-customization-guide.md`
- Accessibility instructions in repo `.github/awesome-copilot/instructions/a11y.instructions.md`
