using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CanvasDrawer.DataModel;
using CanvasDrawer.Util;
using CanvasDrawer.Graphics.Theme;
using CanvasDrawer.Graphics.Editor;
using CanvasDrawer.Pages;

namespace CanvasDrawer.Graphics.Items {
    public abstract class RectItem : Item {

        //to maintain strict ordering of select rects
        protected static readonly int TOPLEFT = 0;
        protected static readonly int TOPRIGHT = 1;
        protected static readonly int BOTTOMRIGHT = 2;
        protected static readonly int BOTTOMLEFT = 3;

        /// <summary>
        /// Create a rectangle item
        /// </summary>
        /// <param name="layer">The layer it will be drawn on.</param>
        /// <param name="bounds">The rectangular bounds.</param>
        public RectItem(Layer layer, Rect bounds) : base(layer, bounds) {
        }

        //create a rect item from a focus used by nodes
        public RectItem(Layer layer, double xc, double yc) :
            base(layer, xc, yc) {
        }

        /// <summary>
        /// Create a rectangle item from properties
        /// </summary>
        /// <param name="layer">The layer the item will licve on.</param>
        /// <param name="properties">The Properties object, probably from a Json deserialization.</param>
        public RectItem(Layer layer, Properties properties) : base(layer, properties) {
        }

        /// <summary>
        /// Initialize the resize rectangles for objects that are resizable.
        /// </summary>
        protected override void InitResizeRects() {
            _resizeRects = null;
        }


        /// <summary>
        /// Custom drawing for the RectItem.
        /// </summary>
        /// <param name="g">The graphics context.</param>
        public override void CustomDraw(Graphics2D g) {
            g.FillColor = ThemeManager.RectItemFillColor;
            g.LineColor = ThemeManager.RectItemLineColor;
            g.DrawRect(GetBounds());
        }

        /// <summary>
        /// Move the item as the result of a drag.
        /// </summary>
        /// <param name="dx">The horizontal movement.</param>
        /// <param name="dy">The vertical movement.</param>
        public override void DragMove(double dx, double dy) {
            OffsetItem(dx, dy);
        }

        /// <summary>
        /// Get the selection rectangles for resizing, if appropriate
        /// </summary>
        /// <returns>the selection rectangles</returns>
        public override Rect[] GetResizeRects() {

            if (_resizeRects != null) {
                DoublePoint scale = GraphicsManager.Instance.CurrentScale();
                double w = RESIZERECTSIZE / scale.X;
                double h = RESIZERECTSIZE / scale.Y;

                Rect bounds = GetBounds();
                double l = bounds.X;
                double t = bounds.Y;
                double r = bounds.Right();
                double b = bounds.Bottom();

                _resizeRects[TOPLEFT].Set(l, t, w, h);
                _resizeRects[TOPRIGHT].Set(r-w, t, w, h);
                _resizeRects[BOTTOMRIGHT].Set(r-w, b-h, w, h);
                _resizeRects[BOTTOMLEFT].Set(l, b-h, w, h);
            }
            return _resizeRects;
        }

        /// <summary>
        /// Add strings to the mouse-over feedback list.This is the list displayed
        /// with the CTRL-SHIFT-F keyboard shortcut.
        /// </summary>
        /// <param name="ue">The mouse event.</param>
        /// <param name="feedbackStrings"></param>
        public override void AddFeedback(UserEvent ue, List<string> feedbackStrings) {
            Rect b = GetBounds();
            feedbackStrings.Add(String.Format("Bounds = [{0:0.#}, {1:0.#},  {2:0.#},  {3:0.#}]", b.X, b.Y, b.Width, b.Height));
            if (Contains(ue.X, ue.Y)) {
                base.AddFeedback(ue, feedbackStrings);
            }
        }

        /// <summary>
        /// Draw annotations on the item.
        /// </summary>
        /// <param name="g">The graphics context.</param>
        public override void DrawAnnotations(Graphics2D g) {

            //theme based text color

            if (Editing && DisplayManager.Instance.IsPropertyEditorVisible()) {
                g.TextColor = ThemeManager.HotItemTextColor;
            }
            else {
                g.TextColor = ThemeManager.NodeTextColor;
            }

            //draw centered
            g.FontAlign = "center";
            double xc = GetBounds().Xc();

            //properties below
            AddPropertiesBelow(g, xc);
        }

