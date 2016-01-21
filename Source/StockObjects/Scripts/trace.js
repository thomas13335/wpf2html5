

var tracescreenobj = null;

// writes text to the console log or external object
function trace(text) {
    try { window.external.trace(text); } catch (e) { }
    try { console.log(text); } catch (e) { }


    /*if (tracescreenobj == null) {
        tracescreenobj = document.createElement("pre");
        document.body.appendChild(tracescreenobj);
    }

    var e = document.createElement("div");
    e.innerText = text;
    tracescreenobj.appendChild(e);*/
}

function __istype(obj, typename) {
    if (obj["$type"] === undefined) return false;
    if (obj.$type == typename) return true;

    // TODO: baseclass
}