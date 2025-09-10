window.setTheme = function(themeName) {
    // Remove all theme classes from <body>
    document.body.classList.remove('theme-light', 'theme-dark', 'theme-medical');
    // Add the selected theme class
    document.body.classList.add('theme-' + themeName);
    // Persist theme to localStorage
    localStorage.setItem('hl7Theme', themeName);
    console.log('Theme set to:', themeName);
};

window.getTheme = function() {
    return localStorage.getItem('hl7Theme') || 'light';
};

// Initialize theme on page load
window.initializeTheme = function() {
    const savedTheme = window.getTheme();
    window.setTheme(savedTheme);
    console.log('Theme initialized to:', savedTheme);
};
