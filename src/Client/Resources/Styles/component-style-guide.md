# Component Style Guide

## Overview

This style guide provides examples and usage patterns for all UI components in the HL7 Results Gateway application. Follow these patterns for consistent design and accessibility.

## Design Principles

- **Consistency**: Use established patterns and components
- **Accessibility**: WCAG 2.2 AA compliance in all components
- **Responsiveness**: Mobile-first, adaptive design
- **Performance**: Optimized CSS and minimal DOM manipulation
- **Theming**: All components work with light, dark, and medical themes

## Layout Components

### Main Layout

#### Structure
```html
<!-- MainLayout.razor -->
<header class="theme-header" role="banner">
  <nav class="navbar navbar-expand-lg navbar-light" role="navigation">
    <!-- Navigation content -->
  </nav>
</header>

<main id="maincontent" class="container py-4" role="main">
  @Body
</main>

<div aria-live="polite" class="position-fixed top-0 end-0 p-3">
  <!-- Toast notifications -->
</div>
```

#### CSS Classes
- `.theme-header`: Header container with theme support
- `.navbar`: Bootstrap navigation with custom theming
- `.container`: Main content container with responsive padding

### Navigation Bar

#### Implementation
```html
<nav class="navbar navbar-expand-lg navbar-light bg-light" aria-label="Main navigation">
  <div class="container-fluid">
    <!-- Brand -->
    <a class="navbar-brand d-flex align-items-center gap-2" href="/">
      <span class="fa fa-notes-medical" role="img" aria-label="HL7 Medical"></span>
      <span class="fw-bold">HL7 Results Gateway</span>
    </a>
    
    <!-- Mobile toggle -->
    <button class="navbar-toggler" type="button" 
            data-bs-toggle="collapse" data-bs-target="#navbarNav"
            aria-controls="navbarNav" aria-expanded="false" 
            aria-label="Toggle navigation">
      <span class="navbar-toggler-icon"></span>
    </button>
    
    <!-- Navigation items -->
    <div class="collapse navbar-collapse" id="navbarNav">
      <ul class="navbar-nav me-auto mb-2 mb-lg-0">
        <li class="nav-item">
          <a class="nav-link active" aria-current="page" href="/">Home</a>
        </li>
        <li class="nav-item">
          <a class="nav-link" href="/dashboard">Dashboard</a>
        </li>
      </ul>
      
      <!-- Right-aligned items -->
      <div class="d-flex align-items-center gap-3">
        <LoginDisplay />
        <ThemeSwitcher />
      </div>
    </div>
  </div>
</nav>
```

#### Accessibility Features
- `role="navigation"` for screen readers
- `aria-label` for navigation context
- `aria-current="page"` for active navigation
- `aria-controls` and `aria-expanded` for mobile toggle

## Data Display Components

### Cards

#### Basic Card
```html
<div class="card h-100">
  <div class="card-header d-flex justify-content-between align-items-center">
    <h5 class="card-title mb-0">
      <i class="fa fa-chart-line me-2" aria-hidden="true"></i>
      Card Title
    </h5>
    <small class="text-muted">Optional subtitle</small>
  </div>
  <div class="card-body">
    <p class="card-text">Card content goes here with proper semantic structure.</p>
    <div class="d-flex gap-2 mt-auto">
      <button class="btn btn-primary" type="button">Primary Action</button>
      <button class="btn btn-outline-secondary" type="button">Secondary</button>
    </div>
  </div>
</div>
```

#### Dashboard Card (Responsive Grid)
```html
<div class="container-fluid">
  <div class="row g-4">
    <div class="col-12 col-sm-6 col-lg-4 col-xl-3">
      <div class="card h-100 dashboard-card">
        <div class="card-body d-flex flex-column">
          <div class="d-flex align-items-center mb-3">
            <div class="icon-container me-3">
              <i class="fa fa-users fa-2x text-primary" aria-hidden="true"></i>
            </div>
            <div>
              <h6 class="card-subtitle mb-1 text-muted">Total Patients</h6>
              <h3 class="card-title mb-0">1,234</h3>
            </div>
          </div>
          <p class="card-text flex-grow-1">Active patients in system</p>
          <small class="text-success">
            <i class="fa fa-arrow-up" aria-hidden="true"></i>
            12% increase this month
          </small>
        </div>
      </div>
    </div>
  </div>
</div>
```

#### CSS Enhancements
```css
/* Dashboard card styling */
.dashboard-card {
  transition: transform 0.2s ease, box-shadow 0.2s ease;
  border: 1px solid var(--border-color, rgba(0,0,0,0.125));
}

.dashboard-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0,0,0,0.15);
}

.icon-container {
  width: 60px;
  height: 60px;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: var(--color-primary-light, rgba(13, 110, 253, 0.1));
  border-radius: 50%;
}
```

