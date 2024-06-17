function initialize(el) {
    el.select2({
        placeholder: "Search for a page",
        minimumInputLength: 1,
        ajax: { // instead of writing the function to execute the request we use Select2's convenient helper
            url: "/Admin/Webpage/Select2Search",
            data: function (params) {
                return {
                    term: params.term,
                    page: params.page || 1
                };
            },
            processResults: function (data, params) {
                params.page = params.page || 1;

                return {
                    results: data.items,
                    pagination: {
                        more: (params.page * 10) < data.total
                    }
                };
            }
        },
        templateResult: formatResult,
        templateSelection: formatSelection,
        escapeMarkup: function (m) {
            return m;
        }
    });
}

function formatResult(item) {
    if (item.loading) {
        return item.text;
    }

    return `${item.text} ${item.id.length ? `(${item.id})` : ''}`;
}

function formatSelection(item) {
    return `${item.text} ${item.id.length ? `(${item.id})` : ''}`;
}

export function setupWebpageUrlSelector() {
    $('[data-webpage-url-selector]').each(function (index, el) {
        initialize($(el));
    });
}