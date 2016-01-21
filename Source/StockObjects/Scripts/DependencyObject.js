
var dpobjseed = 0;
var handlerindex = 0;

function DependencyObject(type) {
    this.$type = type;
    this.__bindings = [];
    this.__verbose = false;
    this.id = ++dpobjseed;
    this.__propchanged = null;
}

DependencyObject.prototype.$static = {}

DependencyObject.prototype.trace = function (s) {
    if (this.__verbose) {
        trace("object " + this.id + " [" + this.$type + "]: " + s);
    }
}

DependencyObject.prototype.InitValue = function (prop, value) {
    if (!value) value = null;
    this[prop] = value;
};

DependencyObject.prototype.GetValue = function (prop) {
    return this[prop];
};

DependencyObject.prototype.SetValue = function (prop, value) {
    if (!prop) {
        trace("warning: property name required for dependencyobject.");
        return;
    }

    var changehandler = this[prop + "Changed"];
    var previousvalue = this[prop];

    // type conversion
    var propinfo = this.__getpropdesc(prop);
    if (!!propinfo && !!propinfo.Convert) {
        var cvalue = propinfo.Convert(value);

        if (this.__verbose) {
            this.trace("convert: " + value + " -> " + cvalue);
        }

        if (cvalue === undefined || cvalue == null) {
            trace("conversion failed.");

            // TODO: validation error, see https://msdn.microsoft.com/en-us/library/ms753962(v=vs.110).aspx
            return;
        }

        value = cvalue;
    }

    // capture transition event
    var eargs = { Property: prop, OldValue: previousvalue, NewValue: value };

    // TODO: coerce

    // assign value
    this[prop] = value;

    // call other change handlers
    this.OnPropertyChanged(eargs);

    // fire INotifyPropertyChanged
    Delegate.Fire(this.__propchanged, this, { PropertyName: prop });

    if (changehandler !== undefined) {
        this.trace("trigger change handler ...");
        changehandler(this, eargs);
    }
};

DependencyObject.prototype.add_PropertyChanged = function (handler) {
    if (!handler) {
        throw "invalid handler specified.";
    }

    this.__propchanged = Delegate.Combine(this.__propchanged, handler);

    // this.trace("change notification handler added: " + handler.id + ": " + JSON.stringify(this.__propchanged));

}

DependencyObject.prototype.remove_PropertyChanged = function (handler) {
    this.__propchanged = Delegate.Remove(this.__propchanged, handler);
}

/*
    Virtual method called when a property has changed.
    Subclass may override but must call the baseclass.
*/
DependencyObject.prototype.OnPropertyChanged = function (e) {
    if (this.__verbose) {
        this.trace("[OnPropertyChanged(" + e.Property + ")]: " + e.OldValue + " -> " + e.NewValue);
    }
}

/*
    Returns the property metadata for a dependency property.
*/
DependencyObject.prototype.__getpropdesc = function (prop, caller) {
    var result = null;
    var type = this.$type;

    while (type !== undefined)
    {
        var desc = TypeSystem.GetTypeDesc(type);
        if (desc === undefined) {
            trace("WARNING: type [" + type + "] has not $static section.");
            break;
        }

        result = desc[prop + "Property"];
        if (result !== undefined) {
            break;
        }
        
        type = desc.BaseClass;
    }

    if (null == result) {
        trace("WARNING: property '" + prop + "' not found on [" + this.$type + "].");
    }

    return result;
}



DependencyObject.prototype.triggerpropertychanged = function (prop) {
}

// expects a BindingObject
DependencyObject.prototype.registerbinding = function (bindobj) {
    var prop = bindobj.get_Property();
    var existing = this.__bindings[prop];
    if (!!existing) {
        existing.Clear();
    }

    this.__bindings[prop] = bindobj;
}

DependencyObject.prototype.clearallbindings = function () {
    var list = this.__bindings;
    for (p in list)
    {
        var bindobj = list[p];
        bindobj.Clear();
    }

    this.__bindings = [];
}

/*
    JS Property Access Helpers
*/

function GetPropertyValue(source, prop) {
    var value = null;
    var getter = source["get_" + prop];
    if (getter !== undefined) {
        // via getter
        value = getter.call(source);
    }
    else {
        // via property
        value = source[prop];

        if (value === undefined) {
            trace("WARNING: property " + prop + " is not defined on " + source.$type);
        }
    }

    if (value === undefined) {
        value = null;
    }

    return value;
}

function SetPropertyValue(target, prop, value) {
    var setter = null;
    if (!!target) {
        var setter = target["set_" + prop];
        if (!!setter) {
            setter.call(target, value);
        }
        else {
            trace("WARNING: target [" + target.$type + "] does not provide setter for " + prop + ".");
        }
    }
}

function GetDepdencencyProperty(obj, prop) {
    var result = null;
    if (obj["__getpropdesc"] !== undefined) {
        result = obj.__getpropdesc(prop);
    }

    if (!result) {
        trace("WARNING: unable to get property for '" + obj.$type + "." + prop + "'.");
    }

    return result;
}
