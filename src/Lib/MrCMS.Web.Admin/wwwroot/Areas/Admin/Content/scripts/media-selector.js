import { setCloseButtonPosition } from "./setup-featherlight";
import { MediaUploader } from "./media-uploader";

function MediaSelector(options) {
    let element;
    const settings = jQuery.extend(MediaSelector.defaults, options);
    let self,
        timer,
        mediaUploader,
        $;

    function toggle(link) {
        const id = link.data('media-toggle');
        const thisElement = element.find('[data-file-result=' + id + ']');
        const others = element.find('[data-file-result]').not(thisElement);
        others.each(function () {
            const result = $(this);
            hideResult(result);
        });
        const shown = thisElement.data('content-shown');
        if (shown) {
            hideResult(thisElement);
        } else {
            thisElement.find(settings.mediaResultSelector).slideDown();
            thisElement.data('content-shown', true);
        }
    };

    function hideResult(result) {
        const shown = result.data('content-shown');
        if (shown) {
            result.find(settings.mediaResultSelector).slideUp();
            deselectAll(result);
            result.data('content-shown', false);
        }
    };

    function submitForm(event) {
        event.preventDefault();
        const form = element.find(settings.formSelector);
        const url = form.attr('action') + '?' + form.serialize();
        updateResults(url);
    };

    function changePage(event) {
        event.preventDefault();
        const url = $(this).attr('href');
        updateResults(url);
    };

    function updateResults(url) {
        $.get(url, function (response) {
            const newResults = $(response).find(settings.resultsHolderSelector);
            $(element).find(settings.resultsHolderSelector).replaceWith(newResults);
            initializeMediaUploader();
        });
    };

    function selectFile(event) {
        const file = $(this).data('file');
        element.find('[data-action="select"][data-file=' + file + ']').removeAttr('disabled');
    };

    function deselectAll(result) {
        result.find('input[data-file]').removeAttr('checked');
        result.find('[data-action="select"]').attr('disabled', 'disabled');
    };

    function selected(event) {
        event.preventDefault();
        const fileValue = element.find('input[data-file]').filter(':checked').val();
        if (fileValue !== '') {
            if (settings.onSelected) {
                $.get('/Admin/MediaSelector/GetFileInfo/', { value: fileValue }, function (info) {
                    settings.onSelected(info);
                });
            }
        }
    };

    function initializeMediaUploader() {
        mediaUploader = new MediaUploader(element, {
            onFileUploadStopped: function (event, dropzone) {
                const form = element.find(settings.formSelector);
                const url = form.attr('action') + '?' + form.serialize();
                updateResults(url);
                dropzone.removeAllFiles();
            }
        });
        mediaUploader.init();
    };

    return {
        show: function (jQ) {
            self = this;
            $ = jQ;
            const link = $('<a>').attr('href', settings.selectorUrl);
            link.hide();
            link.featherlight({
                type: 'iframe',
                iframeWidth: 820,
                afterOpen: function () {
                    element = $('.featherlight-inner').contents();
                    element.find('form').css('margin', '0');
                    setCloseButtonPosition(this.$instance);
                    self.init();
                },
                beforeOpen: function () {
                    $(".mrcms-edit-menu", document).hide();
                },
                onResize: function () {
                    if (this.autoHeight) {
                        // Shrink:
                        this.$content.css('height', '10px');
                        // Then set to the full height:
                        this.$content.css('height', this.$content.contents().find('body')[0].scrollHeight);
                    }
                    setCloseButtonPosition(this.$instance);
                }
            }).click().remove();
            return self;
        },
        init: function () {
            self = this;
            initializeMediaUploader();
            $(element).on('click', 'div.header', function (event) {
                event.preventDefault();
                const link = $(this).find(settings.mediaToggleSelector);
                if (link.length) {
                    toggle(link);
                }
            });
            $(element).on('submit', settings.formSelector, submitForm);
            $(element).on('change', '#CategoryId', submitForm);
            $(element).on('keyup', '#Query', function (event) {
                timer && clearTimeout(timer);
                timer = setTimeout(function () {
                    submitForm(event);
                }, 300);
            });
            $(element).on('click', '.pagination a', changePage);
            $(element).on('click', 'input[data-file]', selectFile);
            $(element).on('click', '[data-action="select"]', selected);

            return self;
        },
        getSettings: function () {
            return settings;
        }
    };
};

