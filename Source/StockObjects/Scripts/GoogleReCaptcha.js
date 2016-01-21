
GoogleReCaptcha.prototype = new Control();
GoogleReCaptcha.prototype.constructor = new GoogleReCaptcha;

function GoogleReCaptcha(ctrl) {
    Control.prototype.constructor.call(this, ctrl);

    if (!!ctrl) {
        this.$type = "GoogleReCaptcha";

        // TODO: does not work yet.
        // trace("constructing [GoogleReCaptcha]: " + ctrl.id);

        var e = document.createElement("div");
        e.setAttribute("class", "g-recaptcha");

        ctrl.appendChild(e);

        // e.innerHTML = "<span>[GoogleReCaptcha2]</span>";

        grecaptcha.render(e, {
            'sitekey': '6LdIgxITAAAAAO_T70vz2cEj1-lMzXdziOpq4Ej7'
        });
    }
}

