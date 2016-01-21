WebViewModelBase.prototype = new DependencyObject();
WebViewModelBase.prototype.constructor = WebViewModelBase;

function WebViewModelBase() 
{
    this.$type = "WebViewModelBase";
    this.seq = 0;
    this.location = null;
    this.notifylocation = null;

    // set this via HIKYCKUIU3
    this.__showio = false;
    this.__showmessage = false;
    this.urlseq = 0;
    this.notifystate = 0;
}

WebViewModelBase.prototype.set_Location = function (url) {
    // trace("webviewmodel: location: " + url);
    this.location = url;
}

WebViewModelBase.prototype.get_Location = function () {
    return this.location;
}

WebViewModelBase.prototype.InitializeComplete = function () { }

WebViewModelBase.prototype.InitializeError = function () { }

WebViewModelBase.prototype.Initialize = function () {
    var __this = this;
    var xdr = new XMLHttpRequest();

    var url = this.location + "/$initialize";

    // trace("[WebViewModelBase] initialization request " + url);

    xdr.open("GET", url, true);
    xdr.withCredentials = true;

    xdr.onreadystatechange = function () {
        if (xdr.readyState == 4) {
            if (xdr.status == 200) {
                __this.InitializeComplete();
            } else {
                __this.InitializeError("connection error");
            }
        }
    }

    xdr.send();

}

WebViewModelBase.prototype.TransactError = function (e) {
}

function wvmbjsonreplacer(key, value) {
    if (value == null) return undefined;
    return value;
}

// HIKYCKUIU3: trace flags
WebViewModelBase.prototype.get_ShowIO = function () {
    return this.__showio;
}

WebViewModelBase.prototype.set_ShowIO = function (value) {
    this.__showio = !!value;
}

WebViewModelBase.prototype.get_ShowMessage = function () {
    return this.__showmessage;
}

WebViewModelBase.prototype.set_ShowMessage = function (value) {
    this.__showmessage = !!value;
}

WebViewModelBase.prototype.SendMessage = function (m) {

    // convert request into JSON string ...
    var request = JSON.stringify(m, wvmbjsonreplacer, 4);

    // trace("SendMessage: " + request);

    if (this.get_ShowMessage()) {
        trace("[WebViewModelBase] send request " + this.location + ": " + request);
    }
    else if (this.get_ShowIO()) {
        trace("[WebViewModelBase] send request " + this.location + " ...");
    }

    var __this = this;
    var xdr = new XMLHttpRequest();
    xdr.open("POST", this.location, true);
    xdr.withCredentials = true;

    xdr.onreadystatechange = function () {
        if (xdr.readyState == 4) {
            if (__this.get_ShowIO()) {
                trace("[WebViewModelBase] response status " + xdr.status);
            }

            if (xdr.status == 200) {
                if (__this.get_ShowMessage()) {
                    trace("[WebViewModelBase] response data: " + xdr.responseText);
                }
                __this.ReceiveMessage(JSON.parse(xdr.responseText));
            } else {
                __this.TransactError("connection error");
            }
        }
    }

    xdr.send(request);
}

WebViewModelBase.prototype.SetNotifyState = function (newstate) {
    if (this.notifystate != newstate) {
        this.notifystate = newstate;
    }
}

WebViewModelBase.prototype.ChangeNotifyStart = function ()
{
    if (this.notifystate != 0)
    {
        trace("ignored [ChangeNotifyStart], already running.");
        return;
    }

    var url = this.notifylocation;
    if (!url) return;

    url += "?s" + (++this.urlseq);

    var __this = this;
    var xdr = new XMLHttpRequest();
    xdr.open("GET", url, true);
    xdr.withCredentials = true;

    xdr.onreadystatechange = function () {
        if (xdr.readyState == 4) {
            if (xdr.status == 200) {
                // trace("received notification ... " + xdr.responseText);

                __this.SetNotifyState(0);
                __this.ReceiveMessage(JSON.parse(xdr.responseText));

                window.setTimeout(function () { __this.ChangeNotifyStart() }, 200);
            }
            else {
                trace("received notification NOK");

                __this.SetNotifyState(0);

                window.setTimeout(function () { __this.ChangeNotifyStart() }, 5000);
            }
        }
    }

    // trace("sending notification request ...");
    this.SetNotifyState(1);
    xdr.send();
}

