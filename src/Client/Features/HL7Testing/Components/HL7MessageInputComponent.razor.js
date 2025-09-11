// Scoped JavaScript for HL7MessageInputComponent
// This module provides validation and input assistance utilities specific to HL7 message input

export function validateHL7Format(content) {
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

export function highlightHL7Syntax(textareaId) {
    const textarea = document.getElementById(textareaId);
    if (!textarea) return;

    // Add syntax highlighting class for potential CSS styling
    textarea.classList.add('hl7-syntax-highlighted');
    
    // Could be enhanced with more sophisticated syntax highlighting
    // For now, just add visual cues via CSS classes
}

export function autoResizeTextarea(textareaId) {
    const textarea = document.getElementById(textareaId);
    if (!textarea) return;

    // Reset height to auto to get the correct scrollHeight
    textarea.style.height = 'auto';
    
    // Set height to scrollHeight with some padding
    const maxHeight = 400; // Maximum height in pixels
    const newHeight = Math.min(textarea.scrollHeight, maxHeight);
    textarea.style.height = newHeight + 'px';
    
    // Add scrollbar if content exceeds max height
    textarea.style.overflowY = textarea.scrollHeight > maxHeight ? 'scroll' : 'hidden';
}

export function showValidationErrors(errors, containerId) {
    const container = document.getElementById(containerId);
    if (!container) return;

    // Clear existing errors
    container.innerHTML = '';

    if (!errors || errors.length === 0) {
        container.style.display = 'none';
        return;
    }

    // Show container
    container.style.display = 'block';
    
    // Create error list
    const errorList = document.createElement('ul');
    errorList.className = 'list-unstyled mb-0';
    
    errors.forEach(error => {
        const li = document.createElement('li');
        li.className = 'text-danger small';
        li.innerHTML = `<i class="fa fa-exclamation-triangle me-1"></i>${error}`;
        errorList.appendChild(li);
    });
    
    container.appendChild(errorList);
}

// Component initialization function
export function initialize(dotNetHelper) {
    // Store reference for any callbacks if needed
    window.hl7MessageInputComponentHelper = dotNetHelper;
    return Promise.resolve();
}

// Component disposal function
export function dispose() {
    // Clean up any global references
    if (window.hl7MessageInputComponentHelper) {
        delete window.hl7MessageInputComponentHelper;
    }
    return Promise.resolve();
}