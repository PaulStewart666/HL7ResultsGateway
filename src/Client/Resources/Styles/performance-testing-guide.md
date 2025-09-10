# Performance Testing Guide

## Overview

This guide covers performance testing procedures for the HL7 Results Gateway UI, focusing on CSS loading optimization, rendering performance, and user experience metrics.

## Core Web Vitals

### Largest Contentful Paint (LCP)
- **Target**: < 2.5 seconds
- **Good**: < 2.5s | **Needs Improvement**: 2.5s-4s | **Poor**: > 4s
- **What it measures**: Loading performance of the largest content element

### First Input Delay (FID)
- **Target**: < 100ms
- **Good**: < 100ms | **Needs Improvement**: 100ms-300ms | **Poor**: > 300ms
- **What it measures**: Responsiveness to user interactions

### Cumulative Layout Shift (CLS)
- **Target**: < 0.1
- **Good**: < 0.1 | **Needs Improvement**: 0.1-0.25 | **Poor**: > 0.25
- **What it measures**: Visual stability during page load

## Performance Testing Tools

### Google Lighthouse
```bash
# Command line testing
lighthouse https://localhost:7276 --output html --output-path ./performance-report.html

# Performance-only audit
lighthouse https://localhost:7276 --only-categories=performance

# Mobile simulation
lighthouse https://localhost:7276 --form-factor=mobile --throttling-method=simulate
```

### WebPageTest
- **URL**: https://www.webpagetest.org/
- **Settings**: 
  - Location: Choose nearest location
  - Browser: Chrome
  - Connection: 3G/4G for mobile testing
  - Number of Tests: 3 (for average)

### Chrome DevTools
1. **Performance Tab**: 
   - Record loading and interaction performance
   - Analyze main thread blocking
   - Identify render-blocking resources

2. **Network Tab**:
   - Monitor CSS file loading times
   - Check for unused CSS
   - Verify CDN cache effectiveness

3. **Coverage Tab**:
   - Identify unused CSS and JavaScript
   - Measure critical CSS effectiveness

## CSS Performance Optimization

### Critical CSS Strategy

#### Current Implementation
```html
<!-- Inlined critical CSS in <head> -->
<style>
/* Critical above-the-fold styles */
:root {
  /* Essential CSS custom properties */
}

body {
  /* Base body styles for immediate rendering */
}

.loading-spinner {
  /* Loading state styles */
}
</style>

<!-- Async load non-critical CSS -->
<link rel="stylesheet" href="css/app.css" media="print" onload="this.media='all'">
```

#### Performance Targets
- Critical CSS: < 10KB inlined
- First Paint: < 1.5 seconds
- CSS blocking time: < 500ms

### CSS Loading Performance

#### CDN Performance Testing
```bash
# Test Bootstrap CDN loading time
curl -w "@curl-format.txt" -o /dev/null -s "https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css"

# Test Google Fonts loading
curl -w "@curl-format.txt" -o /dev/null -s "https://fonts.googleapis.com/css2?family=Roboto:wght@300;400;500;700&display=swap"
```

#### Local CSS Performance
- **app.css**: Target < 50KB minified
- **Theme files**: < 10KB each
- **Component CSS**: < 5KB per component

## Theme Switching Performance

### Performance Metrics
- **Theme switch time**: < 100ms
- **Visual update**: < 50ms
- **Layout stability**: No CLS during theme switch

### Testing Procedure
1. Open Chrome DevTools Performance tab
2. Start recording
3. Switch themes multiple times
4. Stop recording and analyze:
   - JavaScript execution time
   - CSS recalculation time
   - Layout and paint events

### Optimization Techniques
```css
/* Optimized theme switching */
body {
  transition: background-color 0.3s ease, color 0.3s ease;
  /* Use hardware acceleration */
  transform: translateZ(0);
  will-change: background-color, color;
}

/* Avoid expensive animations */
.theme-transition {
  transition: background-color 0.3s ease;
  /* Avoid: transition: all 0.3s ease; */
}
```

## Rendering Performance

### Layout Performance

#### Layout Thrashing Prevention
```css
/* Bad: Causes layout recalculation */
.expensive-animation {
  animation: slide 2s ease;
}

@keyframes slide {
  from { left: 0; }
  to { left: 100px; }
}

/* Good: Uses transform (composite layer) */
.optimized-animation {
  animation: slideOptimized 2s ease;
}

@keyframes slideOptimized {
  from { transform: translateX(0); }
  to { transform: translateX(100px); }
}
```

