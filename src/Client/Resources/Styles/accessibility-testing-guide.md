# Accessibility Testing Guide (WCAG 2.2 AA)

## Overview

This guide provides comprehensive testing procedures to ensure the HL7 Results Gateway meets WCAG 2.2 AA accessibility standards.

## Testing Checklist

### 1. Color and Contrast

#### Color Contrast Requirements
- [ ] Normal text (18px or smaller): 4.5:1 contrast ratio minimum
- [ ] Large text (18px+ or 14px+ bold): 3:1 contrast ratio minimum
- [ ] UI components and graphics: 3:1 contrast ratio minimum

#### Color Testing Tools
- **WebAIM Contrast Checker**: https://webaim.org/resources/contrastchecker/
- **Colour Contrast Analyser**: Free desktop application
- **axe DevTools**: Browser extension for automated testing

#### Color Independence
- [ ] All information conveyed by color is also available through other means
- [ ] Interactive elements are identifiable without relying solely on color
- [ ] Error states use icons or text in addition to color coding

### 2. Keyboard Navigation

#### Keyboard Testing Procedure
1. Use **Tab** key to navigate through all interactive elements
2. Use **Shift+Tab** to navigate backwards
3. Use **Enter** and **Space** to activate buttons and links
4. Use arrow keys for dropdown menus and radio button groups

#### Keyboard Navigation Checklist
- [ ] All interactive elements are reachable via keyboard
- [ ] Tab order is logical and intuitive
- [ ] Focus indicators are clearly visible
- [ ] No keyboard traps (can navigate out of any component)
- [ ] Skip links work properly
- [ ] Modal dialogs trap focus appropriately
- [ ] Dropdown menus are keyboard accessible

### 3. Screen Reader Testing

#### Screen Reader Tools
- **NVDA** (Windows) - Free
- **JAWS** (Windows) - Commercial
- **VoiceOver** (macOS) - Built-in
- **Orca** (Linux) - Free

#### Screen Reader Testing Checklist
- [ ] Page titles are descriptive and unique
- [ ] Heading hierarchy is logical (h1, h2, h3, etc.)
- [ ] All images have appropriate alt text
- [ ] Form labels are properly associated
- [ ] Error messages are announced
- [ ] Dynamic content changes are announced
- [ ] ARIA landmarks are used correctly
- [ ] Interactive elements have descriptive names

### 4. ARIA Implementation

#### ARIA Landmarks
```html
<!-- Required landmarks -->
<header role="banner">
<nav role="navigation" aria-label="Main navigation">
<main role="main">
<aside role="complementary">
<footer role="contentinfo">
```

#### ARIA Labels and Descriptions
```html
<!-- Form elements -->
<label for="email">Email Address</label>
<input type="email" id="email" aria-describedby="email-help" required>
<div id="email-help">We'll never share your email</div>

<!-- Buttons with icons -->
<button aria-label="Close dialog">
  <i class="fa fa-times" aria-hidden="true"></i>
</button>

<!-- Status messages -->
<div role="status" aria-live="polite">
  Form saved successfully
</div>
```

#### ARIA Testing Checklist
- [ ] All form controls have accessible names
- [ ] Complex widgets have appropriate ARIA roles
- [ ] Live regions announce dynamic content
- [ ] ARIA expanded/collapsed states are correct
- [ ] ARIA hidden is used appropriately
- [ ] ARIA describedby provides additional context

### 5. Focus Management

#### Focus Indicator Requirements
```css
/* Minimum focus indicator requirements */
:focus {
  outline: 2px solid var(--color-primary);
  outline-offset: 2px;
}

/* High contrast mode support */
@media (prefers-contrast: high) {
  :focus {
    outline-width: 3px;
  }
}
```

#### Focus Management Checklist
- [ ] All interactive elements have visible focus indicators
- [ ] Focus indicators meet contrast requirements
- [ ] Focus is managed properly in modal dialogs
- [ ] Focus returns appropriately after interactions
- [ ] Skip links are implemented and functional

### 6. Form Accessibility

