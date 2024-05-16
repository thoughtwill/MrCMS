function getBlockHolder() {
    return $('[data-content-admin-blocks]');
}

function getItemBlocks() {
    return $('[data-admin-item-blocks-order]');
}

function getEditorHolder() {
    return $('[data-content-admin-editor]');
}

function loadBlocks(url) {
    const blocks = getBlockHolder();
    if (!blocks.length) {
        return;
    }
    url = url ?? $('[data-content-admin-blocks-current]').data('content-admin-blocks-current') ?? blocks.data('content-admin-blocks');
    $.get(url, function (response) {
        blocks.html(response);
        InitSortBlocks();
        InitToggleHideBlock();
        InitSortItemBlocks();
    })
}

function InitSortBlocks() {
    const blocks = getBlockHolder();
    if (!blocks.length) {
        return;
    }
    blocks.children("ul").sortable({
        handle: ".sort-handle",
        update: function (event, ui) {
            let sortList = [];
            blocks.children("ul").children("li[data-order]").each(function (index, domElement) {
                let order = index + 1;
                $(domElement).data("order", order);
                sortList.push({
                    id: $(domElement).data("id"),
                    order: order
                });
            });
            let url = blocks.children("ul").data("admin-blocks-order");
            $.post(url, {list: sortList})
                .done(function (data) {
                    reloadPreview();
                })
                .fail(function () {
                    loadBlocks();
                });
        }
    });
}

function InitSortItemBlocks() {
    const itemBlocks = getItemBlocks();
    if (!itemBlocks.length) {
        return;
    }

    itemBlocks.each(function () {
        let itemBlock = $(this);
        itemBlock.sortable({
            handle: ".child-sort-handle",
            update: function (event, ui) {
                let sortList = [];
                let blockId = $(event.target).closest('[data-id]').data("id");
                $(event.target).children("li[data-id]").each(function (index, domElement) {
                    let order = index + 1;
                    sortList.push({
                        id: $(domElement).data("id"),
                        order: order
                    });
                });
                let url = $(event.target).data("admin-item-blocks-order");
                $.post(url, {id: blockId, list: sortList})
                    .done(function (data) {
                        reloadPreview();
                        /*loadBlocks();*/ //uncomment for re-order ui again
                    })
                    .fail(function () {
                        loadBlocks();
                    });
            }
        });
    });


}

function InitToggleHideBlock() {
    const blocks = getBlockHolder();
    if (!blocks.length) {
        return;
    }

    blocks.find("[data-content-admin-block-hide]").on("click", function () {
        let self = $(this);
        let url = self.data("content-admin-block-hide");
        $.post(url, {id: self.data("id")})
            .done(function (data) {
                loadBlocks();
                reloadPreview();
            });
    });
}

function ToggleExpandBlock() {
    const editor = getEditorHolder();
    if (!editor.length) {
        return;
    }

    if (editor.closest("[data-content-parent]").hasClass("expand")) {
        collapseEditor();
        triggerContentAdminExpandEditorEvent(false);
    } else {
        expandEditor();
        triggerContentAdminExpandEditorEvent(true);
    }
}

function loadEditor(url) {
    const editor = getEditorHolder();
    if (!editor.length) {
        return;
    }
    if (url) {
        editor.closest("[data-content-parent]").removeClass("open");
        collapseEditor();
        $.get(url, function (response) {
            editor.html(response);
            window.admin.initializePlugins();
            editor.closest("[data-content-parent]").addClass("open");
        })
    } else {
        editor.closest("[data-content-parent]").removeClass("open")
        collapseEditor();
        let blocks = getBlockHolder();
        blocks.find('li.list-group-item-primary').removeClass('list-group-item-primary');
    }
}

function hideEditor() {
    const editor = getEditorHolder();
    if (!editor.length) {
        return;
    }
    editor.closest("[data-content-parent]").removeClass("open");
    collapseEditor();

    let blocks = getBlockHolder();
    blocks.find('li.list-group-item-primary').removeClass('list-group-item-primary');
}

function expandEditor() {
    const editor = getEditorHolder();
    if (!editor.length) {
        return;
    }
    editor.closest("[data-content-parent]").addClass("expand");
    $('[data-content-admin-expand-editor] [data-expand]').addClass('d-none');
    $('[data-content-admin-expand-editor] [data-collapse]').removeClass('d-none');
}

function collapseEditor() {
    const editor = getEditorHolder();
    if (!editor.length) {
        return;
    }
    editor.closest("[data-content-parent]").removeClass("expand");
    $('[data-content-admin-expand-editor] [data-expand]').removeClass('d-none');
    $('[data-content-admin-expand-editor] [data-collapse]').addClass('d-none');
}

function loadEditorFromDataKeyUrl(link, dataKey) {
    const url = link.data(dataKey);
    if (!url) {
        return false;
    }

    loadEditor(url);
    return false;
}

function loadBlocksFromDataKeyUrl(link, dataKey) {
    const url = link.data(dataKey);
    if (!url) {
        return false;
    }

    loadBlocks(url);
    return false;
}

function openBlock(event) {
    const link = $(event.currentTarget);
    return loadBlocksFromDataKeyUrl(link, 'content-admin-block-open');
}

