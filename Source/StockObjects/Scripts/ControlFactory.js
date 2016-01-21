
var wrappersequence = 0;

// ------------------------------------------------------------------------------------------------
// ControlFactory static class
//
// Constructs controls based on HTML elements
var ControlFactory = {
    createcontrolwrapper: function(ctrl, ctype) {
        var result = null;
        if (!!ctype) {
            var ctor = window[ctype];
            if (ctor === undefined) {
                trace("[5EOLH5UROD] control class '" + ctype + "' is unavailable.");
            }
            else {
                result = new ctor(ctrl);
                result.__seq = ++wrappersequence;

                // trace("control [" + ctype + "] created for " + ctrl + " id " + result.id + " ...");
            }
        }

        return result;
    },

    // returns a possibly existing control wrapper for this object.
    querycontrolwrapper: function(ctrl) {
        var result = null;
        var hwrapper = ctrl["__hwrapper"];
        if (hwrapper !== undefined) {
            return ctrl.__hwrapper();
        }

        return;
    },

    /*
        RDLN7CYSPV: retrieves or generates a wrapper control on an HTML element
    */
    getcontrolwrapper: function getcontrolwrapper(ctrl) {
        var result = null;

        // if a wrapper was already attached to the control, use it
        var hwrapper = ctrl["__hwrapper"];
        if (hwrapper !== undefined) {
            result = ctrl.__hwrapper();
            if (null != result && result !== undefined) {
                // trace("using extra handler " + ctrl.id + " -> " + result.__seq + ".");
                return result;
            }
        }

        // not wrapper found, create new one
        var ctype = ctrl.getAttribute("data-ctype");
        if (ctype !== undefined) {
            result = this.createcontrolwrapper(ctrl, ctype);

        }

        if (!result) {
            // TODO: this code should be obsolete.
            // trace("[getcontrolwrapper] legacy path for '" + ctrl.tagName + "' ...");
            switch (ctrl.tagName) {
                case "SPAN":
                    trace("## legacy span.");
                    result = new TextBlock(ctrl);
                    break;

                case "SELECT":
                    result = new ComboBox(ctrl);
                    break;

                case "OPTION":
                    result = new Control(ctrl);
                    break;

                case "DIV":
                    switch (ctrl.getAttribute("data-kind")) {
                        case "content":
                            result = new ContentPresenter(ctrl);
                            break;

                        default:
                            result = new ItemsControl(ctrl);
                    }
                    break;

                case "INPUT":
                    {
                        var itype = ctrl.getAttribute("type");
                        switch (itype) {
                            case "text":
                                result = new TextBox(ctrl);
                                break;

                            case "button":
                                result = new Button(ctrl);
                                break;

                            case "checkbox":
                                result = new CheckBox(ctrl);
                                break;

                            default:
                                trace("WARNING: input type " + itype + " not handled." + ctrl.outerHTML);
                                break;
                        }
                    }
                    break;

                case "BUTTON":
                    result = new Button(ctrl);
                    break;

                case "IMG":
                    result = new Image(ctrl);
                    break;
            }
        }

        if (null == result) {

            if (result === undefined || result == null) {
                trace("WARNING: no wrapper class for control " + ctrl.tagName + "#" + ctrl.id + ".");
                result = null;
            }
        }

        //trace("wrapper [" + result.id + "] created for " + ctrl.tagName + "#" + ctrl.id);

        // attach wrapper to the HTML element via callback function
        ctrl["__hwrapper"] = function () { return result; };

        return result;
    }
}
