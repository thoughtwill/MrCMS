import cronParser from 'cron-parser';
import cronstrue from 'cronstrue';

// Function to get a human-readable description and validate cron expression
function getHumanReadableDescription(cronExpression) {
    try {
        const interval = cronParser.parseExpression(cronExpression);
        const nextOccurrence = interval.next().toString();
        const description = cronstrue.toString(cronExpression);

        return {
            description,
            nextOccurrence,
        };
    } catch (e) {
        console.error("Error in getHumanReadableDescription:", e);
        return null;
    }
}

function CronParser() {
    const cronInput = document.querySelector('[data-cron-expression]');
    if (!cronInput) {
        return;
    }

    const descriptionElement = document.querySelector('[data-cron-description]');
    const form = cronInput.closest('form');
    const setupButton = form.querySelector('input[type="submit"]');

    // Disable the setup button initially if the input is empty
    if (!cronInput.value.trim()) {
        setupButton.disabled = true;
    }

    cronInput.addEventListener('input', function () {
        const cronExpression = this.value.trim();

        if (cronExpression === '') {
            descriptionElement.textContent = '';
            descriptionElement.style.color = ''; // Reset text color
            cronInput.setCustomValidity('Invalid cron expression'); // Set custom validity for empty input
            setupButton.disabled = true; // Disable the setup button
            return;
        }

        try {
            const result = getHumanReadableDescription(cronExpression);

            if (result) {
                descriptionElement.innerHTML = `${result.description}.<br>Next occurrence: ${result.nextOccurrence}`;
                descriptionElement.style.color = 'black'; // Valid, so set text color to black
                cronInput.setCustomValidity(''); // Clear custom validity
                setupButton.disabled = false; // Enable the setup button
            } else {
                descriptionElement.textContent = "Invalid cron expression.";
                descriptionElement.style.color = 'red'; // Invalid, so set text color to red
                cronInput.setCustomValidity('Invalid cron expression'); // Set custom validity
                setupButton.disabled = true; // Disable the setup button
            }
        } catch (e) {
            console.error("Error in CronParser:", e);
            descriptionElement.textContent = "Error parsing cron expression.";
            descriptionElement.style.color = 'red'; // Invalid, so set text color to red
            cronInput.setCustomValidity('Invalid cron expression'); // Set custom validity
            setupButton.disabled = true; // Disable the setup button
        }
    });

    form.addEventListener('submit', function (event) {
        if (!cronInput.checkValidity()) {
            event.preventDefault(); // Prevent form submission if cron expression is invalid
            descriptionElement.textContent = "Please provide a valid cron expression.";
            descriptionElement.style.color = 'red';
        }
    });
}

export function SetupCronParser() {
    document.addEventListener('initialize-plugins', function () {
        CronParser();
    });
    CronParser();
}