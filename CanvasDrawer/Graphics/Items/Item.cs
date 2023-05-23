using System.Collections.Generic;
using CanvasDrawer.Graphics.Feedback;
using CanvasDrawer.Graphics.Selection;
using CanvasDrawer.State;
using CanvasDrawer.DataModel;
using CanvasDrawer.Util;
using System;
using CanvasDrawer.Graphics.Theme;
using System.Drawing;

namespace CanvasDrawer.Graphics.Items {

    public class Item : IFeedbackProvider {

        //size of resize rects
        public static readonly int RESIZERECTSIZE = 12;

        //am I reshaping?
        public bool Reshaping { get; set; }

        //default fill color
        protected static readonly string DEFAULTFILLCOLOR = "rgba(85%,85%,85%,0.5)";

        //model properties
        public Properties Properties { get; set; }

        //new item defaults as selected
        public bool Selected { get; set; }

        //is the item being edited?
        public bool Editing { get; set; }

        //what layer the item is on
        public Layer Layer { get; private set; }

        //resize rects
        public Rect[] _resizeRects { get; set; }

        //cache the bounds
        protected Rect _bounds = new Rect();
        protected bool _boundsDirty = true;

        //used (temporarily) in duplication to restablish connections
        public Item TempClone { get; set; }

        /// <summary>
        /// Create an item from a properties object, probably from json.
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="properties"></param>
        public Item(Layer layer, Properties properties) {
            Layer = layer;
            Properties = properties;
            Layer.Add(this);
            FinalPreparation();
        }

        /// <summary>
        /// Create an item on a given layer with given rectangular bounds.
        /// </summary>
        /// <param name="layer">The layer the item will be drawn on.</param>
        /// <param name="bounds">The rectangular bounds.</param>
        public Item(Layer layer, Rect bounds) {
            Properties = new Properties();

            //every item has a guid
            FeedbackableOnly(DefaultKeys.GUID_KEY, Guid.NewGuid().ToString());

            //create the properties corresponding to bounds
            if (bounds != null) {
                FeedbackableOnly(DefaultKeys.LEFT, bounds.X.ToString());
                FeedbackableOnly(DefaultKeys.TOP, bounds.Y.ToString());
                FeedbackableOnly(DefaultKeys.WIDTH, bounds.Width.ToString());
                FeedbackableOnly(DefaultKeys.HEIGHT, bounds.Height.ToString());
            }

            Layer = layer;
            Layer.Add(this);
            FinalPreparation();
        }

        /// <summary>
        /// Lock (or unlock) an item.
        /// </summary>
        /// <param name="locked">the new value of the locked property.</param>
        public void SetLocked(bool locked) {
            Property prop = Properties.GetProperty(DefaultKeys.LOCKED_KEY);

            if (prop != null) {
                prop.Value = locked ? "true" : "false";
            }
        }

        /// <summary>
        /// Create anitem from a focus point (used by nodes)
        /// </summary>
        /// <param name="layer">The layer the item will be drawn on.</param>
        /// <param name="focusX">The horizontal focus position.</param>
        /// <param name="focusY">The vertical focus position.</param>
        public Item(Layer layer, double focusX, double focusY) {
            Properties = new Properties();

            FeedbackableOnly(DefaultKeys.GUID_KEY, Guid.NewGuid().ToString());
            FeedbackableOnly(DefaultKeys.LEFT, (focusX - 24).ToString());
            FeedbackableOnly(DefaultKeys.TOP, (focusY - 24).ToString());
            FeedbackableOnly(DefaultKeys.WIDTH, "48");
            FeedbackableOnly(DefaultKeys.HEIGHT, "48");

            Layer = layer;
            Layer.Add(this);
            FinalPreparation();
        }

        /// <summary>
        /// This is a convenience method to create a property that will show up in
        /// the editorand feedback, can be edited, but can never be displayed on
        /// the map under the node.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <param name="value">The property value.</param>
        /// <returns>A property with the desired display behavior.</returns>
        public Property NotDisplayable(String key, String value) {
            return Properties.CreateProperty(key, value, Property.NOTDISPLAYABLE);
        }

 
        /// <summary>
        /// Convenience method to create a feedbackable only property.
        /// It is basically hidden except in the debug feedback display
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <param name="value">The property value.</param>
        /// <returns>A property with the desired display behavior.</returns>
        public Property FeedbackableOnly(String key, String value) {
            return Properties.CreateProperty(key, value, Property.FEEDBACKABLE);
        }

