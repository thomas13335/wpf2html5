/* Control.js */

var controlidseed = 0;

// ------------------------------------------------------------------------------------------------
// Control class (abstract)

Control.prototype = new DependencyObject();
Control.prototype.constructor = Control;

Control.prototype.$static = {
    BaseClass: "DependencyObject",
    VisibilityProperty: {},
    ToolTipProperty: {}
}

function Control(ctrl) {
    DependencyObject.prototype.constructor.call(this, "Control");
    this.ctrl = ctrl; 
    this.__defaultdisplay = ctrl ? ctrl.style.display : "block";
    this.__inputbindings = [];
}

Control.prototype.get_IsEnabled = function () {
    return !this.ctrl.disabled;
}

Control.prototype.set_IsEnabled = function (value) {
    // trace("ctrl " + this.ctrl.id + " disabled " + value);
    this.ctrl.disabled = !value;
}

Control.prototype.set_ToolTip = function (value) {
    this.ctrl.title = value;
}

Control.prototype.set_Visibility = function (value) {
    this.SetValue("Visibility", value);
}

Control.prototype.OnPropertyChanged = function (e) {
    if (e.Property == "Visibility") {
        var c = this.ctrl;
        var value = e.NewValue;
        trace("control " + c.id + " visibility = " + value);
        if (value == "collapsed" || value == "hidden") {
            c.style.display = "none";
        }
        else {
            c.style.display = this.__defaultdisplay;
        }
    }
}

/*
    Control Input Bindings
*/

Control.prototype.add_InputBinding = function (mb) {
    var i = this.__inputbindings;
    i.splice(i.length, 0, mb);
}

Control.prototype.fire_MouseBinding = function (ev) {
    var i = this.__inputbindings;
    for (var j = 0; j < i.length; ++i) {
        var b = i[j];
        if (b.gesture !== undefined && b.gesture == ev) {
            // fire mouse event
            b.fire(this, ev);
        }
    }
}

/*
    Control Static Methods
*/ 

function Control_recurselements(e, cb) {
    cb(e);
    if (e["children"] !== undefined) {
        for (var j = 0; j < e.children.length; ++j) {
            var c = e.children[j];
            Control_recurselements(c, cb);
        }
    }
}

function Control_purge(ctrl) {
    Control_recurselements(ctrl, function (e) {
        var wrapper = ControlFactory.querycontrolwrapper(e);
        if (!!wrapper) {
            BindingOperations.ClearAllBindings(wrapper);
        }
    });
}

function Control_generatecontainer(ctrl, tagname) {
    // trace("generatecontainer " + ctrl.id + " tag " + tagname + "...");
    var e = document.createElement(tagname);

    // TODO: ItemContainerStyle?
    return e;
}

function Control_templaterror(presenter, type) {
    // WTCQATT4LZ: no template for item type: missing a DataTemplate?
    var err = document.createElement("div");
    err.setAttribute("class", "apperror");
    var msg = "[WTCQATT4LZ] no template generator for [" + type + "].";
    trace("ERROR: " + msg);
    err.innerHTML = msg;
    presenter.appendChild(err);
}

function Control_applytemplate(ctrl, presenter, item) {

    // extract type if the model item
    var type = item["$type"];

    if (type === undefined) {
        trace("ERROR: no type identification for applytemplate.");
        return false;
    }

    // get the data template to apply
    var generator = TemplateFactory.gettemplate(ctrl, type);
    if (generator === undefined || generator == null) {
        // no such template
        Control_templaterror(presenter, type);
        return false;
    }
    else {
        // create a wrapper object for the presenter control
        var w = ControlFactory.getcontrolwrapper(presenter);

        // associate the data context with the presenter wrapper
        w.DataContext = item;

        // call the generator to populate the presenter content
        generator(ctrl.id, presenter, item);
        return true;
    }
}

function Control_allocatedynamicid() {
    return "c" + (++controlidseed);
}

// attaches a label to the control (7A27DOZU5F)
function Control_AppendText(E, text) {
    var p = E.parentNode;
    var label = document.createElement("label");
    label.setAttribute("for", E.id);
    label.appendChild(document.createTextNode(text));
    p.appendChild(label);
}

/*
    Executes a (Command/CommandParameter) property on a given object.
*/
function Control_wrapperexecutecommand(target) {
    var command = target.GetValue("Command");
    if (!command) {
        trace("control has no associated command object.");
        return;
    }

    var param = target.GetValue("CommandParameter");

    command.Execute(param);
}

function Control_executecommand(ctrl) {
    var target = ControlFactory.getcontrolwrapper(ctrl);
    if (!target) {
        trace("control has no associated control class, click ignored.");
        return;
    }

    return Control_wrapperexecutecommand(target);
}

/*
    CSS modification methods
*/

function css_append(css, cls) {
    css.split(" ");
    var index = css.indexOf(cls);
    if (index < 0) {
        return css + " " + cls;
    }
    else {
        return css;
    }
}

function css_remove(css, cls) {
    var newClassName = "";
    var i;
    var classes = css.split(" ");
    for (i = 0; i < classes.length; i++) {
        if (classes[i] !== cls) {
            newClassName += classes[i] + " ";
        }
    }
    return newClassName;
}

