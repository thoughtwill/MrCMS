export function setupMediaCategory() {

    var container = $("#add-media-category");
    if (container.length === 0) {
        return;
    }

    $.fn.delayKeyup = function (callback, ms) {
        let timer = 0;
        const el = $(this);
        $(this).keyup(function () {
            clearTimeout(timer);
            timer = setTimeout(function () {
                callback(el);
            }, ms);
        });
        return $(this);
    };
    $(function () {

        $("#Name").delayKeyup(function () {
            setStandardUrl();
        }, 100);

        function setStandardUrl() {
            let parentUrl = $("#ParentUrl").val();
            if (parentUrl !== '') {
                parentUrl += "/";
            }
            $("#Path").val(parentUrl + $("#Name").val().trim().replace(/\W/g, '-').toLowerCase());
        }

        $("form").validate();
    });

}