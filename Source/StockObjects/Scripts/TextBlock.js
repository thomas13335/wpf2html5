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

TextBlock.prototype.set_Text = function (value) {
    this.SetValue("Text", value);
}

TextBlock.prototype.get_Text = function (value) {
    this.GetValue("Text");
}

function TextBlock_SetText(ctrl, value) {

    if (value === undefined || value == null)
        value = "";

    while (ctrl.firstChild !== null) {
        ctrl.removeChild(ctrl.firstChild);
    }

    ctrl.appendChild(document.createTextNode(value));
}

TextBlock.prototype.OnPropertyChanged = function (e) {
    TextBlock_SetText(this.ctrl, this.GetValue("Text"));
}

