JsonWebRequest.prototype = new DependencyObject();
JsonWebRequest.prototype.constructor = JsonWebRequest;

function JsonWebRequest() {
    this.$type = "JsonWebRequest";
    this.location = null;
    this.errorhandler = null;
}

JsonWebRequest.prototype.set_Location = function (url) {
    this.location = url;
}

JsonWebRequest.prototype.get_Location = function () {
    return this.location;
}

JsonWebRequest.prototype.set_Parse = function () {
    // ignored
}

JsonWebRequest.prototype.get_ShowIO = function () {
    return true;
}

JsonWebRequest.prototype.get_ShowMessage = function () {
    return true;
}

JsonWebRequest.prototype.set_TransactError = function (handler) {
    this.errorhandler = handler;
}

JsonWebRequest.prototype.get_TransactError = function () {
    return this.errorhandler;
}

JsonWebRequest.prototype.jsonreplacer = function (key, value) {
    if (value == null) return undefined;
    return value;
}


JsonWebRequest.prototype.Send = function (jreq, callback) {

    var __this = this;
    var url = this.location;
    var xdr = new XMLHttpRequest();
    xdr.open("POST", url, true);
    xdr.withCredentials = true;

    xdr.onerror = function () {
        trace("xdr error: " + xdr.status + ", " + xdr.description + ", " + xdr.responseText);
    };

    xdr.onreadystatechange = function () {
        __this.Complete(xdr, callback);
    }

    var textreq = JSON.stringify(jreq, this.jsonreplacer, 4);

    if (__this.get_ShowIO()) {
        trace("<== POST " + url);
        if (this.get_ShowMessage()) {
            trace(textreq);
        }
    }

    try {
        xdr.send(textreq);
    }
    catch (e) {
        this.TriggerError(e);
    }
}

JsonWebRequest.prototype.Get = function (urlex, callback) {

    var __this = this;
    var url = new PathOperations().ComposeURL(this.location, urlex);
    var xdr = new XMLHttpRequest();

    try{
        xdr.withCredentials = true;
        xdr.open("GET", url, true);
    }
    catch(e)
    {
        trace("[JsonWebRequest] exception during open(): " + e);
        this.TriggerError(e);
        return;
    }

    xdr.onerror = function () {
        trace("xdr error: " + xdr.status + ", " + xdr.description + ", " + xdr.responseText);
    };

    xdr.onreadystatechange = function () {
        __this.Complete(xdr, callback);
    }

    if (this.get_ShowIO()) {
        trace("<== GET " + url);
    }

    // PBM: mixed content exception cannot be catched in chrome.
    try {
        xdr.send();
    }
    catch (e) {
        trace("[JsonWebRequest] exception: " + e);
        this.TriggerError(e);
    }
}

JsonWebRequest.prototype.TriggerError = function (e) {
    var msg = "transaction error: " + e;
    if (this.errorhandler) {
        this.errorhandler(e);
    }
}

// completion handler for XMLHttpRequest
JsonWebRequest.prototype.Complete = function (xdr, callback) {
    try {
        if (xdr.readyState == 4) {
            if (this.get_ShowIO()) {
                trace("==> " + xdr.status);
            }

            if (xdr.status == 200) {
                // success
                if (this.get_ShowMessage()) {
                    trace(xdr.responseText);
                }

                var z = JSON.parse(xdr.responseText);
                callback(z);
            } else {
                // error
                this.TriggerError("completion error " + xdr.status);
            }
        }
    }
    catch (e) {
        this.TriggerError(e);
    }
}

