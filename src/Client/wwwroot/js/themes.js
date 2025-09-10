window.setTheme = function(themeName) {
    // Remove all theme classes from <body>
    document.body.classList.remove('theme-light', 'theme-dark', 'theme-medical');
    // Add the selected theme class
    document.body.classList.add('theme-' + themeName);
    // Optionally persist theme to localStorage
    localStorage.setItem('hl7Theme', themeName);
    // Update CSS custom properties if needed
    // (Assume theme CSS files set variables via body class)
};

window.getTheme = function() {
    return localStorage.getItem('hl7Theme') || 'light';
};
