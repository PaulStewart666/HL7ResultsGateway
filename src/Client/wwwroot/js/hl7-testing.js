// HL7 Testing JavaScript Utilities

// Create the hl7Testing namespace
window.hl7Testing = {

    /**
     * Downloads a file with the specified content
     * @param {string} fileName - The name of the file to download
     * @param {string} content - The content to download
     * @param {string} contentType - The MIME type of the file
     */
    downloadFile: function (fileName, content, contentType) {
        const blob = new Blob([content], { type: contentType });
        const url = URL.createObjectURL(blob);

        const a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();

        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    },

    /**
     * Scrolls to an element with the specified ID
     * @param {string} elementId - The ID of the element to scroll to
     */
    scrollToElement: function (elementId) {
        const element = document.getElementById(elementId);
        if (element) {
            element.scrollIntoView({ behavior: 'smooth', block: 'start' });
            // Focus the element for accessibility
            element.focus({ preventScroll: true });
        }
    },

    /**
     * Copies text to the clipboard
     * @param {string} text - The text to copy
     */
    copyToClipboard: async function (text) {
        if (navigator.clipboard) {
            try {
                await navigator.clipboard.writeText(text);
                return true;
            } catch (err) {
                console.error('Failed to copy text to clipboard:', err);
                return false;
            }
        } else {
            // Fallback for older browsers
            const textArea = document.createElement('textarea');
            textArea.value = text;
            textArea.style.position = 'fixed';
            textArea.style.left = '-999999px';
            textArea.style.top = '-999999px';
            document.body.appendChild(textArea);
            textArea.focus();
            textArea.select();

            try {
                const successful = document.execCommand('copy');
                document.body.removeChild(textArea);
                return successful;
            } catch (err) {
                console.error('Fallback copy failed:', err);
                document.body.removeChild(textArea);
                return false;
            }
        }
    },

    /**
     * Shows a toast notification (requires Bootstrap)
     * @param {string} message - The message to display
     * @param {string} type - The type of toast ('success', 'error', 'info', 'warning')
     */
    showToast: function (message, type = 'info') {
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
    },

    /**
     * Validates if a string looks like an HL7 message
     * @param {string} content - The content to validate
     * @returns {object} - Validation result with isValid and errors properties
     */
    validateHL7Format: function (content) {
        const result = {
            isValid: true,
            errors: []
        };

        if (!content || content.trim().length === 0) {
            result.isValid = false;
            result.errors.push('Message content is empty');
            return result;
        }

        const lines = content.split(/\r?\n/).filter(line => line.trim().length > 0);

        if (lines.length === 0) {
            result.isValid = false;
            result.errors.push('No valid HL7 segments found');
            return result;
        }

        // Check MSH segment
        const firstLine = lines[0].trim();
        if (!firstLine.startsWith('MSH|')) {
            result.isValid = false;
            result.errors.push('HL7 messages must start with an MSH (Message Header) segment');
        }

        // Check basic field separator structure
        lines.forEach((line, index) => {
            if (!line.includes('|')) {
                result.errors.push(`Line ${index + 1} does not contain field separators (|)`);
            }
        });

        if (result.errors.length > 0) {
            result.isValid = false;
        }

        return result;
    }

};

// Also make functions available globally for backward compatibility
window.downloadFile = window.hl7Testing.downloadFile;
window.scrollToElement = window.hl7Testing.scrollToElement;
window.copyToClipboard = window.hl7Testing.copyToClipboard;
window.showToast = window.hl7Testing.showToast;
window.validateHL7Format = window.hl7Testing.validateHL7Format;
