export function initPromptEditor() {
    // Find all containers with the class 'prompt-editor'
    const editors = document.querySelectorAll('.prompt-editor');
    if (!editors.length) return;

    editors.forEach(editor => {
        // The visible main prompt text area (final prompt)
        const finalPromptArea = editor.querySelector('.final-prompt');
        // The area within the modal where the prompt template is displayed
        const templateArea = editor.querySelector('.prompt-template-area');
        // The container where dynamic input fields will be generated
        const dynamicInputs = editor.querySelector('.dynamic-inputs');
        // The button that applies the prompt
        const applyButton = editor.querySelector('.apply-prompt-button');
        // The list of prompt options
        const promptItems = editor.querySelectorAll('.prompt-select');

        // Function to generate input fields based on the placeholders in the template
        function generateInputFields(template) {
            if (!dynamicInputs) return;
            dynamicInputs.innerHTML = ''; // Clear previous inputs
            // Regex to match placeholders in the format {{Key}}
            const regex = /{{(.*?)}}/g;
            let match;
            while ((match = regex.exec(template)) !== null) {
                const key = match[1].trim();
                const group = document.createElement('div');
                group.className = 'mb-2';

                const label = document.createElement('label');
                label.innerText = key;
                label.className = 'form-label';

                const input = document.createElement('input');
                input.type = 'text';
                input.className = 'form-control';
                input.setAttribute('data-key', key);

                group.appendChild(label);
                group.appendChild(input);
                dynamicInputs.appendChild(group);
            }
        }

        // Set up click handlers on each prompt list item
        promptItems.forEach(item => {
            item.addEventListener('click', function () {
                const template = this.getAttribute('data-template');
                if (templateArea) {
                    templateArea.value = template;
                    generateInputFields(template);
                }
                // If using a modal, close it after selection.
                const modalEl = editor.querySelector('.modal');
                if (modalEl) {
                    const modalInstance = bootstrap.Modal.getInstance(modalEl);
                    if (modalInstance) {
                        modalInstance.hide();
                    }
                }
            });
        });

        // When the Apply Prompt button is clicked, merge the input values into the template.
        if (applyButton) {
            applyButton.addEventListener('click', function () {
                let finalPrompt = templateArea ? templateArea.value : '';
                const inputs = dynamicInputs ? dynamicInputs.querySelectorAll('input[data-key]') : [];
                inputs.forEach(input => {
                    const key = input.getAttribute('data-key');
                    const value = input.value;
                    // Replace all occurrences of the placeholder with the input value.
                    const regex = new RegExp(`{{\\s*${key}\\s*}}`, 'g');
                    finalPrompt = finalPrompt.replace(regex, value);
                });

                // Update the main final prompt text area so that the form posts the final prompt.
                if (finalPromptArea) {
                    finalPromptArea.value = finalPrompt;
                }

                // Optionally, dispatch a custom event if needed.
                const event = new CustomEvent('promptApplied', { detail: finalPrompt });
                editor.dispatchEvent(event);

                // Close the modal if applicable.
                const modalEl = editor.querySelector('.modal');
                if (modalEl) {
                    const modalInstance = bootstrap.Modal.getInstance(modalEl);
                    if (modalInstance) {
                        modalInstance.hide();
                    }
                }
            });
        }
    });
}