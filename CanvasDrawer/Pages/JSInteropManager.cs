using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using CanvasDrawer.Graphics;

namespace CanvasDrawer.Pages {
    public sealed class JSInteropManager {

	    private readonly IJSRuntime _jsRuntime;
        private readonly IJSInProcessRuntime _synchRT;

        public double CanvasWidth { get; private set; }
        public double CanvasHeight { get; set; }
        public double CssWidth { get; set; }
        public double CssHeight { get; set; }
        public bool ScaleDirty { get; set; } = true;

        public static JSInteropManager? Instance { get; set; }

		// dots per inch
		public double DPI { get; set; } = -1;

        //zoom related
        public int ZoomLevel { get; set; } = 1;
        private readonly double _zoomFactor = 1.25;


        public DoublePoint? Scale { get; set; }
        private double _baseScaleX;
        private double _baseScaleY;

        private double _offsetLeft;

        private double _offsetTop;

        private bool _offsetLeftDirty = true;

        private bool _offsetTopDirty = true;

        //The gateway to the javascript
        public JSInteropManager(IJSRuntime jsRuntime, bool checkScale) {
            _jsRuntime = jsRuntime;
            _synchRT = (IJSInProcessRuntime)_jsRuntime;
            if (checkScale) {
                CheckScale();
            }
            Instance = this;
        }

		/// <summary>
		/// The window has been resized.
		/// </summary>
		public void WindowResized() {
            FixBlur();
            _offsetTopDirty = true;
            _offsetLeftDirty = true;
            ScaleDirty = true;
            Scale = null;
            SetOffsetsDirty();
            CheckScale();
        }

        public void FixBlur() {
            _synchRT.Invoke<string>("canvasDrawer.fixblur");
        }

        /// <summary>
        /// The window has been scrolled
        /// </summary>
        public void WindowScrolled() {
            SetOffsetsDirty();
        }

        public void SetOffsetsDirty() {
            _offsetLeftDirty = true;
            _offsetTopDirty = true;
            GetCanvasOffsetLeft();
            GetCanvasOffsetTop();
        }

        //check if the scale has changed
        public void CheckScale() {
            if (ScaleDirty) {
                CanvasWidth = GetCanvasWidth();
                CanvasHeight = GetCanvasHeight();
                CssWidth = GetCSSWidth();
                CssHeight = GetCSSHeight();

                if (Scale == null) {
                    Scale = new DoublePoint();
                    _baseScaleX = CssWidth / CanvasWidth; 
                    _baseScaleY = CssHeight / CanvasHeight;
                }


                Scale.X = _baseScaleX * ZoomFactor();
                Scale.Y = _baseScaleY * ZoomFactor();

                ScaleDirty = false;
            }

        }

        //the zooming factor
        public double ZoomFactor() {
            int pow = ZoomLevel - 1;

            return Math.Pow(_zoomFactor, pow);
        }

        public override string ToString() {
            return String.Format("Scale: ({0:0.##}, {1:0.##})", Scale.X, Scale.Y);
        }

        //change the cursor
        public void SetCursor(string cur) {
            _synchRT.Invoke<string>("canvasDrawer.canvasCursor", cur);
        }

        //enable or disable (probably a button)
        public void Enable(string elid, bool enabled) {
            if (enabled) {
                _synchRT.Invoke<string>("canvasDrawer.enable", elid);
            }
            else {
                _synchRT.Invoke<string>("canvasDrawer.disable", elid);
            }
        }


        //get the text width
        public double TextWidth(string text, string fontFamily, int fontSize) {
            string fstr = "" + fontSize + "px " + fontFamily;
            return _synchRT.Invoke<Double>("canvasDrawer.textWidth", text, fstr);
        }

        //change the background color of an element
        public void ChangeBackground(string elid, string color) {
            _synchRT.Invoke<string>("canvasDrawer.changeBackground", elid, color);
        }

