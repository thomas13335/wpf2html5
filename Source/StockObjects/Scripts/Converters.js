
function BooleanToVisibilityConverter(invisibility, invert) {
    this.invert = invert;
    this.invisibility = invisibility;
}

BooleanToVisibilityConverter.prototype.Convert = function (value) {
    value = !!value;
    if (this.invert) value = !value;

    return value ? "visible" : this.invisibility;
}

function BooleanInverterConverter() {
}

BooleanInverterConverter.prototype.Convert = function (value) {
    return !value;
}

function IconSourceConverter() {
}

IconSourceConverter.prototype.Convert = function (value) {
    return value;
}

function DateTimeToPrintableConverter() {
}

DateTimeToPrintableConverter.prototype.Convert = function (value) {
    // assume date comes in DDARXGBAW3 format.
    //var d = new Date(value);

    var syear = value.substring(0, 4);
    var smonth = value.substring(5, 7);
    var sday = value.substring(8, 10);
    var shour = value.substring(11, 13);
    var sminute = value.substring(14, 16);

    var date = sday + "." + smonth + "." + syear + " " + shour + ":" + sminute;

    return date;
}
