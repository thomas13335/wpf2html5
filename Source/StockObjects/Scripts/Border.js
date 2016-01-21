// ------------------------------------------------------------------------------------------------
// Border class

Border.prototype = new Control();
Border.prototype.constructor = Border;

Border.prototype.$static = {
    BaseClass: "Control"
}

function Border(ctrl) {
    Control.prototype.constructor.call(this, ctrl);
    this.$type = "Border";
}
