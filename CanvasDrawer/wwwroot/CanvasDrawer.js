window.addEventListener('resize', () => Tools.windowResize());
window.addEventListener('scroll', () => Tools.windowScroll());

//for rubberbanding
var backgroundImage


window.Tools = {

    //in resonse to window resize clear and call csharp
    windowResize: function () {
        canvasDrawer.clear();
        //call the csharp method
        DotNet.invokeMethodAsync('CanvasDrawer', 'WindowResized')
            .then(message => {
            });
    },

    windowScroll: function () {
        //call the csharp method
        DotNet.invokeMethodAsync('CanvasDrawer', 'WindowScrolled')
            .then(message => {
                console.log("[windowScrolled]");
            });
    },
}

window.canvasDrawer = {

    //the window's inner height
    frameHeight: function () {
        var h = window.innerHeight;
        return h.toString();
    },

    //the window's inner height
    frameWidth: function () {

        var w = window.innerWidth;
        return w.toString();
    },

    //used for rubberbanding
    //copies the canvas data to an offscreen background image
    getImageData: function () {
        var mainCan = document.getElementById("maincanvas");
        if (mainCan != null) {
            var ctx = mainCan.getContext("2d");
            backgroundImage = ctx.getImageData(0, 0, mainCan.width, mainCan.height);
        }
    },

    //used for rubberbanding
    //copies from the background image on to the canvas
    putImageData: function () {
        var mainCan = document.getElementById("maincanvas");
        if (mainCan != null) {
            var ctx = mainCan.getContext("2d");
            ctx.putImageData(backgroundImage, 0, 0);
        }
    },

    //used for rubberbanding
    //copies from a rectangular area in the background image on to the canvas
    putImageDirtyRect: function (x, y, w, h) {
        var mainCan = document.getElementById("maincanvas");
        if (mainCan != null) {
            var ctx = mainCan.getContext("2d");
            ctx.putImageData(backgroundImage, 0, 0, x, y, w, h);
        }
    },

    //magic that fixes low res blur
    fixblur: function () {


        var dpi = window.devicePixelRatio;
        var mainCan = document.getElementById("maincanvas");

        if (mainCan != null) {
            let style_height = +getComputedStyle(mainCan).getPropertyValue("height").slice(0, -2);

            let style_width = +getComputedStyle(mainCan).getPropertyValue("width").slice(0, -2);

            mainCan.setAttribute('height', style_height * dpi);
            mainCan.setAttribute('width', style_width * dpi);
        }
        return "done";
    },

    //get the dpi
    dpi: function () {
        return window.devicePixelRatio;
    },

    //put a string into local storage
    localStoragePutString: function (name, value) {
        localStorage.setItem(name, value);
    },

    //retrieve a string from local storage
    localStorageGetString: function (name) {
        return localStorage.getItem(name);
    },

    //clear everything from local storage
    localStorageClear: function () {
        localStorage.clear();
    },

    //remove a string from local storage
    localStorageRemoveString(name) {
        localStorage.removeItem(name);
    },

    //show the javascript prompt
    showPrompt: function (text, helptext) {
        return prompt(text, helptext);
    },

    //example of font  "30px Arial"
    drawText: function (x, y, text, font, color, align) {
        var mainCan = document.getElementById("maincanvas");
        var ctx = mainCan.getContext("2d");
        if (mainCan != null) {
            ctx.fillStyle = color;
            ctx.textAlign = align;
            ctx.font = font;
            ctx.fillText(text, x, y);
        }
    },

    //get the width of the text
    textWidth: function (text, font) {
        var mainCan = document.getElementById("maincanvas");
        if (mainCan != null) {
            var ctx = mainCan.getContext("2d");
            ctx.font = font;
            var metrics = ctx.measureText(text);
            return metrics.width;
        }
        return 0;
    },

    //write a message to the log
    logMessage: function (level, message) {
        console.log("[" + level + "] " + message);
    },

    //restore the context
    restore: function () {
        var mainCan = document.getElementById("maincanvas");
        if (mainCan != null) {
            var ctx = mainCan.getContext("2d");
            ctx.setTransform(1, 0, 0, 1, 0, 0);
        }
    },

    //scale
    scale: function (scalex, scaley) {
        var mainCan = document.getElementById("maincanvas");
        if (mainCan != null) {
            var ctx = mainCan.getContext("2d");
            ctx.scale(scalex, scaley);
        }
    },

    //draw a line
    drawLine: function (x1, y1, x2, y2, color, lineWidth, dashval) {
        var mainCan = document.getElementById("maincanvas");
        if (mainCan != null) {
            var ctx = mainCan.getContext("2d");
            ctx.save();

            ctx.beginPath();
            ctx.moveTo(x1, y1);
            ctx.lineTo(x2, y2);
            //   ctx.fillStyle = color;
            ctx.lineWidth = lineWidth;
            ctx.strokeStyle = color;
            if ((dashval == null) || (dashval < 0.001)) {
                ctx.setLineDash([]);
            }
            else {
                ctx.setLineDash([dashval, dashval]);
            }
            ctx.stroke();
            ctx.restore();
        }
    },

    //draw a simple ellipse.
    drawEllipse(xc, yc, radx, rady, fillColor, borderColor, lineWidth) {
        var mainCan = document.getElementById("maincanvas");
        if (mainCan != null) {
            var ctx = mainCan.getContext("2d");

            ctx.save();
            ctx.lineWidth = lineWidth;

            ctx.setLineDash([]);

            ctx.beginPath();
            ctx.ellipse(xc, yc, radx, rady, 0, 0, 2 * Math.PI);

            if (fillColor) {
                ctx.fillStyle = fillColor;
                ctx.fill();
            }
            if (borderColor) {
                ctx.strokeStyle = borderColor;
                ctx.stroke();
            }
            ctx.restore();
        }
        return "";
    },

    //angles are in radians
    drawArc: function (x, y, r, startAngle, endAngle, fillColor, borderColor, lineWidth, dashval) {
        var mainCan = document.getElementById("maincanvas");
        if (mainCan != null) {
            var ctx = mainCan.getContext("2d");

            ctx.save();
            ctx.lineWidth = lineWidth;

            if ((dashval == null) || (dashval < 0.001)) {
                ctx.setLineDash([]);
            }
            else {
                ctx.setLineDash([dashval, dashval]);
            }

            ctx.beginPath();
            ctx.arc(x, y, r, startAngle, endAngle);

            if (fillColor) {
                ctx.fillStyle = fillColor;
                ctx.fill();
            }
            if (borderColor) {
                ctx.strokeStyle = borderColor;
                ctx.stroke();
            }
            ctx.restore();
        }
        return "";
    },

    //fill a rectangle
    fillRectangle: function (left, top, width, height, fillColor, borderColor, lineWidth, dashval) {

        var mainCan = document.getElementById("maincanvas");
        if (mainCan != null) {
            var ctx = mainCan.getContext("2d");
            ctx.save();
            ctx.lineWidth = lineWidth;

            if ((dashval == null) || (dashval < 0.001)) {
                ctx.setLineDash([]);
            }
            else {
                ctx.setLineDash([dashval, dashval]);
            }

            if (fillColor) {
                ctx.fillStyle = fillColor;
                ctx.fillRect(left, top, width + 1, height + 1);
            }
            if (borderColor) {
                ctx.strokeStyle = borderColor;
                //         ctx.strokeRect(left - lineWidth, top - lineWidth, width + 2 * lineWidth, height + 2 * lineWidth);
                ctx.strokeRect(left, top, width + lineWidth, height + lineWidth);
            }

            ctx.restore();
        }
        return "";
    },

    //draw an image
    drawImage: function (x, y, width, height, imageid) {
        var mainCan = document.getElementById("maincanvas");
        if (mainCan != null) {
            var ctx = mainCan.getContext("2d");
            var image = document.getElementById(imageid);
            if (image != null) {
                ctx.drawImage(image, x, y, width, height);
            }
        }
    },

    //draw a rotated image, angle in radians
    //x and y are the rotation center
    drawRotatedImage: function (x, y, width, height, angle, imageid) {
        var mainCan = document.getElementById("maincanvas");
        if (mainCan != null) {
            var ctx = mainCan.getContext("2d");
            var image = document.getElementById(imageid);
            if (image != null) {
                ctx.save();
                ctx.translate(x, y);
                ctx.rotate(angle);
                ctx.drawImage(image, -width / 2, -height / 2, width, height);
                ctx.restore();
            }

        }
    },

    //update the inner text of an element
    updateInnerText: function (elid, message) {
        var el = document.getElementById(elid);

        if (el != null) {
            el.innerText = message;
        }
        return "done";
    },

    //get the scroll left of an element
    getScrollLeft: function (elid) {
        var el = document.getElementById(elid);

        if (el != null) {
            return el.scrollLeft.toString();
        }
        return "";
    },

    //get the scroll top of an element
    getScrollTop: function (elid) {
        var el = document.getElementById(elid);

        if (el != null) {
            return el.scrollTop.toString();
        }
        return "";
    },


    //get the value text of an input element
    getInputValue: function (elid) {
        var el = document.getElementById(elid);

        if (el != null) {
            var text = el.value;
            console.log("INNER TEXT: [" + text + "]");
            return text;
        }

        console.log("INNER TEXT: [???]");
        return "???";
    },

    //get the value text of an input element
    setInputValue: function (elid, text) {
        var el = document.getElementById(elid);

        if (el != null) {
            el.value = text;
        }

        return "";
    },

    //scroll the page so an element is visible
    scrollIntoView: function (elid) {
        var el = document.getElementById(elid);

        if (el != null) {
            el.scrollIntoView();
        }

        return "";
    },

    //enable (a button)
    enable: function (elid) {
        var el = document.getElementById(elid);
        if (el != null) {
            el.disabled = false;
        }
        return "done";
    },

    alert: function (text) {
        alert(text);
    },

    //OK CANCEL confirm
    confirm: function (prompt) {
        var txt;
        if (confirm(prompt)) {
            txt = "OK";
        }
        else {
            txt = "CANCEL";
        }
        return txt;
    },

    //disable (a button)
    disable: function (elid) {
        var el = document.getElementById(elid);
        if (el != null) {
            el.disabled = true;
        }
        return "done";
    },


    //change the border color of an element
    changeBorder: function (elid, color) {
        var el = document.getElementById(elid);
        if (el != null) {
            el.style.borderColor = color;
        }
        return "done";
    },

    //change the background color of an element
    changeBackground: function (elid, color) {
        var el = document.getElementById(elid);
        if (el != null) {
            el.style.background = color;
        }
        return "done";
    },

    //change the document (body) background
    changeBodyBackground: function (color) {
        document.body.style.background = color;
        return "done";
    },

    //change the document (body) background
    changeBodyBorder: function (color) {
        document.body.style.border.color = color;
        return "done";
    },

    //set the cursor style
    canvasCursor: function (cur) {

        var mainCan = document.getElementById("maincanvas");

        if (String(cur).includes("img_")) {

            var image = document.getElementById(cur);

            var ustr = "url(\"";
            ustr = ustr.concat(image.src);
            ustr = ustr.concat("\") 24 24, auto");

            mainCan.style.cursor = ustr;
        }
        else {
            mainCan.style.cursor = cur;
        }


    },

    //clear the main canvas
    clear: function () {
        var mainCan = document.getElementById("maincanvas");

        if (mainCan != null) {
            var ctx = mainCan.getContext("2d");
            ctx.save();
            ctx.clearRect(0, 0, mainCan.width, mainCan.height);
        }
    },

    translate: function (dx, dy) {
        var mainCan = document.getElementById("maincanvas");

        if (mainCan != null) {
            var ctx = mainCan.getContext("2d");

            console.log("translating dx = " + dx + "  dy = " + dy);
            ctx.translate(dx, dy);
        }
    },

    //get the width of the main canvas
    //so that can convert mouse to local
    canvasWidth: function () {
        var mainCan = document.getElementById("maincanvas");
        if (mainCan != null) {
            var width = mainCan.clientWidth;
            return width.toString();
        }
        return "0";
    },

    //get the height of the main canvas
    //so that can convert mouse to local
    canvasHeight: function () {
        var mainCan = document.getElementById("maincanvas");
        if (mainCan != null) {
            var height = mainCan.clientHeight;
            return height.toString();
        }
        return "0";
    },


    //get the client left of the main canvas
    canvasLeft: function () {
        var mainCan = document.getElementById("maincanvas");
        if (mainCan != null) {
            var left = mainCan.clientLeft;
            return left.toString();
        }
        return "0";
    },

    //get the client top of the main canvas
    canvasTop: function () {
        var mainCan = document.getElementById("maincanvas");
        if (mainCan != null) {
            var top = mainCan.clientTop;
            return top.toString();
        }
        return "0";
    },

    //get the left of the main canvas in css pixels
    //so that can convert mouse to local
    cssLeft: function () {
        var mainCan = document.getElementById("maincanvas");
        if (mainCan != null) {
            var width = mainCan.clientLeft;
            return width.toString();
        }
        return "0";
    },


    //get the top of the main canvas in css pixels
    //so that can convert mouse to local
    cssTop: function () {
        var mainCan = document.getElementById("maincanvas");
        if (mainCan != null) {
            var height = mainCan.top;
            return height.toString();
        }
        return "0";
    },

    //get the width of the main canvas in css pixels
    //so that can convert mouse to local
    cssWidth: function () {
        var mainCan = document.getElementById("maincanvas");
        if (mainCan != null) {
            var width = mainCan.width;
            return width.toString();
        }
        return "0";
    },


    //get the height of the main canvas in css pixels
    //so that can convert mouse to local
    cssHeight: function () {
        var mainCan = document.getElementById("maincanvas");
        if (mainCan != null) {
            var height = mainCan.height;
            return height.toString();
        }
        return "0";
    },

    //used to get mouse coords relative to canvas
    canvasOffsetLeft: function () {
        var mainCan = document.getElementById("maincanvas");
        if (mainCan != null) {
            const rect = mainCan.getBoundingClientRect();
            return rect.left;
        }
        return "0";
    },

    //used to get mouse coords relative to canvas
    canvasOffsetTop: function () {
        var mainCan = document.getElementById("maincanvas");
        if (mainCan != null) {
            const rect = mainCan.getBoundingClientRect();
            return rect.top;
        }
        return "0";
    },

    //get an element's bounding rect
    clientBoundingRect: function (elid) {
        var el = document.getElementById(elid);
        if (el != null) {
            const rect = el.getBoundingClientRect();
            return rect.left + ", " + rect.top + ", " + rect.width + ", " + rect.height;
        }
        return "";
    },

    disableRightClickContextMenuOnShapeButton: function () {
        var mainCan = document.getElementById("subnet");
        if (mainCan != null) {
            mainCan.addEventListener('contextmenu', function (e) {
                if (e.button == 2) {
                    e.preventDefault();
                }
            })
        }
    },

    disableRightClickContextMenuOnConnectorButton: function () {
        var mainCan = document.getElementById("connector");
        if (mainCan != null) {
            mainCan.addEventListener('contextmenu', function (e) {
                if (e.button == 2) {
                    e.preventDefault();
                }
            })
        }
    },

    disableRightClickContextMenu: function () {
        var mainCan = document.getElementById("maincanvas");
        if (mainCan != null) {

            console.log("DISABLING RIGHT CLICK")
            mainCan.addEventListener('contextmenu', function (e) {
                if (e.button == 2) {
                    e.preventDefault();
                }
            })
        }

        var vtb = document.getElementById("mainframe");
        if (vtb != null) {
            vtb.addEventListener('contextmenu', function (e) {
                if (e.button == 2) {
                    e.preventDefault();
                }
            })

            //remove default CTRL-Shift behaviors so they don't interfere with our shortcuts
            vtb.addEventListener('keydown', function (e) {
                e = e || window.event;//Get event
                if (!e.ctrlKey) return;
                if (!e.shiftKey) return;
                e.preventDefault();
            })
        }


    },

};