        //change the background color of the page (body)
        public void ChangeContainerBackground(string color) {
            _synchRT.Invoke<string>("canvasDrawer.changeBodyBackground", color);
        }

        //change the background bordercolor of the page (body)
        public void ChangeContainerBorderColor(string color) {
            _synchRT.Invoke<string>("canvasDrawer.changeBodyBorder", color);
        }
        //change the border color of an element
        public void ChangeBorder(string elid, string color) {
            _synchRT.Invoke<string>("canvasDrawer.changeBorder", elid, color);
        }

        /// <summary>
        /// Copy the canvas to a background image. The
        /// image is defined in the javascript file.
        /// </summary>
        public void SaveCanvasInBackgoundImage() {
            _synchRT.Invoke<string>("canvasDrawer.getImageData");
        }

        /// <summary>
        /// Restore the canvas from an offscreen background image.
        /// The image is stored in the javascript.
        /// </summary>
        public void RestoreCanvasFromBackgroundImage() {
            _synchRT.Invoke<string>("canvasDrawer.putImageData");
        }

        /// <summary>
        /// Restore a rectangular area of the canvas from an offscreen background image.
        /// </summary>
        /// <param name="x">The left of the rectangular area.</param>
        /// <param name="y">The top of the rectangular area.</param>
        /// <param name="w">The width of the rectangular area.</param>
        /// <param name="h">The height of the rectangular area.</param>
        public void RestoreRectangularAreaFromBackgroundImage(double x, double y, double w, double h) {


            if (w < 0) {
                w = -w;
                x = x - w;
            }

            if (h < 0) {
                h = -h;
                y = y - h;
            }

            CheckScale();

            x *= Scale.X;
            y *= Scale.Y;

            w *= Scale.X;
            h *= Scale.Y;

            x = Math.Floor(x);
            y = Math.Floor(y);

            w = Math.Ceiling(w);
            h = Math.Ceiling(h);

            //add some slop
            _synchRT.Invoke<string>("canvasDrawer.putImageDirtyRect", x - 20, y - 20, w + 40, h + 40);
        }

        public string Confirm(string prompt) {
            return _synchRT.Invoke<string>("canvasDrawer.confirm", prompt);
        }

        /// <summary>
        /// A simple alert message
        /// </summary>
        /// <param name="message">The alert message</param>
        public void Alert(String message) {
            _synchRT.Invoke<string>("canvasDrawer.alert", message);
        }

        public string Prompt(string prompt, string helpText) {
            string reply = _synchRT.Invoke<string>(
                    "canvasDrawer.showPrompt",
                    prompt, helpText);
            return reply;
        }

        public double GetCanvasOffsetLeft() {
            if (_offsetLeftDirty) {
                try {
                    _offsetLeft = _synchRT.Invoke<double>("canvasDrawer.canvasOffsetLeft");
                    _offsetLeftDirty = false;
                }
                catch (Exception e) {
                    Console.WriteLine(e.StackTrace);
                }
            }
            return _offsetLeft;
        }

        public double GetCanvasOffsetTop() {
            if (_offsetTopDirty) {
                try {
                    _offsetTop = _synchRT.Invoke<double>("canvasDrawer.canvasOffsetTop");
                    _offsetTopDirty = false;
                }
                catch (Exception e) {
                    Console.WriteLine(e.StackTrace);
                }
            }
            return _offsetTop;
        }

        //get the width of the canvas
        public double GetCanvasWidth() {
            try {
                string result = _synchRT.Invoke<string>("canvasDrawer.canvasWidth");
                return Double.Parse(result);
            }
            catch (Exception e) {
                Console.WriteLine(e.StackTrace);
                return 0;
            }
        }

        //Get the height of the canvas
        public double GetCanvasHeight() {
            try {
                string result = _synchRT.Invoke<string>("canvasDrawer.canvasHeight");
                return Double.Parse(result);
            }
            catch (Exception e) {
                Console.WriteLine(e.StackTrace);
                return 0;
            }
        }

