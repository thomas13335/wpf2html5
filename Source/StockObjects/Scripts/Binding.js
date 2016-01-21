/* binding.js */

/*
    Represents a binding between a data source and a data target.
    @param path The binding path.
    @param source Optional source object.
*/ 

function Binding(path, source) {
    this.$type = "Binding";
    this._path = path;
    this._converter = null;
    this._handlers = [];
    this._elements = [];
    this.onchanged = null;
    this.isupdating = false;
    this.mode = "Default";
    this.__verbose = false;

    if (source !== undefined) {
        this.set_Source(source);
    }
}


Binding.prototype.toString = function () {
    var source = this._elements[0].source;
    var id = !!source ? source.id : "?";
    return "(" + id + ", " + this._path + ", " + this.mode + ")";
}

Binding.prototype.set_Source = function (a) {
    this.clearhandlers(0);
    this.initpath();
    this.updatehandler(0, a);
}

Binding.prototype.get_Source = function () {
    return this._source;
}

Binding.prototype.set_Converter = function (converter) {
    this._converter = converter;
}

Binding.prototype.get_Converter = function () {
    return this._converter;
}


Binding.prototype.initpath = function () {
    // configures the elements array
    this._elements = [];
    var path = this._path;

    // top level
    this._elements[0] = { source: null, name: ".", handler: null };
    if (path != null && path.length > 0) {
        var list = path.split(".");
        var n = list.length;

        for (var j = 0; j < n; ++j) {
            this._elements[j + 1] = {
                name: list[j],
                source: null,
                handler: null
            };
        }
    }
}

Binding.prototype.print = function () {
    for (var j = 0; j < this._elements.length; ++j) {
        var e = this._elements[j];
        trace("  [" + e.name + "] " + e.source + " => " + e.handler);
    }
}

Binding.prototype.clearhandlers = function (j, n) {
    if (n === undefined) n = this._elements.length;
    // trace("binding " + this._path + " clear " + j + ", " + n);
    for (; j < n; j++) {
        var e = this._elements[j];
        if (!!e.handler) {
            if (e.source["remove_PropertyChanged"] !== undefined) {
                e.source.remove_PropertyChanged(e.handler);
            }
            else {
                // may not have INotifyPropertyChanged
                // trace("WARNING: object [" + e.source["$type"] + "] has no property change handler.");
            }
                 
            e.handler = null;
        }
        if (!!e.source) {
            e.source = null;
        }
    }
}

/*
    Updates event handlers starting from path index j.
*/ 
Binding.prototype.updatehandler = function (j, source) {

    var e = this._elements[j];

    if (!!e.source && !!source && e.source.id == source.id) {
        // up to date
        return;
    }

    this.clearhandlers(j);

    e.source = source;

    if (null != e.source && j + 1 < this._elements.length) {
        // install change notification handler
        var next = this._elements[j + 1];
        var __this = this;
        e.handler = {
            execute: function (sender, arg) {
                // handler reevaluates
                if (arg.PropertyName == next.name) {
                    __this.evaluate();
                }
            },
        };

        if (e.source["add_PropertyChanged"] !== undefined) {
            // add property change handler to the source
            e.source.add_PropertyChanged(e.handler);
        }
        else {
            // may not have INotifyPropertyChanged
            // trace("WARNING: object [" + e.source["$type"] + "] has no property change handler.");
        }
    }
    else {
        // do not have a source to bind to.
    }
}

Binding.prototype.evaluate = function () {
    var value = null;

    var n = this._elements.length;

    if (n == 1)
    {
        value = this._elements[0].source;
    }

    for (var j = 1; j < n; ++j) {
        var source = this._elements[j - 1].source;

        var e = this._elements[j];

        if (!source) {
            value = null;
        } else {
            value = GetPropertyValue(source, e.name);
        }

        if (value === undefined) {
            // ensure null
            value = null;
        }

        // container element, request handler
        this.updatehandler(j, value);
    }

    if (this.__verbose) {
        if (null != value) {
            trace("evaluate path '" + this._path + "' => " + value + " (" + value["$type"] + ")");
        }
        else {
            trace("evaluate path '" + this._path + "' => not set.");
        }
    }

    if (!!this.onchanged && !this.isupdating) {
        // trigger the binding's change handler
        this.onchanged(this, value);
    }

    return value;
}

