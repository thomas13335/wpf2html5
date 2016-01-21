// ------------------------------------------------------------------------------------------------
// HtmlEditorView class
HtmlEditorView.prototype = new Control();
HtmlEditorView.prototype.constructor = HtmlEditorView;

function HtmlEditorView(ctrl) {
    Control.prototype.constructor.call(this, ctrl);
    this.$type = "HtmlEditorView";
    this.ctrl = ctrl;

    trace("constructing HtmlEditorView on " + ctrl + " ... " + ctrl.id);

    var area = document.createElement("textarea");
    area.setAttribute("class", "htmleditor");
    area.setAttribute("height", "100px");
    area.setAttribute("rows", "4");

    ctrl.appendChild(area);

    area.onchange = function () {
        var b = bind.getbinding(ctrl.id, "Text");
        if (b !== undefined) {
            b.updatesource(area.value);
        }
    };

    area.onkeyup = function () {
        var b = bind.getbinding(ctrl.id, "Text");
        if (b !== undefined) {
            b.updatesource(area.value);
        }
    }

    this.area = area;
}

HtmlEditorView.prototype.set_Text = function (value) {
    this.area.value = value ? value : "";
}

HtmlEditorView.prototype.get_Text = function () {
    return this.area.value;
}

