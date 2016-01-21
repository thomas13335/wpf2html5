

function CoerceInt32(svalue) {
    var pattern = /^[0-9]+(\.[0-9]+)?$/;
    var result = null;
    if (pattern.test(svalue)) {
        result = parseInt(svalue);
    }

    // trace("CoerceInt32: " + svalue + " -> " + result);

    return result;
}

function CoerceInt64(svalue) {
    var pattern = /^[0-9]+(\.[0-9]+)?$/;
    var result = null;
    if (pattern.test(svalue)) {
        result = parseInt(svalue);
    }

    // trace("CoerceInt32: " + svalue + " -> " + result);

    return result;
}

function CoerceBoolean(bvalue) {
    var result = !!bvalue;

    // trace("CoerceBoolean: " + bvalue + " -> " + result);

    return result;
}

function CoerceDateTime(dvalue) {
    return dvalue;
}
