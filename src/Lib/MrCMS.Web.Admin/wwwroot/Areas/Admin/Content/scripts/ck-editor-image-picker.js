(function ($, doc) {
    function toggle(link) {
        var id = link.data('media-toggle');
        var thisElement = $(doc).find('[data-file-result=' + id + ']');
        var others = $(doc).find('[data-file-result]').not(thisElement);
        others.each(function () {
            var result = $(this);
            hideResult(result);
        });
        var shown = thisElement.data('content-shown');
        if (shown) {
            hideResult(thisElement);
        } else {
            thisElement.find('[data-media-result]').slideDown();
            thisElement.data('content-shown', true);
            link.html('Hide');
        }
    }

    function hideResult(result) {
        var shown = result.data('content-shown');
        if (shown) {
            result.find('[data-media-result]').slideUp();
            result.find('[data-media-toggle]').html('Show');
            //deselectAll(result);
            result.data('content-shown', false);
        }
    }
    function selectFile(event) {
        var file = $(this).data('file');
        $(doc).find('[data-action="select"][data-file=' + file + ']').removeAttr('disabled');

    }

    function selected(event) {
        event.preventDefault();
        var fileValue = $(doc).find('input[data-file]').filter(':checked').val();
        if (fileValue !== '') {
            $.get('/Admin/MediaSelector/GetFileInfo/', { value: fileValue }, function (info) {
                var funcNum = $('#CKEditorFuncNum').val();
                window.opener.CKEDITOR.tools.callFunction(funcNum, info.url, function() {
                    // Get the reference to a dialog window.
                    var dialog = this.getDialog();
                    // Check if this is the Image Properties dialog window.
                    if (dialog.getName() == 'image') {
                        // Get the reference to a text field that stores the "alt" attribute.
                        var element = dialog.getContentElement('info', 'txtAlt');
                        // Assign the new value.
                        if (element)
                            element.setValue(info.alt);

                        var classElement = dialog.getContentElement('advanced', 'txtGenClass');
                        if (classElement) {
                            classElement.setValue('img-fluid');
                        }

                        var titleElement = dialog.getContentElement('advanced', 'txtGenTitle');
                        if (titleElement) {
                            titleElement.setValue(info.description);
                        }
                    }
                    dialog.originalElement.addClass('img-fluid');
                    // Return "false" to stop further execution. In such case CKEditor will ignore the second argument ("fileUrl")
                    // and the "onSelect" function assigned to the button that called the file manager (if defined).
                    // return false;
                });
                window.close();
            });
        }
    }

    function initializeMediaUploader() {
        var mediaUploader = new MediaUploader($(document), {
            onFileUploadStopped: function (event, dropzone) {
                var form = $(document).find("#search-form");
                var url = form.attr('action') + '?' + form.serialize();
                updateResults(url);
                dropzone.removeAllFiles();
            }
        });
        mediaUploader.init();
    };

    function updateResults(url) {
        $.get(url, function (response) {
            var newResults = $(response).find("#results");
            $(document).find($("#results")).replaceWith(newResults);
            initializeMediaUploader();
        });
    };

    $(function () {
        initializeMediaUploader();
        $(doc).on('click', 'div.header',
            function (event) {
                event.preventDefault();
                var link = $(this).find('[data-media-toggle]');
                if (link.length) {
                    toggle(link);
                }
            });
        $(doc).on('click', 'input[data-file]', selectFile);
        $(doc).on('click', '[data-action="select"]', selected);
    });
})(jQuery, document);