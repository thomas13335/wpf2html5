
function StringOperations()
{ }

StringOperations.prototype.IsNullOrEmpty = function (s) {
    if (typeof s === 'string') {
        return s.length > 0;
    }
    else {
        return false;
    }
}

