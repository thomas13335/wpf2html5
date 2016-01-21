// ------------------------------------------------------------------------------------------------
// Image class
Image.prototype = new Control();
Image.prototype.constructor = Image;

Image.prototype.$static = {
    SourceProperty: {}
}

function Image(ctrl) {
    Control.prototype.constructor.call(this, ctrl);
    this.$type = "Image";
}

Image.prototype.OnPropertyChanged = function (e) {
    Control.prototype.OnPropertyChanged.call(this, e);

    if (e.Property == "Source") {
        try {
            this.ctrl.src = this.get_Source();
        }
        catch (e) {
            trace("failed to set image source " + e + ", " + value);
        }
    }
}

Image.prototype.get_Source = function () {
    return this.GetValue("Source");
}

Image.prototype.set_Source = function (value) {
    this.SetValue("Source", value);
}
