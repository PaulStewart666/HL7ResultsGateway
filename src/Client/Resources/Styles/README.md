# HL7 Results Gateway Design System

![Bootstrap 5](https://img.shields.io/badge/Bootstrap-5.3.2-7952b3?logo=bootstrap) ![Accessibility](https://img.shields.io/badge/WCAG-2.2%20AA-green) ![Responsive](https://img.shields.io/badge/Responsive-Mobile%20First-blue)

## Overview

The HL7 Results Gateway design system is built on Bootstrap 5 with custom theming capabilities, accessibility compliance (WCAG 2.2 AA), and responsive design principles. This documentation provides guidelines for using and extending the design system.

## Table of Contents

- [Architecture](#architecture)
- [Theming System](#theming-system)
- [Typography](#typography)
- [Color System](#color-system)
- [Components](#components)
- [Responsive Design](#responsive-design)
- [Accessibility](#accessibility)
- [Performance](#performance)
- [Usage Examples](#usage-examples)
- [Customization](#customization)

## Architecture

### File Structure
```
Resources/Styles/
├── variables.css          # CSS custom properties
├── themes/
│   ├── light.css         # Light theme
│   ├── dark.css          # Dark theme
│   └── medical.css       # Medical theme
├── components.css        # Component-specific styles
├── critical.css          # Above-the-fold critical CSS
├── animations.css        # Micro-interactions and animations
├── print.css            # Print-specific styles
├── accessibility.css    # Accessibility enhancements
wwwroot/css/
├── app.css              # Main application stylesheet
└── themes/              # Production theme files
```

### CSS Loading Strategy
1. **Critical CSS**: Inlined in `index.html` for immediate rendering
2. **Bootstrap CDN**: Loaded from CDN with preload hints
3. **Theme CSS**: Dynamically loaded based on user preference
4. **Component CSS**: Scoped styles using Blazor CSS isolation

## Theming System

### Theme Architecture
Our theming system uses CSS custom properties (CSS variables) for dynamic theme switching:

```css
body.theme-light {
  --background-color: #ffffff;
  --text-color: #212529;
  --color-primary: #0d6efd;
  /* ... more variables */
}

body.theme-dark {
  --background-color: #212529;
  --text-color: #f8f9fa;
  --color-primary: #6ea8fe;
  /* ... more variables */
}
```

### Available Themes
- **Light Theme** (`theme-light`): Clean, professional appearance
- **Dark Theme** (`theme-dark`): Reduced eye strain, modern look
- **Medical Theme** (`theme-medical`): Healthcare-focused green color scheme

### Theme Switching
Themes are managed via the `ThemeService` and persisted in localStorage:

```csharp
@inject IThemeService ThemeService

// Switch theme
await ThemeService.SetThemeAsync("dark");

// Get current theme
var currentTheme = ThemeService.CurrentTheme;
```

## Typography

### Font Families
- **Body Text**: Roboto, Inter, system fonts
- **Headings**: Roboto Slab, serif fallbacks
- **Monospace**: Consolas, Monaco, 'Courier New'

### Font Scales
```css
:root {
  --font-size-xs: 0.75rem;    /* 12px */
  --font-size-sm: 0.875rem;   /* 14px */
  --font-size-base: 1rem;     /* 16px */
  --font-size-lg: 1.125rem;   /* 18px */
  --font-size-xl: 1.25rem;    /* 20px */
  --font-size-2xl: 1.5rem;    /* 24px */
  --font-size-3xl: 1.875rem;  /* 30px */
  --font-size-4xl: 2.25rem;   /* 36px */
}
```

### Usage Examples
```html
<h1 class="display-4">Main Heading</h1>
<h2 class="h3 text-primary">Section Heading</h2>
<p class="lead">Lead paragraph text</p>
<small class="text-muted">Helper text</small>
```

## Color System

### Primary Colors
| Theme | Primary | Secondary | Success | Danger | Warning | Info |
|-------|---------|-----------|---------|--------|---------|------|
| Light | #0d6efd | #6c757d | #198754 | #dc3545 | #ffc107 | #0dcaf0 |
| Dark | #6ea8fe | #a7acb1 | #75b798 | #ea868f | #ffda6a | #6edff6 |
| Medical | #28a745 | #17a2b8 | #28a745 | #dc3545 | #ffc107 | #17a2b8 |

### Semantic Colors
- Use `--color-primary` for primary actions and branding
- Use `--color-success` for positive states and confirmations
- Use `--color-danger` for errors and destructive actions
- Use `--color-warning` for caution and important notices
- Use `--color-info` for informational content

## Components

### Cards
```html
<div class="card">
  <div class="card-header">
    <h5 class="card-title">Card Title</h5>
  </div>
  <div class="card-body">
    <p class="card-text">Card content goes here.</p>
    <button class="btn btn-primary">Action</button>
  </div>
</div>
```

### Buttons
```html
<!-- Primary actions -->
<button class="btn btn-primary">Primary</button>
<button class="btn btn-outline-primary">Secondary</button>

<!-- Semantic states -->
<button class="btn btn-success">Success</button>
<button class="btn btn-danger">Danger</button>
<button class="btn btn-warning">Warning</button>
```

### Forms
```html
<div class="mb-3">
  <label for="email" class="form-label">Email</label>
  <input type="email" class="form-control" id="email" required>
  <div class="invalid-feedback">Please provide a valid email.</div>
</div>
```

### Navigation
```html
<nav class="navbar navbar-expand-lg navbar-light">
  <div class="container-fluid">
    <a class="navbar-brand" href="/">
      <i class="fa fa-notes-medical" aria-label="HL7 Medical"></i>
      HL7 Results Gateway
    </a>
    <!-- Navigation items -->
  </div>
</nav>
```

## Responsive Design

### Breakpoints
```css
/* Bootstrap 5 Breakpoints */
--bs-breakpoint-xs: 0;
--bs-breakpoint-sm: 576px;
--bs-breakpoint-md: 768px;
--bs-breakpoint-lg: 992px;
--bs-breakpoint-xl: 1200px;
--bs-breakpoint-xxl: 1400px;
```

### Mobile-First Approach
All styles are written mobile-first, then enhanced for larger screens:

```css
/* Mobile styles first */
.card {
  margin: 1rem 0;
}

/* Tablet and up */
@media (min-width: 768px) {
  .card {
    margin: 1.5rem 0;
  }
}
```

### Grid System
Use Bootstrap's flexbox grid system:

```html
<div class="container">
  <div class="row">
    <div class="col-12 col-md-8 col-lg-6">
      <!-- Content -->
    </div>
    <div class="col-12 col-md-4 col-lg-6">
      <!-- Sidebar -->
    </div>
  </div>
</div>
```

## Accessibility

### WCAG 2.2 AA Compliance
- **Color Contrast**: 4.5:1 for normal text, 3:1 for large text
- **Focus Indicators**: Visible focus outlines on all interactive elements
- **Semantic HTML**: Proper heading hierarchy and landmark elements
- **ARIA Labels**: Descriptive labels for screen readers
- **Keyboard Navigation**: All functionality accessible via keyboard

### Implementation Examples
```html
<!-- Semantic landmarks -->
<header role="banner">
<nav role="navigation" aria-label="Main navigation">
<main role="main">
<aside role="complementary">
<footer role="contentinfo">

<!-- Skip links -->
<a href="#main-content" class="sr-only sr-only-focusable">
  Skip to main content
</a>

<!-- ARIA labels -->
<button aria-label="Close dialog" aria-expanded="false">
  <i class="fa fa-times" aria-hidden="true"></i>
</button>

<!-- Form accessibility -->
<label for="username">Username</label>
<input type="text" id="username" aria-describedby="username-help">
<div id="username-help" class="form-text">
  Enter your username (required)
</div>
```

### Focus Management
```css
/* Custom focus indicators */
:focus {
  outline: 2px solid var(--color-primary);
  outline-offset: 2px;
}

/* Skip links */
.sr-only-focusable:focus {
  position: absolute;
  top: 0;
  left: 0;
  z-index: 1050;
  padding: 0.5rem 1rem;
  background: var(--color-primary);
  color: white;
  text-decoration: none;
}
```

## Performance

### Critical CSS Strategy
1. **Inline Critical CSS**: Above-the-fold styles inlined in HTML
2. **Async CSS Loading**: Non-critical CSS loaded asynchronously
3. **CDN Usage**: Bootstrap and icons loaded from CDN with preload hints
4. **CSS Optimization**: Minified CSS in production builds

### Loading Performance
```html
<!-- Preload critical resources -->
<link rel="preload" as="style" href="bootstrap.min.css">
<link rel="preload" as="font" href="roboto.woff2" crossorigin>

<!-- Critical CSS inlined -->
<style>
  /* Critical above-the-fold styles */
</style>

<!-- Async load non-critical CSS -->
<link rel="stylesheet" href="app.css" media="print" onload="this.media='all'">
```

## Usage Examples

### Theme-Aware Components
```csharp
@using HL7ResultsGateway.Client.Core.Theming
@inject IThemeService ThemeService
@implements IDisposable

<div class="theme-aware-component">
  <h3>Current Theme: @ThemeService.CurrentTheme</h3>
  <button @onclick="SwitchToDark">Dark Mode</button>
</div>

@code {
  protected override void OnInitialized()
  {
    ThemeService.ThemeChanged += OnThemeChanged;
  }

  private async Task SwitchToDark()
  {
    await ThemeService.SetThemeAsync("dark");
  }

  private void OnThemeChanged(string newTheme)
  {
    InvokeAsync(StateHasChanged);
  }

  public void Dispose()
  {
    ThemeService.ThemeChanged -= OnThemeChanged;
  }
}
```

### Responsive Card Grid
```html
<div class="container-fluid">
  <div class="row g-4">
    @foreach (var item in items)
    {
      <div class="col-12 col-sm-6 col-lg-4 col-xl-3">
        <div class="card h-100">
          <div class="card-body d-flex flex-column">
            <h5 class="card-title">@item.Title</h5>
            <p class="card-text flex-grow-1">@item.Description</p>
            <button class="btn btn-primary mt-auto">View Details</button>
          </div>
        </div>
      </div>
    }
  </div>
</div>
```

## Customization

### Adding New Themes
1. Create new theme file: `themes/custom.css`
2. Define CSS custom properties:
```css
body.theme-custom {
  --background-color: #your-bg-color;
  --text-color: #your-text-color;
  --color-primary: #your-primary-color;
  /* ... other variables */
}
```
3. Add to `ThemeService.AvailableThemes`
4. Import in `app.css`

### Extending Color Palette
```css
:root {
  /* Add custom semantic colors */
  --color-brand: #your-brand-color;
  --color-accent: #your-accent-color;
  
  /* Add custom component colors */
  --card-shadow: rgba(0, 0, 0, 0.1);
  --border-subtle: rgba(0, 0, 0, 0.06);
}
```

### Custom Component Patterns
```css
/* Follow BEM naming convention */
.component-name {
  /* Base component styles */
}

.component-name__element {
  /* Element styles */
}

.component-name--modifier {
  /* Modifier styles */
}

/* Use CSS custom properties for theming */
.custom-component {
  background-color: var(--background-color);
  border: 1px solid var(--border-color, rgba(0, 0, 0, 0.1));
  color: var(--text-color);
}
```

---

## Development Guidelines

### CSS Best Practices
1. Use CSS custom properties for themeable values
2. Follow mobile-first responsive design
3. Implement proper focus indicators
4. Use semantic HTML5 elements
5. Test with screen readers and keyboard navigation
6. Maintain consistent spacing using Bootstrap's spacing utilities
7. Use utility classes before writing custom CSS
8. Keep specificity low to avoid override conflicts

### Component Development
1. Use Blazor CSS isolation for component-specific styles
2. Implement proper ARIA labels and roles
3. Test across all available themes
4. Ensure responsive behavior on all screen sizes
5. Document component usage and examples

### Testing Checklist
- [ ] Cross-browser compatibility (Chrome, Firefox, Safari, Edge)
- [ ] Responsive design on mobile, tablet, desktop
- [ ] Theme switching functionality
- [ ] Accessibility compliance (WCAG 2.2 AA)
- [ ] Keyboard navigation
- [ ] Screen reader compatibility
- [ ] Color contrast ratios
- [ ] Print stylesheet functionality
- [ ] Performance metrics (loading times, rendering)

---

*Last updated: September 10, 2025*
*Version: 1.0*
