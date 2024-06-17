import {setupWebpageUrlSelector} from "./webpage-url-selector";
import {initializeTextEditor} from "./text-editor";
let _isFirstTime;
let _cachedTemplates = {};

export function initiateContentTemplate(){
    _isFirstTime = true;
    formSubmit();
    refreshUI();
}

function formSubmit() {
    $('[data-content-template-input]').closest('form').on('submit', function (e) {
        let $form = $('<form>');
        $('[data-content-template-input]').map((x, el) => {
            let value;
            if (el.tagName.toLowerCase() === 'textarea' && typeof CKEDITOR != 'undefined' && CKEDITOR.instances[el.id]) {
                value = CKEDITOR.instances[el.id].getData();
            } else {
                value = el.type === 'checkbox' ? el.checked : el.value;
            }
            $('<input>').attr({
                type: 'hidden',
                name: el.name,
                value: value
            }).appendTo($form);
            //Maybe remove the element from the dom or keep it if the submit is faild for some reasons
            //$(el).remove()
        });

        let obj = $form.serializeJSON();
        $('<input>').attr({
            type: 'hidden',
            name: 'Properties',
            value: JSON.stringify(obj)
        }).appendTo($(this));
    })
}

function refreshUI() {
    $(".content-template-array-container .add-row").each(function () {
        let arrayContainer = $(this).closest('.content-template-array-container');

        if (_isFirstTime) {
            const _cardTemplate = arrayContainer.children("[data-row-template]").html();
            const _arrayId = $(this).data('array-id');
            _cachedTemplates[_arrayId] = _cardTemplate;
        }

        //To make sure the event is not duplicated and we have a backup row if all rows are deleted
        $(this).off().click(function () {

            const _clone = $(_cachedTemplates[$(this).data('array-id')]);
            const _size = $(this).closest('.content-template-array-container').children('.card').length;
            _clone.find('[data-content-template-input]').each(function (){
                let elementId = $(this).attr('id') + '_' + _size;
                $(this).attr('id', elementId);
            });

            //find a better way to integrate the custom inputs
            const _uniqueInputs = _clone.find('[data-unique-key]');
            if (_uniqueInputs.length > 0) {
                _uniqueInputs.each(function () {
                    const _id = "uk_" + Math.random().toString(36).substr(2, 5);
                    $(this).val(_id);
                    $(this).next().html(_id);
                })
            }
            _clone.find('[data-type=media-selector], [class=media-selector]').mediaSelector();
            _clone.find('.rowIndex').html(_size + 1);
            $(this).closest('.content-template-array-container').children(':last-child').before(_clone);

            setupWebpageUrlSelector();
            initializeTextEditor();
            refreshUI();
        });
    });

    $(".content-template-array-container .delete-row").off().click(function () {
        const didConfirm = confirm("Are you sure You want to delete");
        if (didConfirm === true) {
            $(this).closest('.card').remove();
            resetArrayIndexes();
        }
    });

    $(".content-template-array-container").each((_, element) => {
        const $element = $(element);
        $element.sortable({
            // handle: ".sort-row",
            items: "> .card",
            update: function (event, ui) {
                resetArrayIndexes();
            }
        });
    });

    _isFirstTime = false;
    dragAndDrop();
}

function resetArrayIndexes(){
    $('.content-template-array-container').each(function(){
        let arrayParent= $(this);
        let index = 0;
        arrayParent.children('.card').each(function (){
            let cardItem = $(this);
            cardItem.find('.rowIndex').html(++index);
        });
    });
}

function dragAndDrop() {
    if (document.querySelectorAll("[content-template-drag]").length)
    {
        document.querySelectorAll("[content-template-drag]").forEach(function (el) {
            el.ondragstart = function (e) {
                const dataTransfer = e.dataTransfer;
                dataTransfer.effectAllowed
                dataTransfer.setData("Text", e.target.dataset.text);
            }
        });
    }
}