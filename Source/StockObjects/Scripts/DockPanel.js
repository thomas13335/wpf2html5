// ------------------------------------------------------------------------------------------------
// DockPanel class
DockPanel.prototype = new Control();
DockPanel.prototype.constructor = DockPanel;

function DockPanel(ctrl) {
    this.$type = "DockPanel";
    this.ctrl = ctrl;
}

function DockPanel_ResizeVertical(ctrl) {
    // TODO: implement ?
}
