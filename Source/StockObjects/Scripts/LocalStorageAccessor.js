
function LocalStorageAccessor(ctx) {
    this.prefix = ctx + ".";
}

LocalStorageAccessor.prototype.GetString = function (key) {
    var lkey = this.prefix + key;
    return localStorage.getItem(lkey);
}

LocalStorageAccessor.prototype.SetString = function (key, value) {
    var lkey = this.prefix + key;
    localStorage.setItem(lkey, value);
}

LocalStorageAccessor.prototype.GetObject = function (key) {
    var lkey = this.prefix + key;
    var text = localStorage.getItem(lkey);
    if (text) {
        return JSON.parse(text);
    }
    else {
        return null;
    }
}

LocalStorageAccessor.prototype.SetObject = function (key, value) {
    var lkey = this.prefix + key;
    if (value) {
        localStorage.setItem(lkey, JSON.stringify(value));
    }
    else {
        localStorage.removeItem(lkey);
    }
}