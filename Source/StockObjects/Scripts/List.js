
List.prototype = new Array();
List.prototype.constructor = List;

function List() {
    this.$type = "List";
}

List.prototype.Add = function (e) { this.push(e); }

List.prototype.get_Count = function () { return this.length; }

List.prototype.Clear = function () { this.splice(0, this.length); }

List.prototype.GetEnumerator = function () {
    return new ArrayEnumerator(this);
}


function ListUtility() {
}

ListUtility.prototype.ConvertList = function (list) {
    var a = Array.prototype.slice.call(list);

    var result = new List();
    for (var j = 0; j < a.length; ++j) {
        result.Add(list[j]);
    } 

    return result;
}

ListUtility.prototype.GetEnumerator = function (list) {
    return new ArrayEnumerator(list);
}