// MVJY5JKUV6: installs the notification reader.
WebViewModelBase.prototype.InstallNotificationReader = function (subpath) {
    this.notifylocation = this.get_Location() + "/" + subpath;

    // trace("*** install notify: " + this.notifylocation);

    this.ChangeNotifyStart();
}

WebViewModelBase.prototype.ReceiveMessage = function (m)
{ }

WebViewModelBase.prototype.PopulateViewModel = function (data, viewmodel) {
    for (var prop in data) {
        if (prop.indexOf("$") == 0) continue;

        var value = data[prop];
        var setter = viewmodel["set_" + prop];
        if (setter !== undefined) {
            setter.call(viewmodel, value);
        }
        else {
            // trace("property " + prop + " not supported by " + viewmodel["$type"]);
        }
    }
}

WebViewModelBase.prototype.ConvertList = function (list) {
    return Array.prototype.slice.call(list);
}

WebViewModelBase.prototype.CopyList = function (list) {
    var result = new List();

    var e = list.GetEnumerator();
    while (e.MoveNext())
    {
        result.Add(e.get_Current());
    }

    return result;
}

WebViewModelBase.prototype.CreateEnumerator = function(barray) {
    return new ArrayEnumerator(barray);
}

WebViewModelBase.prototype.ValidateFileName = function (name) {
    return name && name.length > 0;
}

WebViewModelBase.prototype.ClearBindings = function (obj) {
}

WebViewModelBase.prototype.BindingStats = function () {
    // trace("binding has " + bind.count + " entries");
}

WebViewModelBase.prototype.Reload = function () {
    location.reload(true);
}

WebViewModelBase.prototype.PrintObject = function (header, obj) {
    trace(header + ": " + JSON.stringify(obj));
}

WebViewModelBase.prototype.ConfirmMessageBox = function (msg) {
    return window.confirm(msg);
}

WebViewModelBase.prototype.EscapeString = function (s) {
    return escape(s);
}

WebViewModelBase.prototype.ActivateUserInterface = function () {
    trace("activating user interface ...");

    // AYONL7EOWG: make visible
    idbody.classList.add("load");
}

WebViewModelBase.prototype.RedirectToURL = function (url) {
    window.location.assign(url);
}

WebViewModelBase.prototype.trace_object = function (msg, obj) {
    trace(msg + ": " + JSON.stringify(obj, null, 1));
}

WebViewModelBase.prototype.IsNullOrEmpty = function (e) {
    switch (e) {
        case "":
        case 0:
        case "0":
        case null:
        case false:
        case typeof this == "undefined":
            return true;
        default:
            return false;
    }
}

// TODO: deuglify this world-wide object
var _wwactivebutton = null;


WebViewModelBase.prototype.SetFocusToOverlay = function () {
    // trace("tobefocused: " + overlayscontainer.outerHTML);

    var tobefocused = null;

    _wwactivebutton = null;

    // look for a suitable object to focus
    Control_recurselements(overlayscontainer, function (e) {
        if (!!e && !!e.tagName) {
            var tag = e.tagName.toUpperCase();
            if ((tag == "INPUT" || tag == "TEXTAREA") && !tobefocused) {
                tobefocused = e;
            }
            else if (tag == "BUTTON") {
                // CKEKBFTP4K: determine the default button to receive ENTER key.
                if (e.getAttribute("data-isdefault") == "true") {
                    trace("wwactivebutton: " + e.id);
                    _wwactivebutton = e;
                }
            }
        }
    });

    if (tobefocused) tobefocused.focus();
}

WebViewModelBase.prototype.PushNavigationHistory = function (viewstate, id) {
    // trace("add history " + viewstate + ":" + id);
    history.pushState({ s: viewstate, id: id }, "History Item");
}
