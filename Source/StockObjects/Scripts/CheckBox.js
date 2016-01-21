// ------------------------------------------------------------------------------------------------
// CheckBox class
CheckBox.prototype = new Control();
CheckBox.prototype.constructor = CheckBox;

CheckBox.prototype.$static = {
    BaseClass: "Control",
    IsCheckedProperty: {}
}

function CheckBox(ctrl) {
    this.$type = "CheckBox";
    this.ctrl = ctrl;
}

CheckBox.prototype.set_IsChecked = function (value) {
    this.ctrl.checked = !!value;
}

CheckBox.prototype.get_IsChecked = function () {
    return this.ctrl.checked;
}

function CheckBox_Changed(ctrl) {
    trace("[CheckBox_Changed] " + ctrl.id);
    var b = bind.getbinding(ctrl.id, "IsChecked");
    if (b !== undefined) {
        if (b.mode == "duplex") {
            b.updatesource(ctrl.checked);
        }
        else {
            trace("selecteditem no mode " + b.mode);
        }
    }
}
