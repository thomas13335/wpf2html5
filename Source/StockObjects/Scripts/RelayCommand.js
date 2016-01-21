
RelayCommand.prototype.constructor = RelayCommand;

RelayCommand.prototype.$static = { }

function RelayCommand(handler) {
    this.$type = "RelayCommand";
    this.handler = handler;
}

RelayCommand.prototype.Execute = function (p) {
    this.handler(p);
}

RelayCommand.prototype.toString = function () {
    return this.$type + ": " + this.handler;
}