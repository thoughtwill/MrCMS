export function initAceEditor() {
    var editor;
    $('.code-enabled').each(function (index) {
        var $this = $(this);
        var id = "aceeditor_" + index;
        editor = $('<div id="' + id + '" style="widht=100%;height:400px" class="form-control">');
        editor.html($this.html())
        $this.hide().after(editor);
        editor = ace.edit(id);
        // editor.setTheme("ace/theme/tomorrow_night");
        editor.getSession().setMode('ace/mode/html');
        editor.getSession().on("change", function () {
            $this.val(editor.getSession().getValue());
        });
        editor.setOptions({
            enableBasicAutocompletion: true,
            enableSnippets: true,
            fontSize: "16px"
        });
    });
}