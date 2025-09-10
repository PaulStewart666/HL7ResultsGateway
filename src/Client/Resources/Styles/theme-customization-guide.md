# Theme Customization Guide

## Overview

This guide explains how to customize, extend, and create new themes for the HL7 Results Gateway application. The theming system uses CSS custom properties (CSS variables) for dynamic theme switching and easy customization.

## Architecture Overview

### Theme Structure
```
themes/
├── light.css          # Default light theme
├── dark.css           # Dark theme for reduced eye strain
├── medical.css        # Healthcare-focused theme
└── custom.css         # Your custom theme
```

### CSS Custom Properties
Each theme defines CSS custom properties that are used throughout the application:

```css
body.theme-{name} {
  /* Core colors */
  --background-color: #ffffff;
  --text-color: #212529;
  --color-primary: #0d6efd;
  --color-secondary: #6c757d;
  
  /* Semantic colors */
  --color-success: #198754;
  --color-danger: #dc3545;
  --color-warning: #ffc107;
  --color-info: #0dcaf0;
  
  /* Typography */
  --font-family-base: 'Roboto', sans-serif;
  --font-family-heading: 'Roboto Slab', serif;
}
```

## Creating a Custom Theme

### Step 1: Create Theme File

Create a new CSS file in `src/Client/Resources/Styles/themes/`:

```css
/* themes/corporate.css */
body.theme-corporate {
  /* Brand colors */
  --background-color: #f8f9fa !important;
  --text-color: #2c3e50 !important;
  --color-primary: #3498db !important;
  --color-secondary: #95a5a6 !important;
  --color-success: #27ae60 !important;
  --color-danger: #e74c3c !important;
  --color-warning: #f39c12 !important;
  --color-info: #17a2b8 !important;
  
  /* Typography */
  --font-family-base: 'Open Sans', 'Helvetica Neue', sans-serif !important;
  --font-family-heading: 'Montserrat', sans-serif !important;
  
  /* Component-specific variables */
  --card-background: #ffffff !important;
  --navbar-background: #2c3e50 !important;
  --button-border-radius: 0.25rem !important;
  --box-shadow-subtle: 0 2px 4px rgba(0,0,0,0.1) !important;
  
  /* Direct styling for immediate effect */
  background-color: #f8f9fa !important;
  color: #2c3e50 !important;
}

/* Corporate theme component overrides */
body.theme-corporate .navbar {
  background-color: #2c3e50 !important;
  border-bottom: 3px solid #3498db !important;
}

body.theme-corporate .card {
  background-color: #ffffff !important;
  border: 1px solid #dee2e6 !important;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1) !important;
  color: #2c3e50 !important;
}

body.theme-corporate .btn-primary {
  background-color: #3498db !important;
  border-color: #3498db !important;
  border-radius: 0.25rem !important;
}

body.theme-corporate h1,
body.theme-corporate h2,
body.theme-corporate h3,
body.theme-corporate h4,
body.theme-corporate h5,
body.theme-corporate h6 {
  color: #2c3e50 !important;
  font-family: 'Montserrat', sans-serif !important;
}
```

### Step 2: Copy to wwwroot

Copy the theme file to the public directory:

```bash
# From src/Client/ directory
Copy-Item "Resources\Styles\themes\corporate.css" "wwwroot\css\themes\corporate.css"
```

### Step 3: Import in app.css

Add the import to `wwwroot/css/app.css`:

```css
/* Import all theme files */
@import url('variables.css');
@import url('themes/light.css');
@import url('themes/dark.css');
@import url('themes/medical.css');
@import url('themes/corporate.css'); /* Add your custom theme */
@import url('print.css');
@import url('accessibility.css');
```

### Step 4: Register Theme Service

Update the `ThemeService` to include the new theme:

```csharp
// Core/Theming/ThemeService.cs
public class ThemeService : IThemeService
{
    public IReadOnlyList<string> AvailableThemes { get; } = new[] 
    { 
        "light", 
        "dark", 
        "medical",
        "corporate" // Add your theme here
    };
    
    // ... rest of implementation
}
```

### Step 5: Update Theme Switcher

Add the new theme option to `ThemeSwitcher.razor`:

```html
<li>
  <button class="dropdown-item" type="button" 
          @onclick="() => SwitchTheme(\"corporate\")">
    <i class="fa fa-building me-2" aria-hidden="true"></i>
    Corporate Theme
  </button>
</li>
```

## Advanced Customization

### Component-Specific Theming

#### Custom Card Styles
```css
body.theme-corporate .dashboard-card {
  background: linear-gradient(135deg, #ffffff 0%, #f8f9fa 100%) !important;
  border-left: 4px solid #3498db !important;
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

body.theme-corporate .dashboard-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(52, 152, 219, 0.15) !important;
}
```

