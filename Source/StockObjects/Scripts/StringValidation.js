function StringValidation() {
    this.email = /[a-zA-Z0-9\._%+-]+@[a-zA-Z0-9.-]+\.[A-Za-z]{2,4}/i;
}

StringValidation.prototype.ValidateTitle = function (title) {
    var result = false;
    if (title) result = title.length > 2;

    // trace("validate title '" + title + "' => " + result);
    return result;
}

StringValidation.prototype.ValidateEmailName = function (s) {
    var result = this.email.test(s);
    // trace("validate '" + s + "' => " + result);
    return result;
}