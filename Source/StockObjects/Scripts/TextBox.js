// ------------------------------------------------------------------------------------------------
// TextBox class
TextBox.prototype = new Control();
TextBox.prototype.constructor = TextBox;

TextBox.prototype.$static = {
    TextProperty: { BindsTwoWayByDefault: true },
    IsEnabledProperty: {}
}

function TextBox(ctrl) {
    Control.prototype.constructor.call(this, ctrl);
    this.$type = "TextBox";
    this.InitValue("Text");
}

TextBox.prototype.set_Text = function (value) {
    this.ctrl.value = value;
    this.SetValue("Text", value);
}

function propagateeventtobinding(ctrl, e, prop) {
    /*var b = bind.getbinding(ctrl.id, prop);
    if (b !== undefined) {
        b.invokeexecute(e);
    }*/ 
}

function TextBox_OnKeyDown(ctrl, e) {
    propagateeventtobinding(ctrl, e, "__key_" + e.keyCode);
}

function TextBox_OnKeyUp(ctrl, e) {
    propagateeventtobinding(ctrl, e, "__key_" + e.keyCode);

    var target = ControlFactory.getcontrolwrapper(ctrl);
    if (!target) {
        trace("button has no associated control class, click ignored.");
        return;
    }

    var value = ctrl.value;

    // delay update of the binding ...
    setTimeout(function () {
        target.SetValue("Text", value);
    }, 50);

}