function closeBlock(event) {
    const link = $(event.currentTarget);
    return loadBlocksFromDataKeyUrl(link, 'content-admin-block-close');
}

function selectBlock(event) {
    const link = $(event.currentTarget);
    loadEditorFromDataKeyUrl(link, 'content-admin-editor')
    loadBlocksFromDataKeyUrl(link, 'content-admin-block-select');
    return false;
}

function mouseEnterBlock(event){
    const link = $(event.currentTarget);
    highlightPreviewBlock(link);
}

function mouseLeaveBlock(event){
    clearHighlightPreviewBlock();
}

function clearHighlightPreviewBlock(){
    let iframe = document.querySelector('[data-content-admin-preview-pane]');
    if (iframe) {
        let iframeDocument = iframe.contentDocument || iframe.contentWindow.document;
        if (iframeDocument) {
            let allPreviewBlocks = iframeDocument.querySelectorAll('.mrcms-preview-block[data-content-block-id]');
            $(allPreviewBlocks).removeClass('highlight');
        }
    }
}

function highlightPreviewBlock(element) {
    let blockId = element.attr('data-id');
    let iframe = document.querySelector('[data-content-admin-preview-pane]');
    if (iframe) {
        let iframeDocument = iframe.contentDocument || iframe.contentWindow.document;
        if (iframeDocument) {
            let targetPreviewBlock = iframeDocument.querySelector(`.mrcms-preview-block[data-content-block-id="${blockId}"]`);
            if (targetPreviewBlock) {
                $(targetPreviewBlock).addClass("highlight");
                targetPreviewBlock.scrollIntoView({
                    behavior: 'smooth',
                    block: 'center'
                });
            }
        }
    }
}


// function setContentPreviewBlockClick() {
//     let iframe = document.querySelector('[data-content-admin-preview-pane]');
//     if (iframe) {
//         const parentDoc = parent.document;
//
//         $(iframe).contents().find("body").append(`
//         <script>
//             let allPreviewBlocks= document.querySelectorAll('.mrcms-preview-block[data-content-block-id]');
//             allPreviewBlocks.forEach(function(previewBlockItem){
//                previewBlockItem.addEventListener('click',function(item){
//                     console.log(item);
//                 }); 
//             });
//         </script>`);
//
//     }
// }

function removeBlock(event) {
    const link = $(event.currentTarget);
    if (confirm('Are you sure you want to delete this block?')) {
        const url = link.data('content-admin-block-remove');
        $.post(url, function () {
            loadBlocks();
            loadEditor();
            reloadPreview();
        });
    }
    return false;
}

function addChild(event) {
    const link = $(event.currentTarget);
    const url = link.data('content-admin-add-child');
    $.post(url, function () {
        loadBlocks();
        loadEditor();
        reloadPreview();
    });
    return false;
}

function reloadPreview() {
    document.getElementById('content-admin-preview-pane').contentWindow.location.reload();
}

function saveEditor(event) {
    const form = $(event.currentTarget);
    const obj = form.serializeArray().reduceRight((prev, arr) => {
        prev[arr.name] = arr.value;
        return prev;
    }, {});
    const url = form.attr('action');
    $.post(url, obj, function () {
        loadBlocks();
        loadEditor();
        reloadPreview();
        /*alert('success');*/
    });

    return false;
}

function updateContentEditor(e) {
    let iframeSrc = new URL(this.src);
    let targetUrl = this.contentWindow.location.href;
    let currentUrl = iframeSrc.href;
    if (targetUrl !== currentUrl) {
        window.location = `/Admin/ContentVersion/EditByUrl?url=${targetUrl}`;
    }
    // else {
    //     if (!$(this).attr('data-initiated')) {
    //         setContentPreviewBlockClick();
    //         $(this).attr('data-initiated', true);
    //     }
    // }
}

function triggerContentAdminExpandEditorEvent(isExpanded) {
    const event = new CustomEvent('ContentBlockEditorExpand', {
        detail: {
            message: `Content Block Editor has been ${isExpanded ? 'expanded' : 'collapsed'}.`,
            isExpanded: isExpanded
        }
    });

    // Dispatch the event on the document
    document.dispatchEvent(event);
}

export function setupContentAdmin() {
    loadBlocks();

    $(document).on('click', '[data-content-admin-block-open]', openBlock)
    $(document).on('click', '[data-content-admin-block-close]', closeBlock)
    $(document).on('click', '[data-content-admin-block-select]', selectBlock)
    $(document).on('mouseenter', '[data-content-admin-nav] li[data-id]', mouseEnterBlock)
    $(document).on('mouseover', '[data-content-admin-nav] li[data-id]', mouseEnterBlock)
    $(document).on('mouseleave', '[data-content-admin-nav] li[data-id]', mouseLeaveBlock)
    $(document).on('click', '[data-content-admin-block-remove]', removeBlock)
    $(document).on('click', '[data-content-admin-add-child]', addChild)
    $(document).on('submit', '[data-content-admin-save-editor]', saveEditor)
    $(document).on('click', '[data-content-admin-hide-editor]', hideEditor)
    $(document).on('click', '[data-content-admin-expand-editor]', ToggleExpandBlock);
    $('[data-content-admin-preview-pane]').on('load', updateContentEditor);
}