function MediaSelectorWrapper(el, options) {
    const element = el,
        settings = $.extend(MediaSelectorWrapper.defaults, options);
    let self,
        preview,
        removeButton,
        selectButton,
        generateButton,
        buttonHolder,
        eventsRegistered = false;

    function getValue() {
        return element.val();
    }

    function valueIsSet() {
        const value = getValue();
        return value != null && value != '';
    }

    function getAltInput(value) {
        const altFormGroup = $('<div>').addClass('form-group has-feedback hide').attr('id', el.attr('id') + '-alt-holder');
        const altId = el.attr('id') + '-alt';
        const altLabel = $('<label>').html('Alt').attr('for', altId);
        altFormGroup.append(altLabel);
        const altInput = $('<input>').attr('type', 'text').addClass('form-control input-sm').attr('id', altId).attr('data-url', value);
        altFormGroup.append(altInput);
        const altFeedback = $('<span class="fa fa-ok form-control-feedback hide" aria-hidden="true"></span>');
        altFormGroup.append(altFeedback);
        $.get(settings.altUrl, { url: value }, function (response) {
            altInput.val(response.alt);
            altFormGroup.removeClass('hide');
            altInput.on('blur', updateAlt);
        });
        return altFormGroup;
    }

    function showSaved(inputElement) {
        const formGroup = inputElement.closest('.form-group');
        formGroup.addClass('has-success');
        formGroup.find('.form-control-feedback').removeClass('hide');
        clearTimeout(inputElement.removeSuccess);
        inputElement.removeSuccess = setTimeout(function () {
            formGroup.removeClass('has-success');
            formGroup.find('.form-control-feedback').addClass('hide');
        }, 2000);
    }

    function updateAlt(event) {
        const input = $(event.target);
        $.post(settings.updateAltUrl, { url: input.data('url'), value: input.val() }, function (response) {
            if (response) {
                showSaved(input);
            }
        });
    }

    function updateDescription(event) {
        const input = $(event.target);
        $.post(settings.updateDescriptionUrl, { url: input.data('url'), value: input.val() }, function (response) {
            if (response) {
                showSaved(input);
            }
        });
    }

    function getDescriptionInput(value) {
        const descriptionFormGroup = $('<div>').addClass('form-group has-feedback hide').attr('id', el.attr('id') + '-description-holder');
        const descriptionId = el.attr('id') + '-description';
        const descriptionLabel = $('<label>').html('Description').attr('for', descriptionId);
        descriptionFormGroup.append(descriptionLabel);
        const descriptionInput = $('<input>').addClass('form-control input-sm').attr('id', descriptionId).attr('data-url', value);
        descriptionFormGroup.append(descriptionInput);

        const descriptionFeedback = $('<span class="fa fa-ok form-control-feedback hide" aria-hidden="true"></span>');
        descriptionFormGroup.append(descriptionFeedback);
        $.get(settings.descriptionUrl, { url: value }, function (response) {
            descriptionInput.val(response.description);
            descriptionFormGroup.removeClass('hide');
            descriptionInput.on('blur', updateDescription);
        });
        return descriptionFormGroup;
    }

    function getPreview() {
        if (valueIsSet()) {
            const value = getValue();
            if (isImage(value)) {
                const holder = $('<div>').addClass('row media-preview');
                const imageCol = $('<div>').addClass('col-auto pt-3');
                imageCol.append($('<img src="' + value + '" class=\"img-fluid img-thumbnail\" />'));
                imageCol.append(buttonHolder);
                holder.append(imageCol);
                const dataCol = $('<div>').addClass('col pt-3');

                const altFormGroup = getAltInput(value);
                dataCol.append(altFormGroup);

                const descriptionFormGroup = getDescriptionInput(value);
                dataCol.append(descriptionFormGroup);

                holder.append(dataCol);
                return holder;
            } else {
                return $('<span>' + value + '</span>');
            }
        } else {
            buttonHolder.appendTo($('[data-media-selector-holder-' + el.attr('id') + ']'));
            return $('<img src="' + settings.noImageSelectedImage + '" style="' + settings.previewStyle + '" />');
        }
    };

    function isImage(image) {
        const extension = image.split('.').pop().toLowerCase();
        const imageExtensions = ["jpg", "jpeg", "gif", "png", "webp", "svg"];

        let imageCheck = false;
        for (let i = 0; i < imageExtensions.length; i++) {
            if (extension == imageExtensions[i]) {
                imageCheck = true;
            }
        }
        return imageCheck;
    };
    return {
        init: function () {
            self = this;
            if (element.is("input")) {
                const para = $('<p>').attr('data-media-selector-holder-' + el.attr('id'), 'true');
                buttonHolder = $('<div>').addClass("d-flex mt-1").appendTo(para);
                preview = $('<div>');
                element.before(preview);
                selectButton = $('<button>').addClass(settings.selectClasses).html(settings.selectMessage).attr('data-media', 'select')
                    .appendTo(buttonHolder);
                removeButton = $('<button>').addClass(settings.removeClasses).html(settings.removeMessage).attr('data-media', 'remove')
                    .appendTo(buttonHolder);
                generateButton = $('<button>').addClass(settings.generateClasses).html(settings.generateMessage).attr('data-toggle', 'modal').attr('data-target', '#aiImageGenerateModal')
                    .appendTo(buttonHolder);
                element.hide().after(para);
                self.update();
                self.registerEvents();

            }
        },
        registerEvents: function () {
            if (!eventsRegistered) {
                selectButton.click(function (event) {
                    event.preventDefault();
                    self.show();
                });
                removeButton.click(function (event) {
                    event.preventDefault();
                    self.reset();
                });
            }
            eventsRegistered = true;
        },
        update: function () {
            const value = this.getValue();
            if (value !== null && value !== '') {
                removeButton.show();
                buttonHolder.addClass('btn-group');
            } else {
                removeButton.hide();
                buttonHolder.removeClass('btn-group');
            }
            const newPreview = getPreview();
            preview.replaceWith(newPreview);
            preview = newPreview;
        },
        getValue: function () {
            return getValue();
        },
        setValue: function (value) {
            element.val(value);
            self.update();
        },
        show: function () {
            const mediaSelector = new MediaSelector({
                onSelected: self.onSelected,
                element: element,
                name: $(element).attr('id')
            });
            mediaSelector.show($);
        },
        reset: function () {
            self.setValue('');
        },
        onSelected: function (data) {
            self.setValue(data.url);
            $.featherlight.close();
        }
    };
};
MediaSelectorWrapper.defaults =
{
    noImageSelectedImage: '/Areas/Admin/Content/img/no-media-selected.jpg',
    previewStyle: 'max-height: 200px; max-width:200px;',
    selectClasses: 'btn btn-sm btn-success ',
    selectMessage: 'Select media...',
    removeClasses: 'btn btn-sm btn-danger ',
    removeMessage: 'Remove media...',
    generateClasses: 'btn btn-sm btn-ai ml-2 mt-1',
    generateMessage: '<i class="fa fa-magic" aria-hidden="true"></i>',
    altUrl: '/admin/mediaselector/alt',
    updateAltUrl: '/admin/mediaselector/updatealt',
    descriptionUrl: '/admin/mediaselector/description',
    updateDescriptionUrl: '/admin/mediaselector/updatedescription'
};
MediaSelector.defaults =
{
    selectorUrl: '/Admin/MediaSelector/Show',
    formSelector: '#search-form',
    resultsHolderSelector: '#results',
    mediaResultSelector: '[data-media-result]',
    mediaToggleSelector: '[data-media-toggle]',
    onSelected: function (info) {
    }
};

