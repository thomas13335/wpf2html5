
function JsonObjectFactory() {
}

JsonObjectFactory.prototype.Convert = function (o) {
    if (!Array.isArray(o)) {
        throw "JsonObjectFactory requires array.";
    }

    trace("TODO: convert JSON!");

    return new List();
}