
// PXK3ZVWCJN: performs asynchronous data context initialization.
function DataContext_Initialize(cont) {
    var processresponse = function (jstext) {
        var rdata = JSON.parse(xdr.responseText);

        trace("perform data context initialization (PXK3ZVWCJN) ...");

        for (var prop in rdata) {
            var value = rdata[prop];

            var setter = datacontext["set_" + prop];
            if (setter !== undefined) {
                trace("dcinit: set " + prop + " := '" + value + "'.");
                setter.call(datacontext, value);
            } else {
                trace("property " + prop + " not set on datacontext.");
            }
        }

        cont();

        if (datacontext["Start"] !== undefined) {
            datacontext.Start();
        }
    };


    var xdr = new XMLHttpRequest();
    xdr.open("GET", "$datacontext");
    xdr.withCredentials = true;

    xdr.onreadystatechange = function () {
        if (xdr.readyState == 4) {
            if (xdr.status == 200) {
                // trace("data context initializer received from '" + window.location.href + "': " + xdr.responseText);
                processresponse();
            } else {
                __this.InitializeError("connection error");
            }
        }
    }

    xdr.send();
}
