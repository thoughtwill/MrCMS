var editor;
const editorSelector = '.code-css-enabled:not(.editor-initialized)';
export function initializeCssEditor() {
    var editors = $(editorSelector);
    editors.each(function (index) {
        var $this = $(this);
        var id = "aceeditor_css_" + index;
        editor = $('<div id="' + id + '" style="widht=100%;height:'+ ($this.data("height") ?? "400px") +'" class="form-control">');

        $this.addClass('editor-initialized');
        
        editor.html($this.html())
        $this.hide().after(editor);
        editor = ace.edit(id);
        //editor.setTheme("ace/theme/monokai");
        editor.getSession().setMode('ace/mode/css');
        editor.getSession().on("change", function () {
            $this.val(editor.getSession().getValue());
        });
    });
}