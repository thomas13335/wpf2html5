// ------------------------------------------------------------------------------------------------
// Button class
Button.prototype = new Control();
Button.prototype.constructor = Button;

Button.prototype.$static = {
    BaseClass: "Control",
    CommandProperty: { Type: "ICommand", BindsTwoWayByDefault: false },
    CommandParameterProperty: {},
    IsEnabledProperty: {}
}

function Button(ctrl) {
    Control.prototype.constructor.call(this, ctrl);
    this.$type = "Button";
}

function GetCommandBinding(ctrl) {
    var b = bind.getbinding(ctrl.id, "Command");
    if (b !== undefined) {
        var cmd = b.GetSourceValue();

        var bparam = bind.getbinding(ctrl.id, "CommandParameter");
        var pvalue = null;
        if (bparam !== undefined) {
            pvalue = bparam.GetSourceValue();
            //trace("cmdpar: " + pvalue + ": " + bparam['$type'] + ", " + bparam.path);
        }

        return { Command: cmd, Parameter: pvalue };
    }

}

function Button_Click(ctrl) {
    Control_executecommand(ctrl);
}

Button.prototype.set_Command = function (value) {
    this.SetValue("Command", value);
}

Button.prototype.get_Command = function () {
    return this.GetValue("Command");
}

Button.prototype.set_CommandParameter = function (value) {
    this.SetValue("CommandParameter", value);
}

Button.prototype.get_CommandParameter = function () {
    this.GetValue("CommandParameter");
}

Button.prototype.set_Content = function (value) {
    this.ctrl.innerHTML = value;
}

