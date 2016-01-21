// ------------------------------------------------------------------------------------------------
// ItemsControl class
//
// Renders a set of items.
//
ItemsControl.prototype = new Control();
ItemsControl.prototype.constructor = ItemsControl;

ItemsControl.prototype.$static = {
    BaseClass: "Control",
    ItemsSourceProperty: {}
}

function ItemsControl(ctrl) {
    Control.prototype.constructor.call(this, ctrl);
    this.$type = "ItemsControl";
    this.ctrl = ctrl;
    this.itemssource = null;
    this.handlerinstalled = null;
    this.rendertag = "DIV";
}

/*
    Renders a single item into the collection.
*/ 
ItemsControl.prototype.renderitem = function (tg, item, before) {
    var presenter = null;

    if (item["$type"] === undefined) {
        // render as text
        trace("WARNING: no type specification for " + item + " " + item.id);
        presenter = document.createElement(this.rendertag);
        //TextBlock_SetText(presenter, item);
        presenter.appendChild(document.createTextNode(item));
        tg.insertBefore(presenter, before);
    }
    else {
        // use DataTemplate ...
        presenter = Control_generatecontainer(this.ctrl, this.rendertag);

        // insert presenter into visual tree before applying the template
        tg.insertBefore(presenter, before);

        // apply the template
        Control_applytemplate(tg, presenter, item);
    }

    return presenter;
}

ItemsControl.prototype.initialize = function () {
}

ItemsControl.prototype.populate = function () {

    var tg = this.ctrl;

    // clear all childnodes
    while (tg.firstChild != null) {
        Control_purge(tg.firstChild);
        tg.removeChild(tg.firstChild);
    }

    // enumerate the source and render items
    var enumerator = this.itemssource.GetEnumerator();
    while (enumerator.MoveNext()) {
        var item = enumerator.get_Current();
        var presenter = this.renderitem(tg, item);
    }
}

ItemsControl.prototype.onitemssourceupdated = function (sender, e) {
    var tg = this.ctrl;

    // trace("source updated ... " + e.Action);

    if (e.Action == "Reset") {
        // populate again
        this.populate();
    }
    else if (e.Action == "Remove") {
        var index = e.OldStartingIndex;
        for (var j = 0; j < e.OldItems.length; ++j) {
            var child = tg.children[index];
            Control_purge(child);
            tg.removeChild(tg.children[index]);
        }
    }
    else if (e.Action == "Add") {
        for (var j = 0; j < e.NewItems.length; ++j) {
            var item = e.NewItems[j];
            if (null == item) {
                trace("WARNING: null in collection, ignored.");
                continue;
            }
            var index = e.NewStartingIndex + j;
            var before = tg.children[index];

            var presenter = this.renderitem(tg, item, before);
            // tg.insertBefore(presenter, before);
        }
    }
}

ItemsControl.prototype.set_ItemsSource = function (value) {
    var ctrl = this.ctrl;

    /*if (value !== undefined && value != null) {
        trace("set_ItemsSource " + ctrl.id + " (" + ctrl.tagName + ") " + this.$type + " -> " + value['$type']);
    }
    else {
        trace("set_ItemsSource " + ctrl.id + " (" + ctrl.tagName + ") " + this.$type + " -> null");
    }*/ 

    if (this.handlerinstalled) {
        // remove existing handler
        this.itemssource.removechangehandler(this.handlerinstalled);
        this.handlerinstalled = null;
    }

    // transition to new source
    this.itemssource = value;

    var __this = this;
    var addchangehandler = value["addchangehandler"];
    if (addchangehandler !== undefined) {

        // install new change handler
        this.handlerinstalled = value.addchangehandler({
            execute: function (sender, e) {
                __this.onitemssourceupdated(sender, e);
            }
        });
    }

    this.populate();
}

