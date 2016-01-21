// PleaseWaitControl

PleaseWaitControl.prototype = new Control();
PleaseWaitControl.prototype.constructor = PleaseWaitControl;

PleaseWaitControl.prototype.$static = {
    BaseClass: "Control",
    TextProperty: {}
}

function PleaseWaitControl(ctrl) {
    Control.prototype.constructor.call(this, ctrl);
    this.$type = "PleaseWaitControl";
    this.waitpos = 0;

    var content = document.createElement("div");
    content.setAttribute("class", "waitctrl");

    // image wrapper DIV
    var wrap = document.createElement("div");
    wrap.setAttribute("class", "waitwrap");

    // animation IMG
    var img = document.createElement("img");
    img.setAttribute("class", "waitimg");
    img.src = "wait.png";
    wrap.appendChild(img);
    content.appendChild(wrap);

    // text DIV
    var cap = document.createElement("div");
    cap.setAttribute("class", "waitcap");
    content.appendChild(cap);

    ctrl.appendChild(content);

    this.cap = cap;
    this.img = img;

    var __this = this;
    setTimeout(function () { __this.move(); }, 50);
}

PleaseWaitControl.prototype.move = function () {
    var wp = (this.waitpos + 1) % 36;

    // trace("img: " + this.img + " " + this.waitpos);

    this.img.style.marginTop = (-48 * wp) + "px";

    this.waitpos = wp;
    var __this = this;
    setTimeout(function () { __this.move(); }, 50);
}

PleaseWaitControl.prototype.set_Text = function (value) {
    this.cap.innerHTML = value;
}
