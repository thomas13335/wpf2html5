

function TemplateContainer() {
    this.$type = "TemplateContainer";
    this._templates = {};
}

function TemplateFactoryClass() {
    this._cmap = {};
    this.verbose = false;
}

TemplateFactoryClass.prototype.registertemplate = function (type, container, key, factory) {
    if (key != null)
    {
        // TODO: named resources are currently ignored.
        return;
    }

    var tcol = this._cmap[container];
    if(tcol === undefined)
    {
        this._cmap[container] = tcol = new TemplateContainer();
    }

    tcol._templates[type] = factory;
}

/*
    Retreives a DataTemplate registered for a type within a control context.
*/
TemplateFactoryClass.prototype.gettemplate = function (ctrlarg, type) {
    var ctrl = ctrlarg;
    if(this.verbose) trace("get template for type [" + type + "] ...");
    while (ctrl !== undefined && ctrl != null) {
        if (this.verbose) trace("  control " + ctrl.tagName + "#" + ctrl.id + " ...");

        // get control template context or use id
        var id = ctrl.id;
        var templatecontext = ctrl.getAttribute("data-template-context");
        if (!!templatecontext) {
            id = templatecontext;
            if (this.verbose) trace("  template context '" + id + "'.");
        }

        // look in the (id, type) => factory map
        var tcol = this._cmap[id];
        if (tcol) {
            var factory = tcol._templates[type];
            if (factory) {
                if (this.verbose) trace("  ==> found.");
                return factory;
            }
        }

        // continue with parent element
        ctrl = ctrl.parentElement;
    }

    // undefined
    trace("template factory did not find template for [" + type + "] in the context of '"
        + ctrlarg["data-template-context"] + "'.");
}

var TemplateFactory = new TemplateFactoryClass();
