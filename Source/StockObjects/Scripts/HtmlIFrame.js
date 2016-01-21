// ------------------------------------------------------------------------------------------------

function HtmlIFrame(ctrl) {
    this.ctrl = ctrl;
    this.$type = "HtmlIFrame";
}

HtmlIFrame.prototype.set_Location = function (url) {

    var embed = document.createElement("iframe");
    embed.setAttribute("src", url);
    embed.setAttribute("class", "htmliframe");
    embed.setAttribute("type", "text/html");
    var noembed = document.createElement("noembed");
    noembed.innerText = "Browser does not support media format.";
    embed.appendChild(noembed);

    this.ctrl.innerHTML = embed.outerHTML;

    var tobesized = this.ctrl.firstChild;

    trace("tobesized: " + tobesized.tagName);

    var crect = this.ctrl.getBoundingClientRect();
    trace("clientrect: " + crect.top + ", " + crect.bottom);

    trace("fitting to screen ...");
    ResizeManager_FitToScreen(tobesized);
}