#### CSS Performance Best Practices
- Use `transform` and `opacity` for animations
- Avoid animating layout properties (`width`, `height`, `top`, `left`)
- Use `will-change` sparingly and remove after animation
- Minimize selector complexity

### Paint Performance

#### Layer Management
```css
/* Force composite layer for smooth animations */
.smooth-element {
  transform: translateZ(0);
  /* or */
  will-change: transform;
}

/* Remove will-change after animation */
.animation-complete {
  will-change: auto;
}
```

## Mobile Performance

### Mobile-Specific Testing

#### Device Simulation Settings
```javascript
// Chrome DevTools mobile simulation
// CPU: 6x slowdown
// Network: Slow 3G
// Device: iPhone 12 Pro
```

#### Mobile Performance Targets
- **First Contentful Paint**: < 2.0s
- **Speed Index**: < 4.0s
- **Time to Interactive**: < 5.0s
- **Total Blocking Time**: < 300ms

### Touch Performance
- **Touch response time**: < 100ms
- **Scroll performance**: 60fps
- **Tap targets**: Minimum 44px x 44px

## Performance Monitoring

### Automated Performance Testing

#### Lighthouse CI Integration
```yaml
# .github/workflows/performance.yml
name: Performance Testing
on: [push, pull_request]

jobs:
  lighthouse:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Run Lighthouse CI
        uses: treosh/lighthouse-ci-action@v9
        with:
          urls: |
            http://localhost:7276
            http://localhost:7276/dashboard
          budgetPath: ./budget.json
          uploadArtifacts: true
```

#### Performance Budget
```json
{
  "ci": {
    "collect": {
      "numberOfRuns": 3
    },
    "assert": {
      "assertions": {
        "categories:performance": ["error", {"minScore": 0.9}],
        "categories:accessibility": ["error", {"minScore": 0.9}],
        "first-contentful-paint": ["error", {"maxNumericValue": 2000}],
        "largest-contentful-paint": ["error", {"maxNumericValue": 2500}],
        "cumulative-layout-shift": ["error", {"maxNumericValue": 0.1}]
      }
    }
  }
}
```

### Real User Monitoring (RUM)

#### Performance Analytics Setup
```javascript
// Performance measurement
if ('performance' in window) {
  window.addEventListener('load', () => {
    // Measure Core Web Vitals
    const observer = new PerformanceObserver((list) => {
      list.getEntries().forEach((entry) => {
        console.log(`${entry.name}: ${entry.value}`);
        // Send to analytics
      });
    });
    
    observer.observe({ entryTypes: ['paint', 'navigation', 'resource'] });
  });
}
```

## Testing Scenarios

### Load Testing Scenarios

1. **Cold Cache Load**
   - Clear all caches
   - Load application
   - Measure initial loading performance

2. **Warm Cache Load**
   - Load application with cached resources
   - Measure repeat visit performance

3. **Theme Switch Performance**
   - Load application
   - Switch between all available themes
   - Measure transition performance

4. **Mobile Network Simulation**
   - Test with 3G, 4G, and offline scenarios
   - Measure progressive loading

### Performance Test Checklist

#### Pre-Release Testing
- [ ] Lighthouse score > 90 for Performance
- [ ] Core Web Vitals meet "Good" thresholds
- [ ] Critical CSS < 10KB
- [ ] Theme switching < 100ms
- [ ] Mobile performance targets met
- [ ] No layout shifts during loading
- [ ] All animations at 60fps
- [ ] CDN resources load successfully

#### Ongoing Monitoring
- [ ] Weekly performance reports
- [ ] Performance regression alerts
- [ ] User experience metrics tracking
- [ ] Third-party dependency monitoring

## Performance Optimization Results

### Baseline Measurements
| Metric | Target | Current | Status |
|--------|--------|---------|--------|
| LCP | < 2.5s | - | ⏳ Pending |
| FID | < 100ms | - | ⏳ Pending |
| CLS | < 0.1 | - | ⏳ Pending |
| Performance Score | > 90 | - | ⏳ Pending |
| CSS File Size | < 50KB | - | ⏳ Pending |
| Theme Switch | < 100ms | - | ⏳ Pending |

### Optimization Impact
- **Critical CSS Implementation**: Expected 30% improvement in FCP
- **CDN Usage**: Expected 50% improvement in resource loading
- **Async CSS Loading**: Expected 25% improvement in LCP
- **Theme Optimization**: Expected smooth theme transitions

---

*Last updated: September 10, 2025*
