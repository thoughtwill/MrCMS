export function initPromptEditor() {
    // Find all containers with the class 'prompt-editor'
    const editors = document.querySelectorAll('.prompt-editor');
    if (!editors.length) return;

    editors.forEach(editor => {
        const modalEl = editor.querySelector('.modal');
        $(modalEl).on('show.bs.modal', function (event) {
            initEditor(editor);
        })
    });

    document.querySelectorAll('form[prompt-form]').forEach(form => {
        form.addEventListener('submit', function(e) {
            e.preventDefault();

            // Create the JSON data structure
            const formData = new FormData(form);
            const data = Object.fromEntries(formData.entries());

            // Get the target function name from the attribute
            const parentFunctionName = form.getAttribute('prompt-form');

            // Check if the parent function exists before calling
            if (window.parent && typeof window.parent[parentFunctionName] === 'function') {
                window.parent[parentFunctionName](data);

                // Close Featherlight modal after successful submission
                if (window.parent.$ && window.parent.$.featherlight) {
                    window.parent.$.featherlight.close();
                }
            } else {
                console.error(`Parent function ${parentFunctionName} not found`);
            }
        });
    });
}


function initEditor(editor) {
    // The visible main prompt text area (final prompt)
    const finalPromptArea = editor.querySelector('.prompt-textarea');
    // The hidden area within the modal where the prompt template is stored
    const templateArea = editor.querySelector('.prompt-template-area');
    // The container where dynamic input fields will be generated
    const dynamicInputs = editor.querySelector('.dynamic-inputs');
    // The button that applies the prompt
    const applyButton = editor.querySelector('.apply-prompt-button');

    dynamicInputs.innerHTML = '';

    applyButton.classList.add("d-none");

    // Remove previous event listeners from prompt items
    const previousPromptItems = editor.querySelectorAll('.prompt-select');
    previousPromptItems.forEach(item => {
        if (item._promptItemHandler) {
            item.removeEventListener('click', item._promptItemHandler);
            delete item._promptItemHandler;
        }
    });

    // Set up new click handlers on each prompt list item
    const promptItems = editor.querySelectorAll('.prompt-select');
    promptItems.forEach(item => {
        const handler = function () {
            const template = this.getAttribute('data-template');
            const title = this.getAttribute('data-title');
            if (templateArea) {
                templateArea.value = template;
                generateInputFields(title, template);
            }
            const hasDynamicInputs = dynamicInputs && dynamicInputs.children.length > 0;
            const modalEl = editor.querySelector('.modal'); // Moved inside for proper scoping

            if (!hasDynamicInputs) {
                if (modalEl && window.jQuery) {
                    $(modalEl).modal('hide');
                }
                applyButton.classList.remove("d-none");
            }
        };
        item.addEventListener('click', handler);
        item._promptItemHandler = handler; // Store reference
    });

    if (applyButton && applyButton._applyHandler) {
        applyButton.removeEventListener('click', applyButton._applyHandler);
        delete applyButton._applyHandler;
    }

    if (applyButton) {
        const applyHandler = function () {
            let finalPrompt = templateArea ? templateArea.value : '';
            const inputs = dynamicInputs ? dynamicInputs.querySelectorAll('input[data-key]') : [];

            inputs.forEach(input => {
                const key = input.getAttribute('data-key');
                const value = input.value;
                // Fix: Replace only placeholders matching this exact key
                const regex = new RegExp(`\\{\\{${key}\\}\\}`, 'g');
                finalPrompt = finalPrompt.replace(regex, value);
            });

            if (finalPromptArea) {
                finalPromptArea.value = finalPrompt;
            }

            const event = new CustomEvent('promptApplied', { detail: finalPrompt });
            editor.dispatchEvent(event);

            const modalEl = editor.querySelector('.modal');
            if (modalEl && window.jQuery) {
                $(modalEl).modal('hide');
            }
        };
        applyButton.addEventListener('click', applyHandler);
        applyButton._applyHandler = applyHandler; // Store reference
    }

    // Function to generate input fields (unchanged)
    function generateInputFields(title, template) {
        const regex = /\{\{([^}]+)}}/g;
        let match;

        dynamicInputs.innerHTML = `<hr class="my-3"/>`;
        let inputs = document.createElement('div');
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
            inputs.appendChild(group);
        }
        
        dynamicInputs.innerHTML += `<div class="card">
                <div class="card-header">
                    ${title}
                  </div>
                <div class="card-body">
                    ${inputs.innerHTML}
                    </div>
                </div>`;
    }
}