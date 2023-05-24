using System;
using CanvasDrawer.Graphics;
using CanvasDrawer.Graphics.Rubberband;
using CanvasDrawer.Graphics.Toolbar;
using CanvasDrawer.Graphics.Connection;
using CanvasDrawer.Graphics.Reshape;
using Microsoft.AspNetCore.Components.Web;
using CanvasDrawer.Graphics.Dragging;
using CanvasDrawer.Graphics.Editor;
using CanvasDrawer.Graphics.Selection;
using CanvasDrawer.Graphics.Feedback;
using CanvasDrawer.Graphics.Keyboard;
using CanvasDrawer.Graphics.Hover;
using CanvasDrawer.Graphics.Cloning;
using CanvasDrawer.Graphics.Popup;
using CanvasDrawer.Json;
using CanvasDrawer.Util;

namespace CanvasDrawer.Pages {
    public sealed class PageManager : IDisposable {

        public delegate void PageChange(string page);
        public PageChange? PageChanger { get; set; }

        public delegate void MapCanvasHeight(string height);
        public MapCanvasHeight? MapCanvasHeightSet { get; set; }

        //need to reset window size?
        private DoublePoint _windowSize = new DoublePoint(-1, -1);

        // delegate will be assigned to the refresher
        public delegate void PageRefresh();
        public PageRefresh? Refresher { get; set; }

        //is the drawing dirty?
        public bool IsDirty { get; set; } = true;

        //makes the primative calls to javascript
        public JSInteropManager JsManager;

        /// <summary>
        /// Reuse as the current mouse event.
        /// </summary>
        private UserEvent currentEvent = new UserEvent();

        public PageManager(JSInteropManager jsManager) {
            JsManager = jsManager;

            //wake up all managers
            GraphicsManager.Instance.PageManager = this;
            ToolbarManager.Instance.PageManager = this;
            RubberbandManager.Instance.PageManager = this;
            ReshapeManager.Instance.PageManager = this;
            ConnectionManager.Instance.PageManager = this;
            JsonManager.Instance.PageManager = this;
            //wake up other managers
            _ = SharedTimer.Instance;
            _ = DragManager.Instance;
            _ = PropertyEditor.Instance;
            _ = SelectionManager.Instance;
            _ = FeedbackManager.Instance;
            _ = JsonManager.Instance;
            _ = KeyboardManager.Instance;
            _ = DisplayManager.Instance;
            _ = DirtyManager.Instance;
            _ = PopupManager.Instance;
            _ = SubnetShapeMenu.Instance;
            _ = ConnectorMenu.Instance;
            _ = HoverManager.Instance;
        }

        //set the map canvas height pixels
        public void CanvasHeight(int height) {
            if (MapCanvasHeightSet != null) {
                MapCanvasHeightSet(height.ToString() + "px");
            }
        }

        public void WindowResized() {
            //redraw everything
            IsDirty = true;
            JsManager.WindowResized();
            GraphicsManager.Instance.FullRefresh();
        }

        /// <summary>
        /// Get the visual rect in client coodinates. It can be used to check
        /// whether something should be drawn. It is close, but approximate. It
        /// is a little bigger.
        /// </summary>
        /// <returns></returns>
        public Rect VisualRect() {
            Rect r = JsManager.CanvasScrollBoundaryRect();

            JsManager.CheckScale();

            r.X /= JsManager.Scale.X;
            r.Y /= JsManager.Scale.Y;
            r.Width /= JsManager.Scale.X;
            r.Height /= JsManager.Scale.Y;

            return r;
        }


        //external set frame resize
        public void SetMapFrameSize(int width, int height) {
            _windowSize.Set(width, height);
            JsManager.WindowResized();
            GraphicsManager.Instance.FullRefresh();
        }

        //a simple alert message
        public void Alert(string message) {
            JsManager.Alert(message);
        }

        /// <summary>
        /// Copy the map to a background image.
        /// Used for background drawing (e.g. rubberbanding)
        /// </summary>
        public void SaveCanvasInBackgoundImage() {
            JsManager.SaveCanvasInBackgoundImage();
        }

        public double GetDPI() {
            return JsManager.GetDPI();
        }

        /// <summary>
        /// Restore the map from an offscreen background image.
        /// The image is stored in the javascript.
        /// </summary>
        public void RestoreCanvasFromBackgroundImage() {
            JsManager.RestoreCanvasFromBackgroundImage();
        }

        /// <summary>
        /// Restore a rectangular area of the map from an offscreen background image.
        /// </summary>
        /// <param name="x">The left of the rectangular area.</param>
        /// <param name="y">The top of the rectangular area.</param>
        /// <param name="w">The width of the rectangular area.</param>
        /// <param name="h">The height of the rectangular area.</param>
        public void RestoreRectangularAreaFromBackgroundImage(double x, double y, double w, double h) {
            JsManager.RestoreRectangularAreaFromBackgroundImage(x, y, w, h);
        }

        //draw text on the map
        public void DrawText(double x, double y, string text, int size, string family, string color, string align) {
			JsManager.DrawText(this, x, y, text, size, family, color, align);
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
            JsManager.DrawEllipse(xc, yc, radx, rady, fillColor, borderColor, lineWidth);
        }

        //angles are in radians
        public void DrawArc(double x, double y, double rad,
            double startAngle, double endAngle, string fillColor, string borderColor,
            double lineWidth = 0, double dashLength = 0) {

            JsManager.DrawArc(x, y, rad, startAngle, endAngle, fillColor, borderColor, lineWidth, dashLength);
        }

