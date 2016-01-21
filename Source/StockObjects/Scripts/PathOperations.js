function PathOperations() {
}

PathOperations.prototype.GetContainerPath = function (path) {
    var pe = path.split("/");

    var result = "";
    for (var j = 0; j < pe.length - 1; ++j) {
        var e = pe[j];
        if (e.length > 0) result += "/" + e;
    }

    return result;
}

PathOperations.prototype.GetFileName = function (path) {
    if (!path) return "";
    var pe = path.split("/");

    if (pe.length > 1)
    {
        return pe[pe.length - 1];
    }
    else {
        return "";
    }
}

PathOperations.prototype.IsPrefixOf = function (tobetested, ofwhat) {
    var pet = tobetested ? tobetested.split("/") : [];
    var pew = ofwhat && ofwhat != "/" ? ofwhat.split("/") : [];

    pet = pet.filter(function (e) { return e != ""; });
    pew = pew.filter(function (e) { return e != ""; });

    var n = pew.length;
    if (pet.length < n) return false;

    for (var j = 0; j < n; ++j)
    {
        if (pew[j] != pet[j]) return false;
    }

    return true;
}

PathOperations.prototype.ComposeURL = function (url, subpath) {
    var k = url.length;

    // make sure url is terminated with a slash
    if (k > 0) {
        if (url[k - 1] != "/") {
            url = url + "/";
        }
    }

    // append unprefix subpath
    var pe = subpath.split("/");
    return url + pe.join("/");
}