#### Custom Form Styling
```css
body.theme-corporate .form-control {
  border-radius: 0.25rem !important;
  border: 2px solid #dee2e6 !important;
  transition: border-color 0.15s ease, box-shadow 0.15s ease;
}

body.theme-corporate .form-control:focus {
  border-color: #3498db !important;
  box-shadow: 0 0 0 0.2rem rgba(52, 152, 219, 0.25) !important;
}
```

#### Custom Button Variations
```css
body.theme-corporate .btn {
  border-radius: 0.25rem !important;
  font-weight: 500;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

body.theme-corporate .btn-primary {
  background: linear-gradient(45deg, #3498db, #2980b9) !important;
  border: none !important;
}

body.theme-corporate .btn-primary:hover {
  background: linear-gradient(45deg, #2980b9, #21618c) !important;
  transform: translateY(-1px);
}
```

### Typography Customization

#### Custom Font Loading
Add custom fonts to `index.html`:

```html
<!-- Custom Google Fonts for corporate theme -->
<link rel="preconnect" href="https://fonts.googleapis.com">
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
<link href="https://fonts.googleapis.com/css2?family=Montserrat:wght@400;500;600;700&family=Open+Sans:wght@300;400;500;600&display=swap" rel="stylesheet">
```

#### Font Variables
```css
body.theme-corporate {
  --font-family-base: 'Open Sans', -apple-system, BlinkMacSystemFont, sans-serif !important;
  --font-family-heading: 'Montserrat', sans-serif !important;
  --font-family-mono: 'SF Mono', Consolas, 'Liberation Mono', monospace !important;
  
  /* Font size scale */
  --font-size-xs: 0.75rem;
  --font-size-sm: 0.875rem;
  --font-size-base: 1rem;
  --font-size-lg: 1.125rem;
  --font-size-xl: 1.25rem;
  
  /* Line heights */
  --line-height-tight: 1.25;
  --line-height-base: 1.5;
  --line-height-relaxed: 1.75;
}
```

## Color System Customization

### Brand Color Palette
```css
body.theme-corporate {
  /* Primary brand colors */
  --brand-primary: #3498db;
  --brand-secondary: #2c3e50;
  --brand-accent: #e74c3c;
  
  /* Neutral palette */
  --gray-50: #f8f9fa;
  --gray-100: #e9ecef;
  --gray-200: #dee2e6;
  --gray-300: #ced4da;
  --gray-400: #adb5bd;
  --gray-500: #6c757d;
  --gray-600: #495057;
  --gray-700: #343a40;
  --gray-800: #212529;
  --gray-900: #000000;
  
  /* Semantic color mapping */
  --color-primary: var(--brand-primary);
  --color-secondary: var(--brand-secondary);
  --background-color: var(--gray-50);
  --text-color: var(--gray-800);
  --text-muted: var(--gray-600);
  --border-color: var(--gray-300);
}
```

### Accessibility-Compliant Colors
Ensure color contrast meets WCAG 2.2 AA standards:

```css
/* Test color combinations for accessibility */
body.theme-corporate {
  /* High contrast combinations (4.5:1 or higher) */
  --text-on-primary: #ffffff;    /* White on #3498db = 5.1:1 */
  --text-on-secondary: #ffffff;  /* White on #2c3e50 = 12.6:1 */
  --text-on-light: #212529;      /* Dark on light = 16.7:1 */
  
  /* Link colors with sufficient contrast */
  --link-color: #2980b9;         /* Darker blue for better contrast */
  --link-hover-color: #21618c;   /* Even darker on hover */
}
```

## Responsive Theme Customization

### Breakpoint-Specific Styling
```css
/* Mobile-first approach */
body.theme-corporate {
  --container-padding: 1rem;
  --card-margin: 0.5rem;
}

/* Tablet and up */
@media (min-width: 768px) {
  body.theme-corporate {
    --container-padding: 2rem;
    --card-margin: 1rem;
  }
}

/* Desktop and up */
@media (min-width: 1200px) {
  body.theme-corporate {
    --container-padding: 3rem;
    --card-margin: 1.5rem;
  }
}
```

### Component Responsive Behavior
```css
body.theme-corporate .navbar-brand {
  font-size: 1.25rem;
}

@media (min-width: 768px) {
  body.theme-corporate .navbar-brand {
    font-size: 1.5rem;
  }
}

body.theme-corporate .card {
  margin: var(--card-margin);
  padding: 1rem;
}

@media (min-width: 992px) {
  body.theme-corporate .card {
    padding: 1.5rem;
  }
}
```

## Dark Mode Variations

### Auto Dark Mode Theme
Create a theme that responds to system preferences:

```css
/* Auto theme that follows system preference */
body.theme-auto {
  /* Light mode colors (default) */
  --background-color: #ffffff;
  --text-color: #212529;
  --color-primary: #0d6efd;
}

/* Dark mode when system preference is dark */
@media (prefers-color-scheme: dark) {
  body.theme-auto {
    --background-color: #212529;
    --text-color: #f8f9fa;
    --color-primary: #6ea8fe;
  }
}
```

