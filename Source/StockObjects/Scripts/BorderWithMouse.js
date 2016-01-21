// ------------------------------------------------------------------------------------------------
// BorderWithMouse class

BorderWithMouse.prototype = new Border();
BorderWithMouse.prototype.constructor = BorderWithMouse;

BorderWithMouse.prototype.$static = {
    BaseClass: "Border",
    CssClassProperty: {}
}

function BorderWithMouse(ctrl) {
    Border.prototype.constructor.call(this, ctrl);
    this.$type = "BorderWithMouse";
    this.ctrl = ctrl;
    this.CssClass = null;
    var __this = this;
    this.IsOver = false;

    /*
    this.ctrl.addEventListener("mouseenter", function () {
        __this.onmouseoverchanged(true);
    });
    this.ctrl.addEventListener("mouseover", function () {
        __this.onmouseoverchanged(true);
    });
    this.ctrl.addEventListener("mouseleave", function () {
        __this.onmouseoverchanged(false);
    });
    this.ctrl.addEventListener("mouseout", function () {
        __this.onmouseoverchanged(false);
    });*/
}



BorderWithMouse.prototype.onmouseoverchanged = function (isover) {
    if (isover == this.IsOver) return;

    trace("bwm: " + isover);

    this.IsOver = isover;
    var css = this.CssClass;
    var ctrl = this.ctrl;

    if (css) {
        if (this.IsOver) {
            ctrl.className = css_append(ctrl.className, css);
        } else {
            ctrl.className = css_remove(ctrl.className, css);
        }
    }
    else {
        trace("$nocss");
    }
}

