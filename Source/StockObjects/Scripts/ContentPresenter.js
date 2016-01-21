// ------------------------------------------------------------------------------------------------
// ContentPresenter class

ContentPresenter.prototype = new Control();
ContentPresenter.prototype.constructor = ContentPresenter;

ContentPresenter.prototype.$static = {
    BaseClass: "Control",
    ContentProperty: {}
}

function ContentPresenter(ctrl) {
    this.$type = "ContentPresenter";
    this.ctrl = ctrl;
    this.content = null;
}

ContentPresenter.prototype.set_Content = function (value) {
    var tg = this.ctrl;
    var presenter = null;

    while (tg.firstChild != null) {
        // clear bindings before the object is removed.
        Control_purge(tg.firstChild);
        tg.removeChild(tg.firstChild);
    }

    if (value === undefined || value == null) {
        return;
    }

    if (value["$type"] === undefined) {
        // render as text
        trace("WARNING: no type identification on 'Content', render as text: '" + value + "'.");

        presenter = document.createElement("span");
        presenter.appendChild(document.createTextNode(value));
        tg.appendChild(presenter);
    }
    else {
        // use DataTemplate ...
        presenter = Control_generatecontainer(this.ctrl, 'div');

        tg.appendChild(presenter);

        // apply the data template
        Control_applytemplate(this.ctrl, presenter, value);
    }

}
