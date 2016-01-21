
var thingtofit = null;

function ResizeManager_OnResize() {
    //trace("*** resize window (" + window.innerWidth + ", " + window.innerHeight + ")");

    updatethingtofit();

    var rect = window.ClientRect;
    if (!!rect) {
        idbody.style.width = rect.width;
    }
}

function updatethingtofit()
{
    if (null != thingtofit) {
        var ctrl = thingtofit;

        // use the body (CWVYGXULNX) to determine size
        var rect = idbody.getBoundingClientRect();
        var cwidth = rect.right - rect.left;

        // trace("resize-manager: update " + ctrl + " width = " + cwidth);

        // 5CQB5YWBDC
        ctrl.width = cwidth - 20;

        var rect = ctrl.getBoundingClientRect();
        var newheight = (window.innerHeight - rect.top - 20) + "px";
        ctrl.style.height = newheight;
    }
}

function ResizeManager_Initialize() {
    trace("*** resize manager");

    window.addEventListener("resize", function () { ResizeManager_OnResize(); });

    ResizeManager_OnResize();
}

function ResizeManager_FitToScreen(ctrl) {
    // ctrl.setAttribute("width", "99%");

    trace("resize-manager: thingtofit := " + ctrl);

    thingtofit = ctrl;

    window.setTimeout(function () { updatethingtofit(); }, 0);
    
}