function Dictionary()
{ }

Dictionary.prototype.ContainsKey = function(key)
{
    var value = this[key];
    if (value !== undefined && value != null) return true;
    return false;
}

Dictionary.prototype.Lookup = function(key)
{
    var result = this[key];
    if (result === undefined) return null;
    return result;
}