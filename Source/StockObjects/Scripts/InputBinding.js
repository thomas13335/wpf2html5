
InputBinding.prototype = new DependencyObject();
InputBinding.prototype.constructor = InputBinding;

InputBinding.prototype.$static = {
    BaseClass: "DependencyObject",
    CommandProperty: {},
    CommandParameterProperty: {}
}

function InputBinding() {
    DependencyObject.prototype.constructor.call(this, null);
    this.$type = "InputBinding";
}

InputBinding.prototype.get_Command = function () {
    return this.GetValue("Command");
}

InputBinding.prototype.set_Command = function (value) {
    this.SetValue("Command", value);
}

InputBinding.prototype.get_CommandParameter = function () {
    return this.GetValue("CommandParameter");
}

InputBinding.prototype.set_CommandParameter = function (value) {
    this.SetValue("CommandParameter", value);
}

InputBinding.prototype.fire = function (sender, ev) {
    trace("fire input binding " + this.$type + "#" + this.id + " event " + ev + " ...");
    Control_wrapperexecutecommand(this);
}

// ------------------------------------------------------------------------------------------------


KeyBinding.prototype = new InputBinding();
KeyBinding.prototype.constructor = KeyBinding;

function KeyBinding(ctrl, key, cb, pb) {
    this.$type = "KeyBinding";

    // binding parameters
    this.ctrl = ctrl;
    this.prop = key;
    this.cb = cb;
    this.pb = pb;

    // bind.registerbinding(this);
    trace("TODO: register KeyBinding.");
}

KeyBinding.prototype.onkeydown = function (event) {
    /*var b = bind.getbinding(ctrl.id, "__mouse_LeftClick");
    if (b === undefined) return;
    if (b.$type != "InputBinding") return;
    b.invokeexecute(event);*/ 
}

// ------------------------------------------------------------------------------------------------

// MouseBinding class
// Maps mouse events to commands on per control basis.

MouseBinding.prototype = new InputBinding();
MouseBinding.prototype.constructor = KeyBinding;

function MouseBinding(ctrl, mouseaction, cb, pb)
{
    InputBinding.prototype.constructor.call(this);
    this.$type = "MouseBinding";
    this.gesture = mouseaction;

    var w = ControlFactory.getcontrolwrapper(ctrl);

    // binding parameters
    this.ctrl = ctrl;
    this.prop = mouseaction;
    this.cb = cb;
    this.pb = pb;

    // create bindings
    if (cb !== undefined) {
        new BindingObject(this, "Command", cb);
    }

    if (pb != undefined) {
        new BindingObject(this, "CommandParameter", pb);
    }

    // trace("MouseBinding#" + this.id + " created: " + this.get_Command().toString());

    // register this as an input binding with the associcated control wrapper.
    w.add_InputBinding(this);
}

// Mouse_Click function
// Global mouse click handler.
function Mouse_Click(ctrl, event) {
    var w = ControlFactory.getcontrolwrapper(ctrl);
    if (w !== undefined) {
        w.fire_MouseBinding("__mouse_LeftClick");
    }
}