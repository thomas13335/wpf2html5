// ------------------------------------------------------------------------------------------------

function HtmlTextView(ctrl) {
    this.ctrl = ctrl;
}

// ------------------------------------------------------------------------------------------------

HtmlTextView.prototype.set_Source = function (value) {
    // trace("setting html text " + value);
    this.ctrl.innerHTML = value;
}

