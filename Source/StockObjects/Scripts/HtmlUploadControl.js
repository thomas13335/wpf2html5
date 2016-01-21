// SO2EEQTZ32: HtmlUploadControl jscript control

HtmlUploadControl.prototype = new DependencyObject();
HtmlUploadControl.prototype.constructor = HtmlUploadControl;

function HtmlUploadControl(ctrl) {
    this.$type = "HtmlUploadControl";
    this.ctrl = ctrl;
    this.FileObject = null;

    this.__verbose = true;

    //trace("initializing the HTML upload control ...");

    var ctrl = this.ctrl;

    var __this = this;
    var input = document.createElement("input");
    input.setAttribute("type", "file");
    input.setAttribute("name", "fileinput");
    input.setAttribute("accept", "*/*");
    input.setAttribute("class", "htmlupload");
    ctrl.appendChild(input);
    this.fileinput = ctrl.children[0];

    var f = this.fileinput;
    if (f) {
        // trace("attach event handler ...");
        f.onchange = function (e) {
            // trace("file input changed ..." + e.target.files.length + ", " + e.target);
            var fowrap = new HtmlFileObject(e.target.files);
            __this.set_FileObject(fowrap);
        }
    }
}

HtmlUploadControl.prototype.set_FileObject = function (value) {
    this.SetValue("FileObject", value);

    var ctrl = this.ctrl;
    var b = bind.getbinding(ctrl.id, "FileObject");
    if (b !== undefined) {
        if (b.mode == "duplex") {
            b.updatesource(value);
        }
    }

}

HtmlUploadControl.prototype.get_FileObject = function () {
    return this.GetValue("FileObject");
}

HtmlFileObject.prototype.constructor = HtmlFileObject;

function HtmlFileObject(fo) {
    this.files = fo;
}

HtmlFileObject.prototype.get_FileName = function () {
    return this.files[0].name;
}