        //shorten certain strings
        protected string Abbreviate(String s) {

            //there are some acceptable appreviations

            int len = (s == null) ? 0 : s.Length;
            if (len < 11) {
                return s;
            }

            s = s.Replace("Windows Server", "WinSrv");
            s = s.Replace("Windows 10", "Win10");

            return s;
        }

        /// <summary>
        /// Truncate a display string to help with decluttering.
        /// </summary>
        /// <param name="s">The string to truncates</param>
        /// <param name="extraCut">If true, possibly truncate further and add "..."</param>
        /// <returns>The truncated string, possibly with a terminating "...".</returns>
        protected string TruncateString(string s, bool extraCut) {

            int len = (s == null) ? 0 : s.Length;
            if (len <= ThemeManager.MaxDisplayChars) {
                return s;
            }

            if (extraCut) {
                return s.Substring(0, ThemeManager.MaxDisplayChars - 3) + "...";
            }
            else {
                return s.Substring(0, ThemeManager.MaxDisplayChars);
            }

        }


        /// <summary>
        /// Write out some properties below the icon.
        /// </summary>
        /// <param name="g">The graphics context.</param>
        /// <param name="xc">The horizontal center of the item.</param>
        protected void AddPropertiesBelow(Graphics2D g, double xc) {

            double fheight = 1.2 * g.FontSize;
            int fontslop = 1;

            Rect bounds = GetBounds();

            double y = bounds.Bottom() + fheight + fontslop;

            //name always first (subnets handle name differently)
            if (!IsSubnet()) {
                Property nameProp = Properties.GetProperty(DefaultKeys.NAME_KEY);

                Console.WriteLine("NAME: ");
				Console.WriteLine("NAME: [" + nameProp.Value + "]");
                Console.WriteLine("DISPLAYED: " + nameProp.DisplayedOnCanvas);

				if ((nameProp != null) && nameProp.DisplayedOnCanvas && (nameProp.Value != null)) {
                    String s = TruncateString(nameProp.Value, true);
                    g.DrawText(xc, y, s);

                    if (IsLocked() && (JSInteropManager.Instance != null)) {
                        double sw = JSInteropManager.Instance.TextWidth(s, g.FontFamily, g.FontSize);

                        if (ThemeManager.IsLight) {
                            g.DrawImage(xc - sw/2 - 13, y-fheight+1, 12, 12, "black_lock");
                        }
                        else {
                            g.DrawImage(xc - sw / 2 - 13, y-fheight+1, 12, 12, "white_lock");
                        }

                    }

                    y += fheight + fontslop;
                }
            }

            if ((JSInteropManager.Instance != null) && (JSInteropManager.Instance.ZoomLevel > -3) && (Properties != null)) {
                foreach (Property prop in Properties) {
                    if (prop.DisplayedOnCanvas && !prop.IsName()) {


                        String displayStr = prop.Value;

                        if (displayStr == null) {
                            continue;
                        }

                        displayStr = displayStr.Trim();
                        if (displayStr.Length < 1) {
                            continue;
                        }

                        g.DrawText(xc, y, displayStr);
                        y += fheight + fontslop;
                    }  //displayed on map, but not name
                }
            } // not zoomed in too much
        }

        /// <summary>
        /// Draw any adornements
        /// </summary>
        /// <param name="g">The graphics context.</param>
        public override void DrawAdornements(Graphics2D g) {

            bool editing = Editing && DisplayManager.Instance.IsPropertyEditorVisible();
            if (Selected || editing) {
                Graphics2D gsave = g.Save();

                gsave.LineColor = editing ? ThemeManager.HotItemBorderColor : ThemeManager.SelectedItemBorderColor;
                gsave.LineWidth = 2;
                gsave.LineStyle = ELineStyle.SOLID;
                gsave.FillColor = null;
                gsave.DrawRect(GetBounds());
            }
        }


        /// <summary>
        /// There are 8 possible connection points.
        /// </summary>
        /// <param name="index">The index, </param>
        /// <param name="dp"></param>
        public virtual void GetConnectPoint(int index, DoublePoint dp) {
        }

        /// <summary>
        /// Find the connection point that is closest to the given point.
        /// </summary>
        /// <param name="p">The given point</param>
        /// <param name="connectionPoint">Set as the nearest connection point.</param>
        public virtual void ConnectionPoint(DoublePoint p, DoublePoint connectionPoint) {
        }

    }
}
