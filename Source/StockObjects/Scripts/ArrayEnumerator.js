function ArrayEnumerator(arr) {
    this.items = arr;
    this.count = arr.length;
    this.index = 0;
    this.Current = null;
}

ArrayEnumerator.prototype.get_Current = function () {
    return this.Current;
}

ArrayEnumerator.prototype.MoveNext = function () {
    if (this.index < this.count) {
        this.Current = this.items[this.index++];
        return true;
    }
    else {
        this.Current = null;
        return false;
    }
}