        //get the CSS width
        public double GetCSSWidth() {
            string result = _synchRT.Invoke<string>("canvasDrawer.cssWidth");
            return Double.Parse(result);
        }

        //get the css height
        public double GetCSSHeight() {
            string result = _synchRT.Invoke<string>("canvasDrawer.cssHeight");
            return Double.Parse(result);
        }

        public Rect ClientBoundingRect(string elemId) {
            string result = _synchRT.Invoke<string>("canvasDrawer.clientBoundingRect", elemId);
            if ((result == null) || (result.Length < 1)) {
                return null;
            }

            string[] vals = result.Split(',');
            double left = Double.Parse(vals[0]);
            double top = Double.Parse(vals[1]);
            double width = Double.Parse(vals[2]);
            double height = Double.Parse(vals[3]);
            return new Rect(left, top, width, height);
        }


        //log a message
        public async Task<string> LogMessage(ELogLevel level, string message) {
            string v = await _jsRuntime.InvokeAsync<string>("canvasDrawer.logMessage", level.ToString(), message);
            return v;
        }

        public void translate(double dx, double dy) {
            _ = _synchRT.Invoke<string>("canvasDrawer.translate", dx, dy);
        }

        //draw an arc, angles are in radians
        public void DrawArc(double x, double y, double rad,
            double startAngle, double endAngle, string fillColor, string borderColor, double lineWidth, double dashLength) {
            CheckScale();

            if (Scale == null) {
                return;
            }
            double avgScale = (Scale.X + Scale.Y) / 2.0;

            _ = _synchRT.Invoke<string>("canvasDrawer.drawArc", Scale.X * x, Scale.Y * y, avgScale * rad, startAngle, endAngle,
                fillColor, borderColor, lineWidth, dashLength);
        }

        //example of font  "30px Arial"
        public void DrawText(double x, double y, string text, int size, string family, string color, string align) {

            CheckScale();


			if (Scale == null) {
				return;
			}

			if (!CheckDouble(Scale.X) || !CheckDouble(Scale.Y)) {
                return;
            }

            double avgScale = (Scale.X + Scale.Y) / 2.0;

            int scaledfs = Math.Max(8, (int)(avgScale * size));

            //for now, use normal 400 weight
            string font = "normal " + scaledfs + "px " + family;

            _ = _synchRT.Invoke<string>("canvasDrawer.drawText", Scale.X * x, Scale.Y * y, text, font, color, align);
        }

        /// <summary>
        /// Check if the value is NaN or infinity.
        /// </summary>
        /// <param name="val">the number to check</param>
        /// <returns>true if thisis a good value</returns>
        private bool CheckDouble(double val) {
           return !Double.IsNaN(val) && !Double.IsInfinity(val);
        }

        /// <summary>
        /// Draw an image
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="imageId"></param>
        public void DrawImage(double x, double y, double w, double h, string imageId) {

            CheckScale();

			if (Scale == null) {
				return;
			}
			if (!CheckDouble(Scale.X) || !CheckDouble(Scale.Y)) {
                return;
            }

            _ = _synchRT.Invoke<string>("canvasDrawer.drawImage", Scale.X * x, Scale.Y * y,
                Scale.X * w, Scale.Y * h, imageId);
        }

        /// <summary>
        /// Draw a rotated image
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="angle">The angle in radians</param>
        /// <param name="imageId"></param>
        public void DrawRotatedImage(double x, double y, double w, double h, double angle, string imageId) {

            CheckScale();

			if (Scale == null) {
				return;
			}
			if (!CheckDouble(Scale.X) || !CheckDouble(Scale.Y)) {
                return;
            }

            _ = _synchRT.Invoke<string>("canvasDrawer.drawRotatedImage", Scale.X * x, Scale.Y * y,
                Scale.X * w, Scale.Y * h, angle, imageId);
        }

