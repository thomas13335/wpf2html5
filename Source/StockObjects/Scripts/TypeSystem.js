/*
    Type related operations.
*/
TypeSystem = {
    /*
        Checks if an object is the same or a subtype of t.
    */
    IsOfType: function (obj, t) {
        var result = false;
        var type = obj["$type"];
        if (type !== undefined) {
            result = this.IsSubClass(type, t);
        }

        // trace("IsOfType(" + type + ", " + t + ") => " + result);

        return result;
    },
    /*
        Returns the (optional) type descriptor for an object, derived via $type property.
    */ 
    GetTypeDesc: function(x) {
        var desc = window[x];
        if (desc === undefined) {
            trace("WARNING: attempt to [IsSubClass] undefined type '" + x + "'.");
            return;
        }

        var proto = desc["prototype"];
        if (proto === undefined) {
            trace("WARNING: type '" + x + "' has not prototype.");
            return;
        }

        var s = proto["$static"];
        if (s === undefined) {
            trace("WARNING: type descriptor '" + x + "' not found.");
            return;
        }

        return s;
    },

    /*
        Check if a type is the same or a subtype of another type.
    */ 
    IsSubClass: function (x, t) {
        if (x == t) {
            return true;
        }
        else {
            // get typedescriptor for 'x' via 'x.prototype.$static'.
            var s = TypeSystem.GetTypeDesc(x);

            if (s === undefined) {
                return false;
            }
            if (s.BaseClass === undefined) {
                return false;
            }
            else {
                // check baseclass
                var bc = s.BaseClass;
                return TypeSystem.IsSubClass(bc, t);
            }
        }
    }
}