### Tables

#### Responsive Table
```html
<div class="table-responsive">
  <table class="table table-striped table-hover" role="table">
    <caption class="sr-only">HL7 Message Processing Results</caption>
    <thead class="table-dark">
      <tr>
        <th scope="col">
          <button class="btn btn-link p-0 text-white" 
                  aria-label="Sort by Message ID">
            Message ID <i class="fa fa-sort" aria-hidden="true"></i>
          </button>
        </th>
        <th scope="col">Patient</th>
        <th scope="col">Status</th>
        <th scope="col">Date</th>
        <th scope="col" class="text-end">Actions</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td data-label="Message ID">
          <code class="user-select-all">MSG-001</code>
        </td>
        <td data-label="Patient">
          <div class="d-flex align-items-center">
            <div class="avatar me-2">
              <span class="fa fa-user" aria-hidden="true"></span>
            </div>
            <div>
              <div class="fw-semibold">John Doe</div>
              <small class="text-muted">ID: 12345</small>
            </div>
          </div>
        </td>
        <td data-label="Status">
          <span class="badge bg-success" role="status" aria-label="Processing completed successfully">
            <i class="fa fa-check" aria-hidden="true"></i> Success
          </span>
        </td>
        <td data-label="Date">
          <time datetime="2025-09-10T14:30:00Z">Sep 10, 2025 2:30 PM</time>
        </td>
        <td data-label="Actions" class="text-end">
          <div class="btn-group" role="group" aria-label="Message actions">
            <button class="btn btn-sm btn-outline-primary" 
                    type="button" aria-label="View message details">
              <i class="fa fa-eye" aria-hidden="true"></i>
            </button>
            <button class="btn btn-sm btn-outline-secondary" 
                    type="button" aria-label="Download message">
              <i class="fa fa-download" aria-hidden="true"></i>
            </button>
          </div>
        </td>
      </tr>
    </tbody>
  </table>
</div>
```

#### Mobile Table Styles
```css
/* Mobile table responsiveness */
@media (max-width: 767px) {
  .table-responsive table,
  .table-responsive thead,
  .table-responsive tbody,
  .table-responsive th,
  .table-responsive td,
  .table-responsive tr {
    display: block;
  }
  
  .table-responsive thead tr {
    position: absolute;
    top: -9999px;
    left: -9999px;
  }
  
  .table-responsive tr {
    border: 1px solid var(--border-color);
    margin-bottom: 0.5rem;
    border-radius: 0.375rem;
    padding: 1rem;
  }
  
  .table-responsive td {
    border: none;
    position: relative;
    padding-left: 40%;
  }
  
  .table-responsive td:before {
    content: attr(data-label) ": ";
    position: absolute;
    left: 6px;
    width: 35%;
    padding-right: 10px;
    white-space: nowrap;
    font-weight: 600;
  }
}
```

## Form Components

### Form Fields

#### Text Input with Validation
```html
<div class="mb-3">
  <label for="patient-id" class="form-label required">
    Patient ID
    <span class="text-danger" aria-label="required">*</span>
  </label>
  <input type="text" class="form-control" id="patient-id" 
         placeholder="Enter patient ID"
         aria-describedby="patient-id-help patient-id-error"
         required>
  <div id="patient-id-help" class="form-text">
    Enter the unique patient identifier (e.g., P12345)
  </div>
  <div id="patient-id-error" class="invalid-feedback" role="alert">
    Patient ID is required and must be in correct format
  </div>
</div>
```

#### Select Dropdown
```html
<div class="mb-3">
  <label for="message-type" class="form-label">Message Type</label>
  <select class="form-select" id="message-type" 
          aria-describedby="message-type-help">
    <option value="" selected disabled>Choose message type...</option>
    <option value="ORU_R01">ORU^R01 - Observation Result</option>
    <option value="ADT_A08">ADT^A08 - Patient Update</option>
    <option value="DFT_P03">DFT^P03 - Financial Transaction</option>
  </select>
  <div id="message-type-help" class="form-text">
    Select the HL7 message type to process
  </div>
</div>
```

#### Checkbox Group
```html
<fieldset class="mb-3">
  <legend class="form-label">Processing Options</legend>
  <div class="form-check">
    <input class="form-check-input" type="checkbox" id="validate-structure">
    <label class="form-check-label" for="validate-structure">
      Validate message structure
    </label>
  </div>
  <div class="form-check">
    <input class="form-check-input" type="checkbox" id="store-results">
    <label class="form-check-label" for="store-results">
      Store processing results
    </label>
  </div>
  <div class="form-check">
    <input class="form-check-input" type="checkbox" id="send-notifications">
    <label class="form-check-label" for="send-notifications">
      Send completion notifications
    </label>
  </div>
</fieldset>
```