export function registerPlugins() {
    window.MediaSelector = MediaSelector;
    $.fn.mediaSelector = function (method) {
        const methods = {
            init: function (options) {
                return this.each(function () {
                    const self = $(this);
                    if (self.hasClass("media-selector-initialized")) {
                        return;
                    }
                    const wrapper = new MediaSelectorWrapper(self, options);
                    wrapper.init();
                    self.addClass("media-selector-initialized");
                });
            }
        };
        // Method calling logic
        if (methods[method]) {
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        } else if (typeof method === 'object' || !method) {
            return methods.init.apply(this, arguments);
        } else {
            return $.error('Method ' + method + ' does not exist on mediaselector');
        }
    };

    $.fn.mediaSelectorPopup = function (method) {
        const methods = {
            init: function (options) {
                return this.each(function () {
                    const self = $(this);
                    const selector = new MediaSelector(self, options);
                    selector.init();
                    $.data(self[0], 'media-selector', selector);
                    if (parent.$) {
                        parent.$.data(self[0], 'media-selector', selector);
                    }
                });
            }
        };
        // Method calling logic
        if (methods[method]) {
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        } else if (typeof method === 'object' || !method) {
            return methods.init.apply(this, arguments);
        } else {
            return $.error('Method ' + method + ' does not exist on mediaselector');
        }
    };
}
