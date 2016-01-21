// ------------------------------------------------------------------------------------------------
// StackPanel class
StackPanel.prototype = new Control();
StackPanel.prototype.constructor = StackPanel;

function StackPanel(ctrl) {
    this.$type = "StackPanel";
    this.ctrl = ctrl;
}
