// Scoped JavaScript for HL7MessageTestingPage
// This module provides page-specific utilities for navigation, download, and storage

export function scrollToElement(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'start' });
        // Focus the element for accessibility
        element.focus({ preventScroll: true });
    }
}

export function downloadFile(fileName, content, contentType) {
    const blob = new Blob([content], { type: contentType });
    const url = URL.createObjectURL(blob);

    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();

    document.body.removeChild(a);
    URL.revokeObjectURL(url);
}

export function showToast(message, type = 'info') {
    // Create toast element
    const toastId = 'toast-' + Date.now();
    const toastHtml = `
        <div id="${toastId}" class="toast align-items-center text-bg-${type === 'error' ? 'danger' : type}" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `;

    // Find or create toast container
    let toastContainer = document.getElementById('toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toast-container';
        toastContainer.className = 'toast-container position-fixed bottom-0 end-0 p-3';
        toastContainer.style.zIndex = '1070';
        document.body.appendChild(toastContainer);
    }

    // Add toast to container
    toastContainer.insertAdjacentHTML('beforeend', toastHtml);

    // Initialize and show the toast
    const toastElement = document.getElementById(toastId);
    if (toastElement && window.bootstrap) {
        const toast = new bootstrap.Toast(toastElement);
        toast.show();

        // Clean up after toast is hidden
        toastElement.addEventListener('hidden.bs.toast', function () {
            toastElement.remove();
        });
    }
}

export async function saveToLocalStorage(key, data) {
    try {
        const jsonData = JSON.stringify(data);
        localStorage.setItem(key, jsonData);
        return true;
    } catch (error) {
        console.error('Failed to save to localStorage:', error);
        return false;
    }
}

export async function loadFromLocalStorage(key) {
    try {
        const data = localStorage.getItem(key);
        return data ? JSON.parse(data) : null;
    } catch (error) {
        console.error('Failed to load from localStorage:', error);
        return null;
    }
}

export async function removeFromLocalStorage(key) {
    try {
        localStorage.removeItem(key);
        return true;
    } catch (error) {
        console.error('Failed to remove from localStorage:', error);
        return false;
    }
}

export function updatePageTitle(title) {
    document.title = title;
}

export function scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

export function highlightElement(elementId, duration = 2000) {
    const element = document.getElementById(elementId);
    if (!element) return;

    // Add highlight class
    element.classList.add('highlight-animation');

    // Remove highlight after duration
    setTimeout(() => {
        element.classList.remove('highlight-animation');
    }, duration);
}

// Component initialization function
export function initialize(dotNetHelper) {
    // Store reference for any callbacks if needed
    window.hl7MessageTestingPageHelper = dotNetHelper;

    // Set up any page-specific event listeners
    setupPageEventListeners();

    return Promise.resolve();
}

// Component disposal function
export function dispose() {
    // Clean up any global references
    if (window.hl7MessageTestingPageHelper) {
        delete window.hl7MessageTestingPageHelper;
    }

    // Clean up event listeners
    cleanupPageEventListeners();

    return Promise.resolve();
}

// Internal helper functions
function setupPageEventListeners() {
    // Add any page-specific event listeners here
    // For example, keyboard shortcuts, window events, etc.
}

function cleanupPageEventListeners() {
    // Remove any page-specific event listeners here
}
