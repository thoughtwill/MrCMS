var editor;
const editorSelector = '.code-enabled:not(.editor-initialized)';
export function initializeHtmlEditor() {
    var editors = $(editorSelector);
    editors.each(function (index) {
        var $this = $(this);
        var id = "aceeditor_" + index;
        
        $this.addClass('editor-initialized');
        
        editor = $('<div id="' + id + '" style="widht=100%;height:'+ ($this.data("height") ?? "400px") +'" class="form-control">');
        editor.html($this.html())
        $this.hide().after(editor);
        editor = ace.edit(id);
        //editor.setTheme("ace/theme/monokai");
        editor.getSession().setMode('ace/mode/html');
        editor.getSession().on("change", function () {
            $this.val(editor.getSession().getValue());
        });
    });
}