
PathControl.prototype = new Control();
PathControl.prototype.constructor = PathControl;

PathControl.prototype.$static = {
    BaseClass: "Control",
    ParentProperty: {},
    PathProperty: {},
    ModeProperty: {},
    ParentModelProperty: {}
}

function PathControl(ctrl) {
    Control.prototype.constructor.call(this, ctrl);
    this.$type = "PathControl";

    trace("PathControl constructed: " + TypeSystem.IsOfType(this, "DependencyObject"));
}

PathControl.prototype.set_ParentModel = function (value) {
    this.SetValue("ParentModel", value);
    this.recalculate();
}

PathControl.prototype.get_ParentModel = function () {
    return this.GetValue("ParentModel");
}

PathControl.prototype.set_Path = function (value) {
    this.SetValue("Path", value);
    this.recalculate();
}

PathControl.prototype.createPathItem = function(path, text) {
    var e = document.createElement("span");
    var thispath = path;
    e.setAttribute("class", "pathelement");
    e.setAttribute("title", path);
    e.appendChild(document.createTextNode(text));

    var t$ = this;

    e.onclick = function () {
        var model = t$.get_ParentModel();
        if (model !== undefined) {
            model.NavigateListing(thispath);
        }
    };

    return e;
}

PathControl.prototype.recalculate = function()
{
    var value = this.GetValue("Path");
    var ctrl = this.ctrl;

    // TODO: not a generic solution, possibly need events.
    var model = this.GetValue("ParentModel");

    if (model == null || value == null) return;

    // split path into pieces, skip first entry if empty
    var pes = value.split("/");
    if (pes[0] == null || pes[0] == "") pes.splice(0, 1);

    // clear existing content
    ctrl.innerHTML = "";

    var cont = document.createElement("div");
    cont.setAttribute("class", "pathcontent");
    ctrl.appendChild(cont);

    cont.appendChild(this.createPathItem("", "\u2022"));

    var n = pes.length;
    var name = pes[n - 1];
    var selfpath = pes.join("/");

    if (n > 1)
    {
        pes.splice(n - 1, 1);
        var contpath = pes.join("/");
        cont.appendChild(this.createPathItem(contpath, "\u2022\u2022"));
    }

    if (n >= 1)
    {
        cont.appendChild(this.createPathItem(selfpath, name));
    }
}