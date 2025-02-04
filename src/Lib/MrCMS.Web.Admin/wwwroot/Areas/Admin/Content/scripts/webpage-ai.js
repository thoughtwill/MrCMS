export function setupWebpageAiFunctions() {
    // Attach event listener for the "enhance content" button.
    $(document).on('click', 'button[data-ai-enhance-content]', async function () {
        const button = this;
        button.disabled = true;

        const titleElement = document.querySelector(button.dataset.title);
        const contentElement = document.querySelector(button.dataset.content);

        if (!titleElement || !contentElement) {
            console.error(
                'Title or content element not found:',
                button.dataset.title,
                button.dataset.content
            );
            button.disabled = false;
            return;
        }

        try {
            await fetchEnhanceContent(button.dataset.aiEnhanceContent, titleElement, contentElement);
        } catch (error) {
            console.error('Error in fetchEnhanceContent:', error);
        } finally {
            button.disabled = false;
        }
    });

    // Attach event listener for the "SEO content" button.
    $(document).on('click', 'button[data-ai-seo-content]', async function () {
        const button = this;
        button.disabled = true;

        const titleElement = document.querySelector(button.dataset.title);
        const descriptionElement = document.querySelector(button.dataset.description);
        const keywordsElement = document.querySelector(button.dataset.keywords);

        if (!titleElement || !descriptionElement || !keywordsElement) {
            console.error(
                'Title, description, or keywords element not found:',
                button.dataset.title,
                button.dataset.description,
                button.dataset.keywords
            );
            button.disabled = false;
            return;
        }

        try {
            await fetchSeoContent(button.dataset.aiSeoContent, titleElement, descriptionElement, keywordsElement);
        } catch (error) {
            console.error('Error in fetchSeoContent:', error);
        } finally {
            button.disabled = false;
        }
    });
}

/**
 * Processes a streamed fetch response and calls the provided tokenProcessor
 * for each complete JSON object received.
 *
 * @param {string} url - The URL to fetch.
 * @param {object} data
 * @param {(tokenObj: any) => void} tokenProcessor - Function to process each token object.
 */
async function processStream(url, data, tokenProcessor) {
    const response = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data)
    });

    const reader = response.body.getReader();
    const decoder = new TextDecoder();
    let buffer = '';
    let strippedLeadingBracket = false;

    while (true) {
        const {done, value} = await reader.read();
        if (done) break;

        const chunk = decoder.decode(value, {stream: true});
        buffer += chunk;

        if (!strippedLeadingBracket) {
            buffer = buffer.trim();
            if (buffer.startsWith('[')) {
                buffer = buffer.slice(1);
            }
            strippedLeadingBracket = true;
        }

        buffer = buffer.trim();
        if (buffer.endsWith(']')) {
            buffer = buffer.slice(0, -1);
        }

        const extraction = extractJsonObjects(buffer);
        extraction.objects.forEach((jsonStr) => {
            try {
                const tokenObj = JSON.parse(jsonStr);
                tokenProcessor(tokenObj);
            } catch (e) {
                console.error('JSON parsing error:', e, jsonStr);
            }
        });
        buffer = extraction.buffer;
    }

    if (buffer.length > 0) {
        const extraction = extractJsonObjects(buffer);
        extraction.objects.forEach((jsonStr) => {
            try {
                const tokenObj = JSON.parse(jsonStr);
                tokenProcessor(tokenObj);
            } catch (e) {
                console.error('Final JSON parsing error:', e, jsonStr);
            }
        });
    }
}

async function fetchEnhanceContent(webpageId, titleElement, contentElement) {
    const url = `/Admin/WebpageAi/EnhanceContent?webpageId=${webpageId}`;
    const parsedData = {
        title: '',
        content: ''
    };

    await processStream(url, {webpageId: webpageId}, (tokenObj) => {
        if (tokenObj.token === 'title') {
            parsedData.title += tokenObj.content;
            titleElement.value = parsedData.title;
        } else if (tokenObj.token === 'content') {
            parsedData.content += tokenObj.content;
            if (window.CKEDITOR && CKEDITOR.instances[contentElement.id]) {
                CKEDITOR.instances[contentElement.id].setData(parsedData.content);
            }
        }
    });
}

async function fetchSeoContent(webpageId, titleElement, descriptionElement, keywordsElement) {
    const url = `/Admin/WebpageAi/GenerateSeo?webpageId=${webpageId}`;
    const parsedData = {
        title: '',
        description: '',
        keywords: ''
    };

    await processStream(url, {webpageId: webpageId}, (tokenObj) => {
        if (tokenObj.token === 'title') {
            parsedData.title += tokenObj.content;
            titleElement.value = parsedData.title;
        } else if (tokenObj.token === 'description') {
            parsedData.description += tokenObj.content;
            descriptionElement.value = parsedData.description;
        } else if (tokenObj.token === 'keywords') {
            parsedData.keywords += tokenObj.content;
            keywordsElement.value = parsedData.keywords;
            const tags = parsedData.keywords.split(',');
            if (tags.length > 0) {
                $(keywordsElement).tagit('removeAll');
                tags.forEach((tag) => {
                    $(keywordsElement).tagit('createTag', tag);
                });
            }
        }
    });
}

/**
 * Attempts to extract complete JSON objects from a string buffer.
 *
 * This function uses a simple brace-counting algorithm. It returns an object:
 *  - objects: an array of complete JSON object strings.
 *  - buffer: the remainder of the string that could not be parsed into a complete object.
 *
 * @param {string} buffer - The text buffer to process.
 * @returns {{objects: string[], buffer: string}}
 */
function extractJsonObjects(buffer) {
    const objects = [];
    let openBraces = 0;
    let startIndex = -1;
    let lastProcessedIndex = 0;

    for (let i = 0; i < buffer.length; i++) {
        const char = buffer[i];
        if (char === '{') {
            if (openBraces === 0) {
                startIndex = i;
            }
            openBraces++;
        } else if (char === '}') {
            openBraces--;
            if (openBraces === 0 && startIndex !== -1) {
                const jsonStr = buffer.slice(startIndex, i + 1);
                objects.push(jsonStr);
                lastProcessedIndex = i + 1;
                startIndex = -1;
            }
        }
    }
    return {objects, buffer: buffer.slice(lastProcessedIndex)};
}