### File Upload Component

```html
<div class="mb-3">
  <label for="hl7-file" class="form-label">Upload HL7 File</label>
  <div class="file-upload-area" 
       ondrop="handleDrop(event)" 
       ondragover="handleDragOver(event)"
       ondragleave="handleDragLeave(event)">
    <input type="file" class="form-control" id="hl7-file" 
           accept=".hl7,.txt,.dat" 
           aria-describedby="file-help">
    <div class="file-upload-text">
      <i class="fa fa-cloud-upload-alt fa-2x mb-2" aria-hidden="true"></i>
      <p class="mb-1">Drag and drop HL7 file here or click to browse</p>
      <small class="text-muted">Supported formats: .hl7, .txt, .dat</small>
    </div>
  </div>
  <div id="file-help" class="form-text">
    Maximum file size: 10MB. File will be validated upon upload.
  </div>
</div>
```

```css
/* File upload styling */
.file-upload-area {
  border: 2px dashed var(--border-color, #dee2e6);
  border-radius: 0.375rem;
  padding: 2rem;
  text-align: center;
  transition: border-color 0.15s ease;
  position: relative;
  background-color: var(--background-color-subtle, #f8f9fa);
}

.file-upload-area:hover,
.file-upload-area.dragover {
  border-color: var(--color-primary, #0d6efd);
  background-color: var(--color-primary-light, rgba(13, 110, 253, 0.05));
}

.file-upload-area input[type="file"] {
  position: absolute;
  inset: 0;
  opacity: 0;
  cursor: pointer;
}
```

## Interactive Components

### Buttons

#### Button Variations
```html
<!-- Primary actions -->
<button type="button" class="btn btn-primary">
  <i class="fa fa-save me-2" aria-hidden="true"></i>
  Save Changes
</button>

<button type="button" class="btn btn-primary" disabled>
  <span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
  <span class="sr-only">Loading...</span>
  Processing...
</button>

<!-- Secondary actions -->
<button type="button" class="btn btn-outline-secondary">
  <i class="fa fa-times me-2" aria-hidden="true"></i>
  Cancel
</button>

<!-- Icon-only buttons -->
<button type="button" class="btn btn-outline-primary btn-sm" 
        aria-label="Edit message">
  <i class="fa fa-edit" aria-hidden="true"></i>
</button>

<!-- Button group -->
<div class="btn-group" role="group" aria-label="Message actions">
  <button type="button" class="btn btn-outline-primary">
    <i class="fa fa-eye" aria-hidden="true"></i>
    View
  </button>
  <button type="button" class="btn btn-outline-secondary">
    <i class="fa fa-edit" aria-hidden="true"></i>
    Edit
  </button>
  <button type="button" class="btn btn-outline-danger">
    <i class="fa fa-trash" aria-hidden="true"></i>
    Delete
  </button>
</div>
```

### Theme Switcher

```html
<div class="dropdown">
  <button class="btn btn-outline-secondary dropdown-toggle" 
          type="button" id="themeDropdown" 
          data-bs-toggle="dropdown" 
          aria-expanded="false" 
          aria-label="Choose theme">
    <i class="fa fa-palette me-2" aria-hidden="true"></i>
    <span class="theme-name">@CurrentThemeName</span>
  </button>
  <ul class="dropdown-menu" aria-labelledby="themeDropdown">
    <li>
      <button class="dropdown-item" type="button" 
              @onclick="() => SwitchTheme(\"light\")">
        <i class="fa fa-sun me-2" aria-hidden="true"></i>
        Light Theme
      </button>
    </li>
    <li>
      <button class="dropdown-item" type="button" 
              @onclick="() => SwitchTheme(\"dark\")">
        <i class="fa fa-moon me-2" aria-hidden="true"></i>
        Dark Theme
      </button>
    </li>
    <li>
      <button class="dropdown-item" type="button" 
              @onclick="() => SwitchTheme(\"medical\")">
        <i class="fa fa-heartbeat me-2" aria-hidden="true"></i>
        Medical Theme
      </button>
    </li>
  </ul>
</div>
```

### Loading States

