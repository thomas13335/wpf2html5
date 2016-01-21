

var Delegate = {
    outstanding: 0,
    Combine: function (list, e) {
        if (!e.id) {
            e.id = "h" + handlerindex++;
        }
        if (!list) {
            list = [e];
        }
        else{
            list.splice(list.length, 0, e);
        }

        this.outstanding++;

        // trace("delegate " + this.outstanding + " after combine.");

        return list;
    },
    Fire: function (list, sender, e) {
        if (!!list) {
            var tocall = [];
            for (var j = 0; j < list.length; ++j) {
                tocall[j] = list[j];
            }

            for (var j = 0; j < tocall.length; ++j) {
                var h = tocall[j];
                h.execute(sender, e);
            }
        }
    },
    Remove: function (list, e) {
        if (e.id) {
            for (var j = 0; j < list.length; ++j) {
                var h = list[j];
                if (h.id == e.id) {
                    list.splice(j, 1);
                    this.outstanding--;
                    --j;
                }
            }
        } else {
            trace("unable to remove delegate listener, no identifier.");
        }

        // trace("delegate " + this.outstanding + " after remove.");

        return list;
    }
};

function EventArgs() {
    $type = "EventArgs";
}