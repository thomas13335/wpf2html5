UserControl.prototype = new Control();
UserControl.prototype.constructor = UserControl;

function UserControl(ctrl) {
    Control.prototype.constructor.call(this, ctrl);
    this.$type = "UserControl";
}