#### Spinner Component
```html
<!-- Inline loading -->
<div class="d-inline-flex align-items-center">
  <div class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></div>
  <span>Loading results...</span>
</div>

<!-- Full page loading -->
<div class="loading-container d-flex flex-column align-items-center justify-content-center" 
     style="min-height: 400px;">
  <div class="spinner-border text-primary mb-3" role="status">
    <span class="sr-only">Loading...</span>
  </div>
  <h4 class="mb-2">Processing HL7 Message</h4>
  <p class="text-muted mb-0">Please wait while we validate and process your message...</p>
</div>

<!-- Progress bar -->
<div class="progress mb-3" role="progressbar" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100">
  <div class="progress-bar" style="width: 75%">
    75% Complete
  </div>
</div>
```

### Toast Notifications

```html
<div class="toast show" role="alert" aria-live="assertive" aria-atomic="true">
  <div class="toast-header">
    <i class="fa fa-check-circle text-success me-2" aria-hidden="true"></i>
    <strong class="me-auto">Success</strong>
    <small class="text-muted">Just now</small>
    <button type="button" class="btn-close" data-bs-dismiss="toast" 
            aria-label="Close notification"></button>
  </div>
  <div class="toast-body">
    HL7 message processed successfully. 3 observations recorded.
  </div>
</div>

<!-- Error toast -->
<div class="toast show" role="alert" aria-live="assertive" aria-atomic="true">
  <div class="toast-header bg-danger text-white">
    <i class="fa fa-exclamation-triangle me-2" aria-hidden="true"></i>
    <strong class="me-auto">Error</strong>
    <small>2 minutes ago</small>
    <button type="button" class="btn-close btn-close-white" 
            data-bs-dismiss="toast" aria-label="Close error notification"></button>
  </div>
  <div class="toast-body">
    <p class="mb-2">Failed to process HL7 message:</p>
    <code class="d-block">Invalid segment MSH - missing required field 3</code>
  </div>
</div>
```

## Utility Classes

### Spacing and Layout
```html
<!-- Responsive spacing -->
<div class="p-3 p-md-4 p-lg-5">Responsive padding</div>
<div class="mb-3 mb-md-4">Responsive bottom margin</div>

<!-- Flexbox utilities -->
<div class="d-flex justify-content-between align-items-center">
  <span>Left content</span>
  <span>Right content</span>
</div>

<!-- Grid gap -->
<div class="row g-3 g-md-4">
  <div class="col">Column with responsive gap</div>
</div>
```

### Typography
```html
<!-- Headings -->
<h1 class="display-4 text-primary">Main Heading</h1>
<h2 class="h3 fw-semibold">Section Heading</h2>

<!-- Text utilities -->
<p class="lead">Important paragraph text</p>
<small class="text-muted">Helper text</small>
<code class="user-select-all">MSG-12345</code>
```

### Theme-Aware Utilities
```css
/* Custom utility classes */
.text-theme-primary {
  color: var(--color-primary) !important;
}

.bg-theme-subtle {
  background-color: var(--background-color-subtle) !important;
}

.border-theme {
  border-color: var(--border-color) !important;
}
```

## Accessibility Features

### Screen Reader Support
```html
<!-- Skip links -->
<a href="#main-content" class="sr-only sr-only-focusable">Skip to main content</a>

<!-- Screen reader only text -->
<span class="sr-only">Additional context for screen readers</span>

<!-- ARIA live regions -->
<div aria-live="polite" aria-atomic="true" class="sr-only" id="status-message"></div>
```

### Focus Management
```css
/* Enhanced focus indicators */
:focus {
  outline: 2px solid var(--color-primary);
  outline-offset: 2px;
}

/* Focus within containers */
.card:focus-within {
  box-shadow: 0 0 0 2px var(--color-primary-light);
}

/* Skip link styling */
.sr-only-focusable:focus {
  position: absolute;
  top: 0;
  left: 0;
  z-index: 1050;
  padding: 0.5rem 1rem;
  background: var(--color-primary);
  color: white;
  text-decoration: none;
  border-radius: 0 0 0.375rem 0;
}
```

---

## Usage Guidelines

### Component Selection
1. **Use Bootstrap components first** - Leverage existing patterns
2. **Customize through CSS custom properties** - Maintain theme compatibility
3. **Add ARIA labels** - Ensure accessibility compliance
4. **Test across themes** - Verify appearance in all available themes
5. **Mobile-first approach** - Design for small screens first

### Custom Component Development
1. Follow Blazor CSS isolation patterns
2. Use semantic HTML5 elements
3. Implement proper focus management
4. Provide comprehensive ARIA labels
5. Test with keyboard navigation
6. Validate with screen readers

### Performance Considerations
1. Use CSS custom properties for theming
2. Avoid inline styles
3. Minimize DOM manipulation
4. Use efficient CSS selectors
5. Implement lazy loading for heavy components

---

*Last updated: September 10, 2025*
