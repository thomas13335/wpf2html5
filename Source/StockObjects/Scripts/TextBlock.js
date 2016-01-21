// ------------------------------------------------------------------------------------------------
// TextBlock class
TextBlock.prototype = new Control();
TextBlock.prototype.constructor = TextBlock;

function TextBlock(ctrl) {
    Control.prototype.constructor.call(this, ctrl);
    this.$type = "TextBlock";
}

TextBlock.prototype.$static = {
    BaseClass: "Control",
    TextProperty: {}
}

function TextBlock_SetText(ctrl, value) {
    while (ctrl.firstChild !== null) {
        ctrl.removeChild(ctrl.firstChild);
    }

    ctrl.appendChild(document.createTextNode(value));
}

TextBlock.prototype.set_Text = function (value) {
    if (value === undefined || value == null)
        value = "";

    TextBlock_SetText(this.ctrl, value);
}
