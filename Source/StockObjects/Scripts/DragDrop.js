
// ApplicationModel specific code

// FileUploader class
function FileUploader(fd, name, size) {

    // POXDAH2POB: check if drop is allowed
    if(!datacontext.IsDropAllowed())
    {
        return null;
    }


    var model = new UploadViewModel();
    model.set_Parent(datacontext);
    model.set_FileName(name);
    model.set_FileSize(size);
    model.set_Status("initial");

    // add model to uploads collection
    datacontext.AddUpload(model);

    var url = new PathOperations().ComposeURL(datacontext.get_DataLocation(), datacontext.get_CurrentPath());

    url += "?newname=" + encodeURIComponent(name);

    trace("uploading to " + url);

    var xdr = this.xdr = new XMLHttpRequest();
    xdr.open("POST", url, true);
    xdr.withCredentials = true;
    xdr.setRequestHeader("X-RedirectAfterUpload", "1");

    // NJYJFRWXKK: match with server

    if (xdr.upload !== undefined) {
        xdr.addEventListener("progress", function (e) {
            var percent = 0;
            if (e.loaded) {
                var position = e.loaded;
                var total = e.total;
                if (e.lengthComputable) {
                    var percent = Math.ceil(position / total * 100);
                    model.set_Progress(percent);
                }

                model.set_Status("running");

                trace("progress " + percent);
            }
            else {
                trace("progres unavailable.");
            }
        });
    }

    xdr.onreadystatechange = function () {
        // trace("ready: " + xdr.readyState);

        if (xdr.readyState == 4) {
            trace("status: " + xdr.status);
            if (xdr.status != 200) {
                model.set_Status("failed");
            }
            else {
                model.set_Status("completed");
            }
        }
    }

    trace("*** starting upload ...");
    xdr.send(fd);
}


// FileUploadInitiator class
function FileUploadInitiator() {
    this.$type = "FileUploadInitiator";
}

FileUploadInitiator.prototype.InitiateUpload = function (fileswrap, newname) {
    // SO2EEQTZ32: unwrap
    var files = fileswrap.files;

    for (var i = 0; i < files.length; i++) {
        var fd = new FormData();
        fd.append("file", files[i]);

        var name = files[i].name;

        // SO2EEQTZ32: supply alternative name
        if (!!newname && i == 0) {
            name = newname;
            files[i].name = newname;
        }

        trace("uploading as: " + name);

        var up = new FileUploader(fd, name, files[i].size);
    }
}

// DragDrop_Initialize initialize function
function DragDrop_Initialize()
{
    var container = document.getElementById("idbody");

    var handleFileUpload = function (files, obj) {
        for (var i = 0; i < files.length; i++) {
            var fd = new FormData();
            fd.append('file', files[i]);

            var up = new FileUploader(fd, files[i].name, files[i].size);
        }
    }

    window.addEventListener("dragover", function (e) {
        // prevent default here for custom behaviour ...
        e.preventDefault();
        return false;
    });

    window.addEventListener("drop", function (e) {
        e.preventDefault();
        var files = e.dataTransfer.files;
        handleFileUpload(files, container);
        return false;
    });

}