#### Form Structure Requirements
```html
<!-- Proper form labeling -->
<div class="mb-3">
  <label for="username" class="form-label required">
    Username <span aria-label="required">*</span>
  </label>
  <input type="text" class="form-control" id="username" 
         aria-describedby="username-help" required>
  <div id="username-help" class="form-text">
    Choose a unique username (3-20 characters)
  </div>
</div>

<!-- Error handling -->
<div class="mb-3">
  <label for="email-error" class="form-label">Email</label>
  <input type="email" class="form-control is-invalid" 
         id="email-error" aria-describedby="email-error-msg">
  <div id="email-error-msg" class="invalid-feedback" role="alert">
    Please provide a valid email address
  </div>
</div>
```

#### Form Testing Checklist
- [ ] All form controls have associated labels
- [ ] Required fields are clearly marked
- [ ] Error messages are descriptive and helpful
- [ ] Field validation is announced to screen readers
- [ ] Form instructions are provided before the form
- [ ] Error summary is provided for complex forms

### 7. Automated Testing Tools

#### Browser Extensions
- **axe DevTools**: Comprehensive accessibility testing
- **WAVE**: Web accessibility evaluation
- **Lighthouse**: Performance and accessibility audit
- **Accessibility Insights**: Microsoft's testing tool

#### Command Line Tools
```bash
# axe-core CLI testing
npx @axe-core/cli https://localhost:7276

# Pa11y testing
npx pa11y https://localhost:7276

# Lighthouse accessibility audit
lighthouse https://localhost:7276 --only-categories=accessibility
```

### 8. Manual Testing Procedures

#### Daily Testing Routine
1. **Tab through entire application** without using mouse
2. **Test with screen reader** for 5-10 minutes
3. **Verify color contrast** on new components
4. **Check ARIA labels** on interactive elements

#### Weekly Testing Routine
1. **Full keyboard navigation test** of all features
2. **Complete screen reader walkthrough**
3. **Automated testing with axe DevTools**
4. **High contrast mode testing**
5. **Zoom testing up to 200%**

#### Release Testing Routine
1. **Cross-platform screen reader testing**
2. **Full WCAG 2.2 AA compliance audit**
3. **User testing with disabled users** (if possible)
4. **Performance testing with assistive technologies**

### 9. Common Accessibility Issues

#### Critical Issues (Must Fix)
- Missing alt text on informative images
- Form controls without labels
- Insufficient color contrast
- Keyboard inaccessible interactive elements
- Missing focus indicators

#### Important Issues (Should Fix)
- Poor heading hierarchy
- Missing ARIA landmarks
- Unclear error messages
- Inconsistent navigation
- Missing skip links

### 10. Testing Documentation

#### Test Report Template
```markdown
## Accessibility Test Report
**Date**: [Test Date]
**Tester**: [Name]
**Tools Used**: [List of tools]
**Scope**: [Pages/features tested]

### Summary
- Issues Found: [Number]
- Critical Issues: [Number]
- WCAG 2.2 AA Compliance: [Pass/Fail]

### Detailed Findings
[List issues with severity levels]

### Recommendations
[Prioritized list of fixes needed]
```

---

## Resources

### WCAG 2.2 Guidelines
- **Official Guidelines**: https://www.w3.org/WAI/WCAG22/quickref/
- **Understanding WCAG**: https://www.w3.org/WAI/WCAG22/Understanding/
- **Techniques**: https://www.w3.org/WAI/WCAG22/Techniques/

### Testing Tools
- **axe DevTools**: https://www.deque.com/axe/devtools/
- **WAVE**: https://wave.webaim.org/
- **Colour Contrast Analyser**: https://www.tpgi.com/color-contrast-checker/
- **Accessibility Insights**: https://accessibilityinsights.io/

### Screen Readers
- **NVDA**: https://www.nvaccess.org/
- **JAWS**: https://www.freedomscientific.com/products/software/jaws/
- **VoiceOver Guide**: https://webaim.org/articles/voiceover/

---

*Last updated: September 10, 2025*