### High Contrast Mode Support
```css
/* High contrast mode support */
@media (prefers-contrast: high) {
  body.theme-corporate {
    --color-primary: #0000ff;
    --color-danger: #ff0000;
    --text-color: #000000;
    --background-color: #ffffff;
    --border-color: #000000;
  }
  
  body.theme-corporate .card {
    border: 2px solid #000000 !important;
  }
  
  body.theme-corporate .btn {
    border: 2px solid currentColor !important;
  }
}
```

## Animation and Transition Customization

### Theme Switching Animations
```css
body.theme-corporate {
  /* Smooth transitions for theme switching */
  transition: 
    background-color 0.3s cubic-bezier(0.4, 0, 0.2, 1),
    color 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

body.theme-corporate * {
  transition: 
    background-color 0.3s cubic-bezier(0.4, 0, 0.2, 1),
    border-color 0.3s cubic-bezier(0.4, 0, 0.2, 1),
    color 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

/* Micro-interactions */
body.theme-corporate .btn {
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
}

body.theme-corporate .card {
  transition: 
    transform 0.2s cubic-bezier(0.4, 0, 0.2, 1),
    box-shadow 0.2s cubic-bezier(0.4, 0, 0.2, 1);
}
```

## Testing Your Theme

### Manual Testing Checklist
- [ ] All text has sufficient contrast (4.5:1 minimum)
- [ ] Interactive elements are clearly visible
- [ ] Focus indicators work properly
- [ ] Theme switching is smooth
- [ ] All components render correctly
- [ ] Mobile responsiveness maintained
- [ ] Print styles work (if applicable)

### Automated Testing
```bash
# Test color contrast with axe-core
npx @axe-core/cli https://localhost:7276 --tags wcag2a,wcag2aa

# Test with Lighthouse
lighthouse https://localhost:7276 --only-categories=accessibility
```

### Cross-Browser Testing
Test your theme in:
- Chrome/Chromium
- Firefox
- Safari (if on macOS)
- Edge

## Performance Considerations

### CSS Optimization
```css
/* Use efficient selectors */
body.theme-corporate .component {  /* Good */
  /* styles */
}

body.theme-corporate div > span + p {  /* Avoid complex selectors */
  /* styles */
}

/* Group related properties */
body.theme-corporate .card {
  /* Positioning */
  position: relative;
  
  /* Box model */
  margin: 1rem;
  padding: 1.5rem;
  border: 1px solid var(--border-color);
  
  /* Visual */
  background-color: var(--card-background);
  box-shadow: var(--box-shadow-subtle);
  
  /* Typography */
  color: var(--text-color);
  font-family: var(--font-family-base);
  
  /* Other */
  transition: transform 0.2s ease;
}
```

### File Size Management
- Keep theme files under 10KB each
- Use CSS custom properties to avoid duplication
- Minimize use of `!important`
- Group related styles together

## Theme Distribution

### Packaging Themes
Create a theme package for distribution:

```json
{
  "name": "hl7-corporate-theme",
  "version": "1.0.0",
  "description": "Corporate theme for HL7 Results Gateway",
  "files": [
    "corporate.css",
    "corporate-icons.css",
    "README.md"
  ],
  "author": "Your Organization",
  "license": "MIT"
}
```

### Installation Instructions
```markdown
# Corporate Theme Installation

1. Copy `corporate.css` to `src/Client/Resources/Styles/themes/`
2. Copy to `wwwroot/css/themes/corporate.css`
3. Add import to `app.css`
4. Register in `ThemeService`
5. Update `ThemeSwitcher` component
6. Test thoroughly across browsers and devices
```

## Troubleshooting

### Common Issues

#### Theme Not Applying
- Check CSS import order in `app.css`
- Verify file is copied to `wwwroot/css/themes/`
- Ensure theme name matches in all files
- Check browser developer tools for CSS errors

#### Insufficient Specificity
- Add `!important` to critical properties
- Use more specific selectors
- Check Bootstrap override order

#### Performance Issues
- Minimize complex selectors
- Reduce use of expensive properties (box-shadow, gradients)
- Optimize transition properties

### Debug Tools
```css
/* Temporary debug styles */
body.theme-corporate * {
  outline: 1px solid red !important; /* Visualize all elements */
}

body.theme-corporate [class*="theme-"] {
  background: yellow !important; /* Highlight themed elements */
}
```

---

## Resources

### Color Tools
- **Coolors.co**: Color palette generator
- **Contrast Ratio**: WebAIM contrast checker
- **Adobe Color**: Professional color wheel

### Typography Resources
- **Google Fonts**: Free web fonts
- **Font Pair**: Font combination suggestions
- **Modular Scale**: Typography scale calculator

### Accessibility Testing
- **axe DevTools**: Browser extension
- **WAVE**: Web accessibility evaluator
- **Colour Contrast Analyser**: Desktop application

---

*Last updated: September 10, 2025*