        //Draw a rectangle
        public void DrawRect(double x, double y, double width, double height, string fillColor, string borderColor, double lineWidth = 0, double dashLength = 0) {
            JsManager.DrawRectangle(this, x, y, width, height, fillColor, borderColor, lineWidth, dashLength);
        }


        //Draw a rectangle
        public void DrawRect(Rect r, string fillColor, string borderColor,
        double lineWidth = 0, double dashLength = 0) {
            DrawRect(r.X, r.Y, r.Width, r.Height, fillColor, borderColor, lineWidth, dashLength);
        }

        /// <summary>
        /// Draw an image
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="imageId"></param>
        public void DrawImage(double x, double y, double width, double height, string imageId) {
            JsManager.DrawImage(x, y, width, height, imageId);
        }

        /// <summary>
        /// Draw a rotated image.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="angle">The angle in radians.</param>
        /// <param name="imageId"></param>
        public void DrawRotatedImage(double x, double y, double width, double height, double angle, string imageId) {
            JsManager.DrawRotatedImage(x, y, width, height, angle, imageId);
        }

        //Draw a line
        public void DrawLine(double x1, double y1, double x2, double y2, string lineColor,
            double lineWidth = 0, double dashLength = 0) {
            JsManager.DrawLine(this, x1, y1, x2, y2, lineColor, lineWidth, dashLength);
        }

        private void ChangeZoomLevel(int del) {
            JsManager.ZoomLevel += del;
            JsManager.ScaleDirty = true;
            JsManager.CheckScale();
            IsDirty = true;
            Draw();
        }

        //zoom in by a fixed amount
        public void ZoomIn() {
            ChangeZoomLevel(1);
        }

        //zoom out by a fixed amount
        public void ZoomOut() {
            ChangeZoomLevel(-1);
        }

        //Get the current scale in each direction
        public DoublePoint CurrentScale() {
            return JsManager.Scale;
        }

        //refresh, but only if dirty
        public void Draw() {
            if (IsDirty) {
                JsManager.Clear();
                GraphicsManager.Instance.DrawModel();
                IsDirty = false;
            }
        }

        public void CanvasScrolled() {
            Refresher();
            SharedTimer.Instance.PageManagerRefreshPending = true;
        }

        //Force a redraw
        public void ForceDraw() {
            IsDirty = true;
            Draw();
        }

        //get the rectangle corresponding to canvas
        //it includes what lies beyond the scroll bars
        public Rect CanvasBounds() {
            double x = JsManager.GetCanvasOffsetLeft();
            double y = JsManager.GetCanvasOffsetTop();
            double w = JsManager.CanvasWidth;
            double h = JsManager.CanvasHeight;

            DoublePoint p0 = new DoublePoint();
            DoublePoint p1 = new DoublePoint();

            ClientToLocal(x, y, p0);
            ClientToLocal(x + w, y + h, p1);

            Rect r = new Rect(p0.X, p0.Y, p1.X - p0.X, p1.Y - p0.Y);
            return r;
        }

        //convert the client coords to true local relative to corner of main canvas
        public void ClientToLocal(MouseEventArgs e) {
            double offL = JsManager.GetCanvasOffsetLeft();
            double offT = JsManager.GetCanvasOffsetTop();

            e.ClientX = (e.ClientX - offL) / JsManager.ZoomFactor();
            e.ClientY = (e.ClientY - offT) / JsManager.ZoomFactor();
        }


        public void ClientToLocal(double x, double y, DoublePoint pp) {
            double offL = JsManager.GetCanvasOffsetLeft();
            double offT = JsManager.GetCanvasOffsetTop();
            pp.X = (x - offL) / JsManager.ZoomFactor();
            pp.Y = (y - offT) / JsManager.ZoomFactor();
        }

        void IDisposable.Dispose() {
        }

        //get the text width
        public double TextWidth(string text, string fontFamily, int fontSize) {
            return JsManager.TextWidth(text, fontFamily, fontSize);
        }

        //mouse event on the canvas (map)
        public void MouseEvent(EUserEventType etype, MouseEventArgs e) {
            UserEvent ue = FromMouseEvent(etype, e);
            GraphicsManager.Instance.UserEvent(ue);
        }

        //Create a user event corresponding to a mouse event
        private UserEvent FromMouseEvent(EUserEventType etype, MouseEventArgs e) {

            double pX = e.ClientX;
            double pY = e.ClientY;

            ClientToLocal(e);
            
            currentEvent.Type = etype;

            currentEvent.X = e.ClientX;
            currentEvent.Y = e.ClientY;

            currentEvent.Xpage = pX;
            currentEvent.Ypage = pY;

            currentEvent.Meta = e.MetaKey;
            currentEvent.Shift = e.ShiftKey;
            currentEvent.Control = e.CtrlKey;
            currentEvent.Alt = e.AltKey;
            currentEvent.Button = (int)(e.Button);

            currentEvent.ResizeRectIndex = -1;

            currentEvent.SingleClick = (etype == EUserEventType.SINGLECLICK);
            currentEvent.DoubleClick = (etype == EUserEventType.DOUBLECLICK);
            return currentEvent;
        }

        //put a string into local storage
        public void LocalStoragePutString(string name, string value) {
            JsManager.LocalStoragePutString(name, value);
        }

        //retrieve a string from local storage
        public string LocalStorageGetString(string name) {
            return JsManager.LocalStorageGetString(name);
        }

        //clear everything from local storage
        public void localStorageClear() {
            JsManager.localStorageClear();
        }

        //remove an item from local storage
        public void LocalStorageRemoveString(string name) {
            JsManager.LocalStorageRemoveString(name);
        }

    }
}