        /// <summary>
        /// Convenience method to create a non editable (but displayed on map) property.
        /// It is basically hidden except in the debug feedback display
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <param name="value">The property value.</param>
        /// <returns>A property with the desired display behavior.</returns>
        public Property NotEditable(String key, String value) {
            return Properties.CreateProperty(key, value, Property.NOTEDITABLE);
        }

        

        //convenience method to create a property with all features
        //including display on map if it is a node
        public Property AllFeatures(String key, String value) {
            return Properties.CreateProperty(key, value, Property.BASIC);
        }

        //convenience method to create an invisible deprecated property
        // for backwards compatibility
        public Property Deprecated(String key, String value) {
            return Properties.CreateProperty(key, value, Property.DEPRECATED);
        }

        //convenience method to create a hidden property
        public Property Hidden(String key, String value) {
            return Properties.CreateProperty(key, value, Property.HIDDEN);
        }

        //convenience method to create a property with all features
        //except display on map 
        public Property NotDisplayedOnCanvas(String key, String value) {
            return Properties.CreateProperty(key, value, Property.NOTDISPLAYEDONCANVAS);
        }
        //convenience method to create a property
        public Property CreateProperty(String key, String value, int controlBits) {
            return Properties.CreateProperty(key, value, controlBits);
        }

        //get the guid string
        public string GuidString() {
            string guidstr = Properties.GetValue(DefaultKeys.GUID_KEY);
            return guidstr;
        }

        /// <summary>
        /// Final preparation of the item.
        /// </summary>
        private void FinalPreparation() {
            InitResizeRects();
            SelectionManager.Instance.UnselectAll();
        }

        /// <summary>
        /// Convenience method to get the name from the properties.
        /// </summary>
        /// <returns>The item's name.</returns>
        public string Name() {
            if (Properties == null) {
                return "??";
            }
            else {
                return Properties.Name();
            }
        }

        public EItemType Type() {
            string typestr = Properties.GetValue(DefaultKeys.TYPE_KEY);
            return FromString(typestr);
        }

        //update the Drawable
        public void Update(Graphics2D g) {
            Draw(g);
        }

        /// <summary>
        /// Get the text for a hover message
        /// </summary>
        /// <returns></returns>
        public virtual String GetHoverText() {
            return null;
        }

        /// <summary>
        /// Is the item visible (even partially) on screen? If
        /// not, we don't have to draw it.
        /// </summary>
        /// <returns>true if the item is visible on screen.</returns>
        public virtual bool IsVisibleOnScreen() {
            bool draw = GetTotalBounds().Intersects(GraphicsManager.Instance.VisualRect);
            return draw;
        }

        /// <summary>
        /// Is the item fully visible (not just partially) on screen?
        /// </summary>
        /// <returns>true if the item is visible on screen.</returns>
        public virtual bool IsFullyVisibleOnScreen() {
            Rect tb = new Rect(GetTotalBounds());
            tb.GrowRect(-1, -1);
            return GraphicsManager.Instance.VisualRect.Contains(tb);
        }

        /// <summary>
        /// Is the item fully visible in a horizontal sense (not just partially) on screen?
        /// </summary>
        /// <returns>true if the item is visible in a horizotal sense on screen.</returns>
        public virtual bool IsFullyHVisibleOnScreen() {
            Rect vr = GraphicsManager.Instance.VisualRect;
            Rect tb = new Rect(GetTotalBounds());
            tb.GrowRect(-1, -1);
            return (tb.X > vr.X) && (tb.Right() < vr.Right());
        }
        /// <summary>
        /// Is the item fully visible int a vertical sense (not just partially) on screen?
        /// </summary>
        /// <returns>true if the item is visible in a vertical on screen.</returns>
        public virtual bool IsFullyVVisibleOnScreen() {
            Rect vr = GraphicsManager.Instance.VisualRect;
            Rect tb = new Rect(GetTotalBounds());
            tb.GrowRect(-1, -1);
            return (tb.Y > vr.Y) && (tb.Bottom() < vr.Bottom());
        }

        /// <summary>
        /// Get the bounds that includes all displayed adornments
        /// </summary>
        /// <returns>Get the total effective rectangular bounds </returns>
        public virtual Rect GetTotalBounds() {
            return GetBounds();
        }

            //Offset when panning