Binding.prototype.update = function (value) {
    var n = this._elements.length;
    var e = this._elements[n - 1];
    var parent = this._elements[n - 2];

    try {
        this.isupdating = true;

        // trace("binding update source: " + parent.source["id"]);

        if (!!parent.source) {
            SetPropertyValue(parent.source, e.name, value);
        }
        else {
            // trace("update source failed, property '" + e.name + "' is not accessible.");
        }
    }
    finally {
        this.isupdating = false;
    }
}

Binding.prototype.clear = function ()
{
    this.clearhandlers(0);
}


var BindingOperations = {
    SetBinding: function (target, prop, binding) {

    },
    ClearAllBindings: function (target) {
        if (target.clearallbindings !== undefined) {
            target.clearallbindings();
        }
        else {
            trace("WARNING: type " + target.$type + " has not clearbindings");
        }
    }
}

BindingObject.prototype.constructor = BindingObject;


function BindingObject(target, prop, binding)
{
    var __this = this;
    this.target = target;
    this.prop = prop;
    this.tosource = false;
    this.fromsource = true;
    this.handler = null;

    if (binding.mode === undefined) {
        binding.mode = "Default";
    }

    // validate target

    this.get_Property = function () { return __this.prop; };

    this.Clear = function () {
        __this.binding.clear();
        if (null != __this.handler) {
            __this.target.remove_PropertyChanged(__this.handler);
        }
    };

    this.toString = function () {
        return "BindingObject(" + this.target.$type + "#" + this.target.id + ", " + this.prop + ")";
    }

    this.binding = binding;

    // install change notification handler on the binding
    binding.onchanged = function (sender, value) {
        var converter = binding.get_Converter();
        if (!!converter) {
            value = converter.Convert(value);
        }
        __this.SetTargetValue(value);
    }

    // initial value
    var value = binding.evaluate();

    //trace("GGD: " + JSON.stringify({ type: this.target.$type, id: ctrl.id, tag: ctrl.tagname }));

    // get target property info
    var mode = binding.mode;
    if (mode == "Default") {
        this.dp = GetDepdencencyProperty(this.target, prop);
        if (null != this.dp) {
            if (this.dp.BindsTwoWayByDefault) {
                this.tosource = true;
                mode = "TwoWay";
            }
            else {
                mode = "OneWay";
            }
        }
        else {
            mode = "OneWay";
        }
    }
    else if (mode == "TwoWay") {
        this.tosource = true;
        mode = "TwoWay";
    }
    else {
        throw "unsupported binding mode parameter '" + mode + "'.";
    }

    // TODO: handle mode parameter

    // TODO: duplex?
    if (this.tosource) {
        this.handler = {
            id: "h" + handlerindex++,
            execute: function (sender, e) {
                __this.UpdateSourceValue();
            },
        };

        if (!!this.target && !!this.target["add_PropertyChanged"]) {
            this.target.add_PropertyChanged(this.handler);
        }
    }

    // trace("binding => (" + this.toString() + ") => (" + this.binding.toString() + ") := " + value);

    if (!this.target.registerbinding) {
        trace("WARNING: " + this.toString() + " does not support registerbinding.");
    }
    else {
        this.target.registerbinding(this);
    }
}


BindingObject.prototype.SetTargetValue = function (value) {
    SetPropertyValue(this.target, this.prop, value);
}

BindingObject.prototype.UpdateSourceValue = function () {
    var value = GetPropertyValue(this.target, this.prop);
    // trace("from " + this.toString() + " update source value: " + value);
    this.binding.update(value);
}

/*
    Creates a data binding from source (source, path) to target (ctrl, prop) (G4M6XZEASP).

    Extracts the target wrapper object.
    Constructs a binding source object.
*/
function CreateBindingObject(ctrl, prop, source, path, converter, mode) {
    // create binding object
    var binding = binding = new Binding(path);
    binding.set_Converter(converter);
    binding.set_Source(source);

    // resolve target wrapper
    var target = ControlFactory.getcontrolwrapper(ctrl);
    if (target["$type"] === undefined) {
        trace("WARNING: wrapper of '" + ctrl.id + "' has no type associated.");
        return;
    }

    // use core method
    return new BindingObject(target, prop, binding);
}
