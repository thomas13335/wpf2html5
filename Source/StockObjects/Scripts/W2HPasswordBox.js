// ------------------------------------------------------------------------------------------------
// HtmlPasswordBox class
W2HPasswordBox.prototype = new Control();
W2HPasswordBox.prototype.constructor = new W2HPasswordBox;

function W2HPasswordBox(ctrl) {
    Control.prototype.constructor.call(this, ctrl);
    this.$type = "W2HPasswordBox";
    this.strength = 0;

    if (!!ctrl) {

        this.input = document.createElement("input");
        this.input.setAttribute("type", "password");
        this.input.setAttribute("class", "w2h5-password");

        while (ctrl.firstChild) {
            ctrl.removeChild(ctrl.firstChild);
        }
        
        ctrl.appendChild(this.input);

        var _this = this;
        this.input.onkeyup = function () { _this.ValidatePasswordStrength(); };
    }
}

W2HPasswordBox.prototype.set_IsStrongEnough = function (value) {
}

W2HPasswordBox.prototype.set_Consumer = function (consumer) {
    this.SetValue("Consumer", consumer);
    if (!!consumer) {
        consumer.RegisterSource(this);
        consumer.PasswordStrenghtChanged(this.strength);
    }
}

W2HPasswordBox.prototype.SetPassword = function (p) {
    this.input.value = p;
    this.ValidatePasswordStrength();
}

W2HPasswordBox.prototype.GetPassword = function () {
    return this.input.value;
}

W2HPasswordBox.prototype.ValidatePasswordStrength = function () {
    var value = this.input.value;
    var s = value.length >= 6 ? 1 : 0;

    var consumer = this.GetValue("Consumer");

    if (s != this.strength && !!consumer)
    {
        this.strength = s;
        consumer.PasswordStrenghtChanged(s);
    }
}
