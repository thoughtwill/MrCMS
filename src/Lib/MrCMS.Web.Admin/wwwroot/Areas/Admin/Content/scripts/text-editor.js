export function initializeTextEditor() {
    //this setTimeout is needed because of the wired bug in js (selector returns many wired elements), already checked with Gary
    setTimeout(function () {
        let textareas = document.querySelectorAll('textarea.enable-editor');
        textareas.forEach((element) => {
            let elementId = element.getAttribute('id');
            if (!element.hasAttribute("editor-initiated")) {
                element.setAttribute("editor-initiated", "true");
                if (CKEDITOR.instances[elementId]) {
                    delete CKEDITOR.instances[elementId];
                }
                CKEDITOR.replace(elementId);
            }
        });
    }, 100);
}