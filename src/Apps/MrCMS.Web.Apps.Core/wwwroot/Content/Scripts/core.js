//Bootstrap JS
import * as bootstrap from 'bootstrap';

import GLightbox from 'glightbox';
import {setupRecaptcha} from "./recaptcha";

(function() {
    setupRecaptcha();
    const lightbox = GLightbox({
        selector: ".glightbox",
    });
    const forms = document.querySelectorAll('form[novalidate]');

    Array.prototype.slice.call(forms).forEach((form) => {
        form.addEventListener('submit', (event) => {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }

            form.classList.add('was-validated');
        }, false);
    });
})();