            public void OffsetItem(double dx, double dy) {
            SetLeft(GetLeft() + dx);
            SetTop(GetTop() + dy);
        }

        //get the left of the bounds
        public double GetLeft() {
            Property prop = Properties.GetProperty(DefaultKeys.LEFT);

            if (prop == null) {
                return 0;
            } else {
                return Double.Parse(prop.Value);
            }
        }

        //set the left of the bounds
        public void SetLeft(double val) {
            Properties.SetProperty(DefaultKeys.LEFT, val.ToString());
            _boundsDirty = true;
        }

        //get the top of the bounds
        public double GetTop() {
            Property prop = Properties.GetProperty(DefaultKeys.TOP);
            if (prop == null) {
                return 0;
            }
            else {
                return Double.Parse(prop.Value);
            }
        }

        //set the top of the bounds
        public void SetTop(double val) {
            Properties.SetProperty(DefaultKeys.TOP, val.ToString());
            _boundsDirty = true;
        }

        //get the width of the bounds
        public double GetWidth() {
            Property prop = Properties.GetProperty(DefaultKeys.WIDTH);
            if (prop == null) {
                return 0;
            }
            else {
                return Double.Parse(prop.Value);
            }
        }

        //set the width of the bounds
        public void SetWidth(double val) {
            Properties.SetProperty(DefaultKeys.WIDTH, val.ToString());
            _boundsDirty = true;
        }

        //get the height of the bounds
        public double GetHeight() {
            Property prop = Properties.GetProperty(DefaultKeys.HEIGHT);
            if (prop == null) {
                return 0;
            }
            else {
                return Double.Parse(prop.Value);
            }
        }

        //set the height of the bounds
        public void SetHeight(double val) {
            Properties.SetProperty(DefaultKeys.HEIGHT, val.ToString());
            _boundsDirty = true;
        }

        /// <summary>
        /// Get a bounding rectangle of the item.
        /// </summary>
        /// <returns>A bounding rectangle</returns>
        public virtual Rect GetBounds() {
            if (_boundsDirty) {
                _bounds.Set(GetLeft(), GetTop(), GetWidth(), GetHeight());
                _boundsDirty = false;
            }
            return _bounds;
        }

        //set the bounds
        public void SetBounds(double left, double top, double width, double height) {
            SetLeft(left);
            SetTop(top);
            SetWidth(width);
            SetHeight(height);
            _bounds.Set(left, top, width, height);
            _boundsDirty = false;
        }

        //Set the bounds from another rect
        public void SetBounds(Rect b) {
            SetBounds(b.X, b.Y, b.Width, b.Height);
        }

        /// <summary>
        /// Set the rectangular bounds from two points
        /// </summary>
        /// <param name="p1">The first point.</param>
        /// <param name="p2">The second point.</param>
        public void SetBounds(DoublePoint p1, DoublePoint p2) {
            double left = Math.Min(p1.X, p2.X);
            double top = Math.Min(p1.Y, p2.Y);
            double width = Math.Abs(p2.X - p1.X);
            double height = Math.Abs(p2.Y - p1.Y);
            SetBounds(left, top, width, height);
        }

        public void Draw(Graphics2D g) {

            SetG2D(g);
            CustomDraw(g);

            //Annotations
            DrawAnnotations(g);

            //Draw Adornements
            DrawAdornements(g);
        }

        //draw annotations
        public virtual void DrawAnnotations(Graphics2D g) {
        }

        public virtual void DrawAdornements(Graphics2D g) {
        }

        public virtual void CustomDraw(Graphics2D g) {
        }

        //move the item (result of drag)
        public virtual void DragMove(double dx, double dy) {
        }

        /// <summary>
        /// Get the focus of the item. Base implementation is justvthe center of the bounds.
        /// </summary>
        /// <returns>The item's focus.</returns>
        public DoublePoint GetFocus() {
            return GetBounds().Center();
        }

        //draw the selection indicators
        /// <summary>
        /// Draw the resize rectangles, for subnets and connectors.
        /// Nodes do not have resize rects.
        /// </summary>
        /// <param name="g">The graphics context.</param>
        public void DrawResizeRects(Graphics2D g) {

            //speed up dragging a bit
            if (StateMachine.Instance.CurrentState == EState.Drag) {
                return;
            }

            g.LineColor = "black";
            g.FillColor = "white";

            Rect[] sr = GetResizeRects();
            if (sr != null) {
                foreach (Rect r in sr) {
                    g.DrawRect(r);
                }
            }
        }


