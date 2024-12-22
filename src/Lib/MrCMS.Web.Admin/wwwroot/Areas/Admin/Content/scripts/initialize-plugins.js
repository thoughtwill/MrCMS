import {initTagging} from "./tagging";
import {initSortable} from "./sort";
import {initMediaUploader} from "./media-uploader";
import {initAceEditor} from "./ace";

export function initializePlugins() {
    CKEDITOR.replaceAll('ckedit-enabled');
    CKEDITOR.on('instanceReady', (ev) => {
        if (window.location !== window.parent.location) // if in iframe, trigger resize.
            top.$(top).trigger('resize');
    });
    $('[data-type=media-selector], [class=media-selector]').mediaSelector();
    initMediaUploader();
    const form = $('form');
    form.removeData("validator");
    form.removeData("unobtrusiveValidation");
    form.find('input, select').each((index, element) => {
        $.data(element, "previousValue", null);
    });
    $.validator.unobtrusive.parse("form");
    initTagging();
    $("[data-color-picker]").each(function (index, element) {
        const $element = $(element);
        $element.spectrum({
            preferredFormat: "hex",
            showInput: true
        });
    });

    $('form[data-are-you-sure]').each(function (index, element) {
        const form = $(element);
        form.areYouSure({message: form.data('are-you-sure')});
    });
    initSortable();
    initAceEditor();

    $(document).trigger('initialize-plugins', {});
}


export function registerGlobalFunctions() {
    window.admin ??= {};
    window.admin.initializePlugins = initializePlugins;
}
