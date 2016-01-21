function ObservableCollection() {
    this.$type = "ObservableCollection";
    this.items = [];
    this.observers = [];
    this.changehandler = null;
}

ObservableCollection.prototype.addchangehandler = function (callback) {
    this.changehandler = Delegate.Combine(this.changehandler, callback);
    return callback;
};

ObservableCollection.prototype.removechangehandler = function (callback) {
    this.changehandler = Delegate.Remove(this.changehandler, callback);
    return callback;
}

ObservableCollection.prototype.Add = function (item) {
    // trace("collection-add " + item["$type"] + ", " + item.Label);
    var index = this.items.length;
    this.items.push(item);
    var e = { Action: "Add", NewItems: [item], NewStartingIndex: index };
    this.changed(e);
}

ObservableCollection.prototype.Insert = function (index, item) {
    // trace("collection-insert " + item["$type"] + " at " + index);
    if (index > this.items.length) {
        index = this.items.length;
    }

    this.items.splice(index, 0, item);
    var e = { Action: "Add", NewItems: [item], NewStartingIndex: index };
    this.changed(e);
}

ObservableCollection.prototype.Clear = function () {
    var e = { Action: "Remove", OldItems: this.items, OldStartingIndex: 0 }
    this.items = [];

    if (e.OldItems.length > 0) {
        this.changed(e);
    }
}

ObservableCollection.prototype.Remove = function (obj) {
    var n = this.items.length;
    for(var index = 0; index < n; ++index)
    {
        if (this.items[index] == obj) {
            var olditem = this.items[index];
            this.items.splice(index, 1);
            var e = { Action: "Remove", OldItems: [ olditem ] , OldStartingIndex: index }
            this.changed(e);
            break;
        }
    }
}

ObservableCollection.prototype.get_Count = function () {
    return this.items.length;
}

ObservableCollection.prototype.Contains = function (item) {
    return this.items.indexOf(item) >= 0;
}

ObservableCollection.prototype.changed = function (e) {
    Delegate.Fire(this.changehandler, this, e);
}


ObservableCollection.prototype.GetEnumerator = function () {
    return new ArrayEnumerator(this.items);
}