        /// <summary>
        /// Which rect is the given point in?
        /// </summary>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        /// <returns>The index of the rect, or -1 if not in any provided rect.</returns>
        private int WhichRect(double x, double y, Rect[] rects) {

            if (rects == null) {
                return -1;
            }

            for (int i = 0; i < rects.Length; i++) {
                if (rects[i].Contains(x, y)) {
                    return i;
                }
            }

            return -1;
        }


        /// <summary>
        /// Which resize rect is the given point in?
        /// </summary>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        /// <returns>The index of the attach rect, or -1 if not in any resize rect.</returns>
        public int WhichResizeRect(double x, double y) {
            return WhichRect(x, y, _resizeRects);
        }

        /// <summary>
        /// Initialize the resize rectangles.
        /// </summary>
        protected virtual void InitResizeRects() {
        }


        /// <summary>
        /// Get the resize rects.
        /// </summary>
        /// <returns>The rects that can be grabbed for resizing.</returns>
        public virtual Rect[] GetResizeRects() {
            return _resizeRects;
        }

        //convenience method to set the graphics context based
        //on the item's style
        protected void SetG2D(Graphics2D g) {
            g.FillColor = GetBackground();
            g.LineColor = GetForeground();
            g.LineStyle = GetLineStyle();
            g.LineWidth = GetLineWidth();
        }

        /// <summary>
        /// Get the background color from the properties.
        /// </summary>
        /// <returns>The background color.</returns>
        public string GetBackground() {
            Property prop = Properties.GetProperty(DefaultKeys.BG_COLOR);
            return (prop != null) ? prop.Value : DEFAULTFILLCOLOR;
        }

        public void SetBackground(String color) {
            Properties.SetProperty(DefaultKeys.BG_COLOR, color);
        }

        /// <summary>
        /// Get the foreground color from the properties.
        /// </summary>
        /// <returns>The foreground color.</returns>
        public string GetForeground() {
            Property prop = Properties.GetProperty(DefaultKeys.FG_COLOR);
            return (prop != null) ? prop.Value : "#ffffff";
        }

        public void SetForeground(String color) {
            Properties.SetProperty(DefaultKeys.FG_COLOR, color);
        }

        public double GetLineWidth() {
            Property prop = Properties.GetProperty(DefaultKeys.LINE_WIDTH);
            return (prop != null) ? Double.Parse(prop.Value) : 1;
        }

        public void SetLineWidth(double width) {
            Properties.SetProperty(DefaultKeys.LINE_WIDTH, width.ToString());
        }

        public ELineStyle GetLineStyle() {
            Property prop = Properties.GetProperty(DefaultKeys.LINE_STYLE);
            if (prop == null) {
                return ELineStyle.SOLID;
            }
            return (ELineStyle)(Enum.Parse(typeof(ELineStyle), prop.Value, true));
        }

        public void SetLineStyle(string lineStyle) {
            ELineStyle eline = (ELineStyle)(Enum.Parse(typeof(ELineStyle), lineStyle, true));
            Properties.SetProperty(DefaultKeys.LINE_STYLE, eline.ToString());
        }

        /// <summary>
        /// Duplicate this item.
        /// </summary>
        /// <param name="dx">an offset location parameter (horizontal).</param>
        /// <param name="dy">an offset location parameter (vertical).</param>
        /// <returns>A clone, but with its own Guid.</returns>
        public virtual Item Duplicate(double dx, double dy) {
            return null;
        }

        /// <summary>
        /// Is the item locked down?
        /// </summary>
        /// <returns>true if the item is locked.</returns>
        public bool IsLocked() {
            Property prop = Properties.GetProperty(DefaultKeys.LOCKED_KEY);
            return (prop == null) ? false : prop.Value.Contains("rue");
        }


        //delete this item
        public virtual void Delete() {
            if (Layer != null) {
                Layer.Items.Remove(this);
            }
            ItemManager.Instance.NotifyObservers(new ItemEvent(this, EItemChange.DELETED));
        }

        

