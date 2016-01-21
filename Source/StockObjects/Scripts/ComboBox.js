// ------------------------------------------------------------------------------------------------
// ComboBox class
//
// Renders a SELECT control.

ComboBox.prototype = new ItemsControl();
ComboBox.prototype.constructor = ComboBox;

function ComboBox(ctrl) {
    ItemsControl.prototype.constructor.call(this, ctrl);
    this.$type = "ComboBox";
    this.rendertag = "OPTION";
}

ComboBox.prototype.$static = {
    ItemsSourceProperty: {}, // TODO: this belongs to the baseclass!
    SelectedItemProperty: { BindsTwoWayByDefault: true }
}

ComboBox.prototype.set_SelectedItem = function (value) {
    this.SetValue("SelectedItem", value);
}

ComboBox.prototype.get_SelectedItem = function () {
    return this.GetValue("SelectedItem");
}

ComboBox.prototype.initialize = function () {
    ComboBox_selectionchanged(this.ctrl);
}

function ComboBox_selectionchanged(ctrl) {
    var item = null;
    var index = ctrl.selectedIndex;

    if (index >= 0) {
        // OPTION element
        var s = ctrl.children[index];

        // presenter element
        var presenter = s.firstChild;

        // get data context from control wapper
        var pw = ControlFactory.querycontrolwrapper(presenter);
        var item = pw.DataContext;
    }

    // get combo box control wrapper
    var cw = ControlFactory.querycontrolwrapper(ctrl);

    cw.set_SelectedItem(item);

}