        public void DrawLine(double x1, double y1, double x2, double y2, string color, double lineWidth = 0, double dashLength = 0) {
            CheckScale();
            double avgScale = (Scale.X + Scale.Y) / 2.0;

            _ = _synchRT.Invoke<string>("canvasDrawer.drawLine", Scale.X * x1, Scale.Y * y1, Scale.X * x2,
                Scale.Y * y2, color, avgScale * lineWidth, avgScale * dashLength);
        }

        /// <summary>
        /// Draw a simple, upright ellipse
        /// </summary>
        /// <param name="xc">The horizontal center.</param>
        /// <param name="yc">The vertical center.</param>
        /// <param name="radx">The horizontal radius.</param>
        /// <param name="rady">The vertical radius.</param>
        /// <param name="fillColor">The fill color.</param>
        /// <param name="borderColor">The line color.</param>
        /// <param name="lineWidth">The line width.</param>
        public void DrawEllipse(double xc, double yc, double radx, double rady,
            string fillColor, string borderColor, double lineWidth = 0) {
            CheckScale();

			if (Scale == null) {
				return;
			}
			double avgScale = (Scale.X + Scale.Y) / 2.0;
            _ = _synchRT.Invoke<string>("canvasDrawer.drawEllipse", Scale.X * xc, Scale.Y * yc, Scale.X * radx, Scale.Y * rady, fillColor, borderColor, avgScale * lineWidth);
		}

        /// <summary>
        /// Draw a rectangle.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="fillColor"></param>
        /// <param name="borderColor"></param>
        /// <param name="lineWidth"></param>
        /// <param name="dashLength"></param>
        public void DrawRectangle(double left, double top, double width, double height,
            string fillColor, string borderColor, double lineWidth = 0, double dashLength = 0) {

            CheckScale();

            if (Scale == null) {
                return;
            }
            double avgScale = (Scale.X + Scale.Y) / 2.0;
           _ = _synchRT.Invoke<string>("canvasDrawer.fillRectangle", Scale.X * left, Scale.Y * top, Scale.X * width, Scale.Y * height, fillColor, borderColor, avgScale * lineWidth, avgScale * dashLength);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <param name="fillColor"></param>
        /// <param name="borderColor"></param>
        /// <param name="lineWidth"></param>
        /// <param name="dashLength"></param>
        public void DrawRectangle(Rect r, string fillColor, string borderColor,
            double lineWidth = 0, double dashLength = 0) {
            DrawRectangle(r.X, r.Y, r.Width, r.Height, fillColor, borderColor, lineWidth, dashLength);
        }

			/// <summary>
			/// Get the rectangle bounded by the canvas scroll bars.
			/// </summary>
			/// <returns>the rectangle bounded by the canvas scroll bars.</returns>
			public Rect CanvasScrollBoundaryRect() {
            Rect r = ClientBoundingRect("dcboundary");
            r.Move(-GetCanvasOffsetLeft(), -GetCanvasOffsetTop());
            return r;
        }


        /// <summary>
        /// Get the dots per inch.
        /// </summary>
        /// <returns>The dots per in value.</returns>
        public double GetDPI() {
            if (DPI < 1) {
                DPI = _synchRT.Invoke<double>("canvasDrawer.dpi");
            }
            return DPI;
        }

        /// <summary>
        /// Clear the
        /// </summary>
        public void Clear() {
            _ = _synchRT.Invoke<string>("canvasDrawer.clear");
        }

        //put a string into local storage
        public void LocalStoragePutString(string name, string value) {
            _synchRT.Invoke<string>("canvasDrawer.localStoragePutString", name, value);
        }

        //retrieve a string from local storage
        public string LocalStorageGetString(string name) {
            return _synchRT.Invoke<string>("canvasDrawer.localStorageGetString", name);
        }

        //clear everything from local storage
        public void localStorageClear() {
            _synchRT.Invoke<string>("canvasDrawer.localStorageClear");
        }

        //remove an item from local storage
        public void LocalStorageRemoveString(string name) {
            _synchRT.Invoke<string>("canvasDrawer.localStorageRemoveString", name);
        }

    }
}