        //add properties to feedback
        public virtual void AddFeedback(UserEvent ue, List<string> feedbackStrings) {

            foreach (Property prop in Properties) {

                if (prop.Feedbackable()) {
                    if (DefaultKeys.GUID_KEY.Equals(prop.Key)) {
                        string gstr = prop.Value.Substring(0, 15);
                        feedbackStrings.Add("Guid: " + gstr + "...");
                    }
                   
                    else {
                        string displayStr = prop.ToString();

                        if (displayStr != null) {
                            feedbackStrings.Add(prop.ToString());
                        }
                    }
                }

            }
        }

        /// <summary>
        /// Check whether the item contains the given point.
        /// </summary>
        /// <param name="x">The X coordinate of the point.</param>
        /// <param name="y">The Y coordinate of the point.</param>
        /// <returns>true if the item contains the given point.</returns>
        public virtual bool Contains(double x, double y) {
            bool inbounds = GetBounds().Contains(x, y);
            if (inbounds) {
                return true;
            }

            //include the resize rects
            //if (WhichResizeRect(x, y) >= 0) {
            //    return true;
            //}

            return false;
        }

        /// <summary>
        /// Snap the item to the grid.
        /// </summary>
        public virtual void SnapToGrid() {
        }

        /// <summary>
        /// Called after an item was reshaped.
        /// </summary>
        public virtual void AfterReshape() {
        }


        /// <summary>
        /// Is the given item contained in this item?
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>true if the passed item is contained in this item.</returns>
        public virtual bool Contains(Item item) {
            return false;
        }

        /// <summary>
        /// Sets the resize rect index from a mouse event.
        /// </summary>
        /// <param name="ue">The mouse event.</param>
        public void SetResizeRectIndex(UserEvent ue) {
            ue.ResizeRectIndex =  WhichResizeRect(ue.X, ue.Y);
        }

        public List<string> GetConnectedRouterIPs() {
            List<string> list = new List<string>();

            List<Item> connectedItems = GetAllConnected();
            foreach (Item item in connectedItems) {
                if (item is NodeItem) {
                    NodeItem node = (NodeItem)item;
                    
                } //is node
            } //foreach

            return list;
        }

        /// <summary>
        /// Get all the items attached to this item by a connector
        /// </summary>
        /// <returns>A list of all the items attached to this item by a connector</returns>
        public List<Item> GetAllConnected() {
            List<Item> list = new List<Item>();

            List<Item> connectors = GraphicsManager.Instance.ConnectorLayer.Items;

            if (connectors != null) {
                foreach (Item item in connectors) {
                    ConnectorItem connectorItem = (ConnectorItem)item;

                    if (this == connectorItem.StartItem) {
                        list.Add(connectorItem.EndItem);
                    }
                    else if (this == connectorItem.EndItem) {
                        list.Add(connectorItem.StartItem);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Convenience method to see if this item is a connector.
        /// </summary>
        /// <returns>true if this item is a connector.
        public bool IsConnector() {
            return IsLineConnector();
        }

        /// <summary>
        /// Convenience method to see if this item is a subnet container.
        /// </summary>
        /// <returns>true if this item is a subnet.
        public bool IsSubnet() {
            return (Type() == EItemType.NodeBox);
        }

        /// <summary>
        /// Convenience method to see if this item is a line (or bolt) connector.
        /// </summary>
        /// <returns>true if this item is a line (or bolt) connector.
        public bool IsLineConnector() {
            return (Type() == EItemType.LineConnector);
        }

        /// <summary>
        /// Convenience method to see if this item is a node.
        /// </summary>
        /// <returns>true if this item is a node.
        public bool IsNode() {
            return (Type() == EItemType.Node);
        }

        /// <summary>
        /// Convenience method to see if this item is a text item.
        /// </summary>
        /// <returns>true if this item is a text item.
        public bool IsText() {
            return (Type() == EItemType.Text);
        }

        /// <summary>
        /// Obtain the item type from the name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static EItemType FromString(string name) {

            if (EItemType.Node.ToString("g").Equals(name)) {
                return EItemType.Node;
            }
            else if (EItemType.LineConnector.ToString("g").Equals(name)) {
                return EItemType.LineConnector;
            }
            else if (EItemType.NodeBox.ToString("g").Equals(name)) {
                return EItemType.NodeBox;
            }
            else if (EItemType.Text.ToString("g").Equals(name)) {
                return EItemType.Text;
            }

            //kept for backwards compatibility
            else if (EItemType.ElbowConnector.ToString("g").Equals(name)) {
                return EItemType.ElbowConnector;
            }
            return EItemType.Unknown;
        }

    }
}
