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

// File download utility function
window.downloadFile = function(filename, dataUrl) {
    try {
        // Create a temporary anchor element to trigger download
        const link = document.createElement('a');
        link.download = filename;
        link.href = dataUrl;

        // Append to body, click, and remove
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        console.log('File download initiated:', filename);
    } catch (error) {
        console.error('Error downloading file:', error);
        throw error;
    }
};
