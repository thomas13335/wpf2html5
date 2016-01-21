// ------------------------------------------------------------------------------------------------
// Hyperlink class
Hyperlink.prototype = new Control();
Hyperlink.prototype.constructor = Hyperlink;

Hyperlink.prototype.$static = {
    CommandProperty: {},
    CommandParameterProperty: {}
}

function Hyperlink(ctrl) {
    Control.prototype.constructor.call(this, ctrl);
    this.$type = "Hyperlink";
}

Hyperlink.prototype.set_NavigateUri = function (value) {
    // TODO: wrong impl
    this.ctrl.href = value;
}

Hyperlink.prototype.set_Command = function (arg) {
    this.SetValue("Command", arg);
}


Hyperlink.prototype.set_CommandParameter = function (arg) {
    this.SetValue("CommandParameter", arg);
}

function Hyperlink_Click(ctrl) {
    Control_executecommand(ctrl);
}
