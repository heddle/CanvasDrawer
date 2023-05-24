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

        /// <summary>
        /// Reuse as the current mouse event.
        /// </summary>
        private UserEvent currentEvent = new UserEvent();

        public PageManager() {

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

			JSInteropManager? jsm = JSInteropManager.Instance;
			if (jsm == null) {
				return;
			}
			IsDirty = true;
            jsm.WindowResized();
            GraphicsManager.Instance.FullRefresh();
        }

        /// <summary>
        /// Get the visual rect in client coodinates. It can be used to check
        /// whether something should be drawn. It is close, but approximate. It
        /// is a little bigger.
        /// </summary>
        /// <returns></returns>
        public Rect? VisualRect() {

            JSInteropManager? jsm = JSInteropManager.Instance;
            if ((jsm == null) || (jsm.Scale == null)) {
                return null;
            }

            Rect r = jsm.CanvasScrollBoundaryRect();

            jsm.CheckScale();

            r.X /= jsm.Scale.X;
            r.Y /= jsm.Scale.Y;
            r.Width /= jsm.Scale.X;
            r.Height /= jsm.Scale.Y;

            return r;
        }


        //external set frame resize
        public void SetMapFrameSize(int width, int height) {
            _windowSize.Set(width, height);

            if (JSInteropManager.Instance != null) {
				JSInteropManager.Instance.WindowResized();
                GraphicsManager.Instance.FullRefresh();
            }
        }

        
       
        private void ChangeZoomLevel(int del) {
			JSInteropManager? jsm = JSInteropManager.Instance;
			if (jsm == null) {
				return;
			}
			jsm.ZoomLevel += del;
            jsm.ScaleDirty = true;
            jsm.CheckScale();
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

        //refresh, but only if dirty
        public void Draw() {
            if (IsDirty && (JSInteropManager.Instance != null)) {
                JSInteropManager.Instance.Clear();
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
        public Rect? CanvasBounds() {
			JSInteropManager? jsm = JSInteropManager.Instance;
			if (jsm == null) {
				return null;
			}
			double x = jsm.GetCanvasOffsetLeft();
            double y = jsm.GetCanvasOffsetTop();
            double w = jsm.CanvasWidth;
            double h = jsm.CanvasHeight;

            DoublePoint p0 = new DoublePoint();
            DoublePoint p1 = new DoublePoint();

            ClientToLocal(x, y, p0);
            ClientToLocal(x + w, y + h, p1);

            Rect r = new Rect(p0.X, p0.Y, p1.X - p0.X, p1.Y - p0.Y);
            return r;
        }

        //convert the client coords to true local relative to corner of main canvas
        public void ClientToLocal(MouseEventArgs e) {

            JSInteropManager? jsm = JSInteropManager.Instance;
            if (jsm == null) {
                return;
            }
            double offL = jsm.GetCanvasOffsetLeft();
            double offT = jsm.GetCanvasOffsetTop();

            e.ClientX = (e.ClientX - offL) / jsm.ZoomFactor();
            e.ClientY = (e.ClientY - offT) / jsm.ZoomFactor();
        }


        public void ClientToLocal(double x, double y, DoublePoint pp) {
			JSInteropManager? jsm = JSInteropManager.Instance;
			if (jsm == null) {
				return;
			}
			double offL = jsm.GetCanvasOffsetLeft();
            double offT = jsm.GetCanvasOffsetTop();
            pp.X = (x - offL) / jsm.ZoomFactor();
            pp.Y = (y - offT) / jsm.ZoomFactor();
        }

        void IDisposable.Dispose() {
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

       

    }
}

