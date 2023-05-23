using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CanvasDrawer.State;
using CanvasDrawer.Util;
using CanvasDrawer.DataModel;
using CanvasDrawer.Graphics.Popup;
using CanvasDrawer.Graphics.Theme;
using System.Drawing;
using CanvasDrawer.CanvasDrawerComponents;
using System.Net.Http;

namespace CanvasDrawer.Graphics.Items {
    public sealed class SubnetItem : RectItem {

        //to maintain strict ordering of select rects
        //These are for ellipses. For rects, defined in RectItem
        private static readonly int TOP = 0;
        private static readonly int RIGHT = 1;
        private static readonly int BOTTOM = 2;
        private static readonly int LEFT = 3;

        //list of contained nodes
        private List<Item> _containedNodes;

        /// <summary>
        /// Create a subnet from properties. This is done in JSon deserialization.
        /// </summary>
        /// <param name="properties">The properties from Json.</param>
        public SubnetItem(Properties properties) : base(GraphicsManager.Instance.SubnetLayer, properties) {
            Properties = properties;

            //backwards compatibility, make sure it has a locked property
            //add a locked property if it doesn't have one
            Property prop = Properties.GetProperty(DefaultKeys.LOCKED_KEY);
            if (prop == null) {
                NotDisplayable(DefaultKeys.LOCKED_KEY, "false");
            }

            //backward compatibility for when there was only rectangles

            prop = Properties.GetProperty(DefaultKeys.SHAPE);
            if (prop == null) {
                prop = new Property(DefaultKeys.SHAPE, SubnetShapeMenu.RECTANGLE.ToString(), 0);
                Properties.Add(prop);
            }

            ItemManager.Instance.NotifyObservers(new ItemEvent(this, EItemChange.ADDED));
        }

        /// <summary>
        /// Create a subnet item
        /// </summary>
        /// <param name="layer">The layer it will be placed on.</param>
        /// <param name="bounds">The rectangular bounds.</param>
        public SubnetItem(Layer layer, Rect bounds) : base(layer, bounds) {
            CustomizeProperties();
            ItemManager.Instance.NotifyObservers(new ItemEvent(this, EItemChange.ADDED));
        }

        /// <summary>
        /// Get the shape of this subnet.
        /// </summary>
        /// <returns>The shape of the subnet.</returns>
        public int GetShape() {
            Property prop = Properties.GetProperty(DefaultKeys.SHAPE);
            if (prop == null) {
                return SubnetShapeMenu.RECTANGLE;
            }
            else {
                return Int32.Parse(prop.Value);
            }
        }

        /// <summary>
        /// Add the custom properties to the subnet item.
        /// </summary>
        public void CustomizeProperties() {
            FeedbackableOnly(DefaultKeys.TYPE_KEY, EItemType.NodeBox.ToString());
            AllFeatures(DefaultKeys.NAME_KEY, "Subnet");
            FeedbackableOnly(DefaultKeys.BG_COLOR, Item.DEFAULTFILLCOLOR);
            FeedbackableOnly(DefaultKeys.TYPE_KEY, EItemType.NodeBox.ToString());

            NotDisplayable(DefaultKeys.LOCKED_KEY, "false");

            //shape is based on current selection
            Hidden(DefaultKeys.SHAPE, SubnetShapeMenu.Instance.CurrentSelection.ToString());
        }

        

        /// <summary>
        /// Is a given host address as a uint one of the hosts in this
        /// subnet? This is not a validity checjk, just a contained check.
        /// It is used to assign the next availible valid address.
        /// </summary>
        /// <param name="hostId"></param>
        /// <returns></returns>
        public bool ContainsHost(uint hostId) {
            return false;
        }

    

        /// <summary>
        /// The name display string for this subnet. It combines the name
        /// and the IP address
        /// </summary>
        /// <returns>The display string for this subnet.</returns>
        public string NameDisplayString() {

            if (Properties == null) {
                return null;
            }

            string displayStr = "";
            Property nameProp = Properties.GetProperty(DefaultKeys.NAME_KEY);
            if ((nameProp != null) && nameProp.DisplayedOnCanvas) {
                String name = nameProp.Value;
                if (name != null) {
                    displayStr = String.Copy(name);
                }
            }

            
            return displayStr;
        }

        /// <summary>
        /// Sraw any annotations
        /// </summary>
        /// <param name="g">The graphics context.</param>
        public override void DrawAnnotations(Graphics2D g) {

            string displayStr = NameDisplayString();
            if (displayStr == null) {
                return;
            }

            
            //theme based text color
            g.TextColor = ThemeManager.NodeTextColor;
            

            //draw centered
            g.FontAlign = "center";
            double xc = GetBounds().Xc();

            //name above
            Graphics2D gsave = g.Save();
            gsave.FontSize = 12;
            double fheight = 1.2 * gsave.FontSize;
            int fontslop = 3;

            double y = GetBounds().Y - fontslop - 1;
            gsave.DrawText(xc, y, displayStr);

            if (IsLocked()) {
                double sw = GraphicsManager.Instance.PageManager.TextWidth(displayStr, gsave.FontFamily, gsave.FontSize);

                if (ThemeManager.IsLight) {
                    g.DrawImage(xc - sw / 2 - 13, y - fheight +4, 12, 12, "black_lock");
                }
                else {
                    g.DrawImage(xc - sw / 2 - 13, y - fheight + 4, 12, 12, "white_lock");
                }

            }


            //properties below
            AddPropertiesBelow(g, xc);
        }

        /// <summary>
        /// Custom drawing for the subnet item.
        /// </summary>
        /// <param name="g">The graphics context.</param>
        public override void CustomDraw(Graphics2D g) {
            g.FillColor = ThemeManager.RectItemFillColor;
            g.LineColor = ThemeManager.RectItemLineColor;
            Rect b = GetBounds();

            if (GetShape() == SubnetShapeMenu.RECTANGLE) {
                g.DrawRect(b);
            }
            else if (GetShape() == SubnetShapeMenu.ELLIPSE) {
                g.DrawEllipse(b.Xc(), b.Yc(), b.Width/2, b.Height/2);
            }
            else if (GetShape() == SubnetShapeMenu.CLOUD) {
                g.DrawImage(b.X, b.Y, b.Width, b.Height, "cloud_Net");
            }
           

            if (StateMachine.Instance.CurrentState == EState.Idle) {
                _containedNodes = GetAllContainedNodes();
            }
        }


        /// <summary>
        /// There are 8 possible connection points.
        /// </summary>
        /// <param name="index">The index, </param>
        /// <param name="dp"></param>
        public override void GetConnectPoint(int index, DoublePoint dp) {

            Rect b = GetBounds();

            if (GetShape() == SubnetShapeMenu.ELLIPSE) {
                double theta = index * Math.PI / 4;
                double xc = b.Xc();
                double yc = b.Yc();
                double rx = b.Width / 2;
                double ry = b.Height / 2;

                dp.X = xc + rx * Math.Cos(theta);
                dp.Y = yc + ry * Math.Sin(theta);

            }
            else if (GetShape() == SubnetShapeMenu.CLOUD) {
                double w = b.Width;
                double h = b.Height;

                switch (index) {
                    case 0:
                        dp.Set(b.X + w/10, b.Y+h/10);
                        break;

                    case 1:
                        dp.Set(b.Xc(), b.Y+h/30);
                        break;

                    case 2:
                        dp.Set(b.Right()-w/9, b.Y+h/10);
                        break;

                    case 3:
                        dp.Set(b.Right()-w/30, b.Yc());
                        break;

                    case 4:
                        dp.Set(b.Right()-w/15, b.Bottom()-h/10);
                        break;

                    case 5:
                        dp.Set(b.Xc(), b.Bottom()-h/20);
                        break;

                    case 6:
                        dp.Set(b.X+w/15, b.Bottom()-h/15);
                        break;

                    case 7:
                        dp.Set(b.X+w/40, b.Yc());
                        break;
                }
            }
            else {
                switch (index) {
                    case 0:
                        dp.Set(b.X, b.Y);
                        break;

                    case 1:
                        dp.Set(b.Xc(), b.Y);
                        break;

                    case 2:
                        dp.Set(b.Right(), b.Y);
                        break;

                    case 3:
                        dp.Set(b.Right(), b.Yc());
                        break;

                    case 4:
                        dp.Set(b.Right(), b.Bottom());
                        break;

                    case 5:
                        dp.Set(b.Xc(), b.Bottom());
                        break;

                    case 6:
                        dp.Set(b.X, b.Bottom());
                        break;

                    case 7:
                        dp.Set(b.X, b.Yc());
                        break;
                }


            }

        }

        /// <summary>
        /// Find the connection point that is closest to the given point.
        /// </summary>
        /// <param name="p">The given point</param>
        /// <param name="connectionPoint">Set as the nearest connection point.</param>
        public override void ConnectionPoint(DoublePoint p, DoublePoint connectionPoint) {
            double mindistance = Double.PositiveInfinity;

            DoublePoint dp = new DoublePoint();

            for (int index = 0; index < 8; index++) {
                GetConnectPoint(index, dp);
                double distance = p.DistanceSq(dp);

                if (distance < mindistance) {
                    connectionPoint.Set(dp);
                    mindistance = distance;
                }
            }
        }

        /// <summary>
        /// Check whether the item contains the given point. Exact
        /// for RECTANGLE and ELLIPSE. Approximate for CLOUD.
        /// </summary>
        /// <param name="x">The X coordinate of the point.</param>
        /// <param name="y">The Y coordinate of the point.</param>
        /// <returns>true if the item contains the given point</returns>
        public override bool Contains(double x, double y) {
 
            if (GetShape() == SubnetShapeMenu.ELLIPSE) {
                //include the resize rects
                //if (WhichResizeRect(x, y) >= 0) {
                //    return true;
                //}

                Rect bounds = GetBounds();
                double a = bounds.Width / 2;
                double b = bounds.Height / 2;
                double xc = bounds.Xc();
                double yc = bounds.Yc();
                double xx = (x - xc) / a;
                double yy = (y - yc) / b;
                return xx * xx + yy * yy < 1;
            }
            return base.Contains(x, y);
        }

        /// <summary>
        /// Move as the result of a drag.
        /// </summary>
        /// <param name="dx">The horizontal shift</param>
        /// <param name="dy">The vertical shift</param>
        public override void DragMove(double dx, double dy) {
            base.DragMove(dx, dy);
            foreach (Item node in _containedNodes) {
                //drag unselected contained nodes along
                //the selected ones will come along for free
                if (!node.Selected) {
                    node.DragMove(dx, dy);
                }
            }
        }

        public override void Delete() {
            Layer nodeLayer = GraphicsManager.Instance.NodeLayer;
            foreach (Item item in nodeLayer.Items) {
                NodeItem node = (NodeItem)item;
                if (node.Subnet == this) {
                    node.Subnet = null;
                }
            }

            base.Delete();
        }

		/// <summary>
        /// Initilize the resizing rectangles.
        /// </summary>
        protected override void InitResizeRects() {
            _resizeRects = new Rect[4];

            for (int i = 0; i < _resizeRects.Length; i++) {
                _resizeRects[i] = new Rect();
            }
        }

        /// <summary>
        /// Get the resize rects based on the shape.
        /// </summary>
        /// <returns>The four select rects</returns>
        public override Rect[] GetResizeRects() {
           if (GetShape() == SubnetShapeMenu.ELLIPSE) {
                DoublePoint scale = GraphicsManager.Instance.CurrentScale();
                double w = RESIZERECTSIZE / scale.X;
                double h = RESIZERECTSIZE / scale.Y;

                Rect bounds = GetBounds();
                double l = bounds.X;
                double t = bounds.Y;
                double r = bounds.Right();
                double b = bounds.Bottom();
                double xc = bounds.Xc() - w/2;
                double yc = bounds.Yc() - h/2;

                _resizeRects[TOP].Set(xc, t, w, h);
                _resizeRects[RIGHT].Set(r-w, yc, w, h);
                _resizeRects[BOTTOM].Set(xc, b-h, w, h);
                _resizeRects[LEFT].Set(l, yc, w, h);

                return _resizeRects;
            }
 
           //for rects and clouds, use base implementation
            return base.GetResizeRects();
        }

            /// <summary>
            /// Does this subnet item fully contain a node.
            /// Exact for rectangles and ellipses, approximate for clouds.
            /// </summary>
            /// <param name="node">The node to check</param>
            /// <returns></returns>
            public bool Contains(NodeItem node) {
            if (GetShape() == SubnetShapeMenu.ELLIPSE) {
                Rect nb = node.GetBounds();
                double l = nb.X+1;
                double t = nb.Y+1;
                double r = nb.Right()-1;
                double b = nb.Bottom()-1;
                return Contains(l, t) && Contains(r, t) && Contains(r, b) && Contains(l, b);
            }
           
            return GetBounds().Contains(node.GetBounds());
        }

        /// <summary>
        /// Get all the contained nodes.
        /// </summary>
        /// <returns>All the nodes in this subnet.</returns>
        public List<Item> GetAllContainedNodes() {
            List<Item> nodes = new List<Item>();

            Layer nodeLayer = GraphicsManager.Instance.NodeLayer;

            foreach (Item item in nodeLayer.Items) {

                NodeItem node = (NodeItem)item;

                if (Contains(node)) {
                    //add it if not in another box
                    if ((node.Subnet == null) || (node.Subnet == this)) {
                        nodes.Add(node);
                        node.Subnet = this;
                    }
                }
                else {
                    // if not contained and thinks it is, fix
                    if (node.Subnet == this) {
                        node.Subnet = null;
                    }
                }
            }

            return nodes;
        }

        /// <summary>
        /// Duplicate this item.
        /// </summary>
        /// <param name="dx">an offset location parameter (horizontal).</param>
        /// <param name="dy">an offset location parameter (vertical).</param>
        /// <returns>A clone, but with its own Guid.</returns>
        public override Item Duplicate(double dx, double dy) {

            SubnetItem copy = new SubnetItem(Layer, GetBounds());

            //used only to establish connections after duplication
            TempClone = copy;

            //have to copy the properties (but use the new guid)
            copy.Properties = new Properties(Properties);
            copy.Properties.GetProperty(DefaultKeys.GUID_KEY).Value = Guid.NewGuid().ToString();
           
            copy.OffsetItem(dx, dy);
            copy.SetLocked(false);

            //now copy any contained items regardless of whether they were selected
            if (_containedNodes != null) {
                copy._containedNodes = new List<Item>();
                foreach (Item conItem in _containedNodes) {
                    if (conItem.IsNode()) {
                        NodeItem node = (NodeItem)conItem;
                        NodeItem dupNode = (NodeItem)(node.Duplicate(dx, dy));
                        copy._containedNodes.Add(dupNode);
                        dupNode.Subnet = copy;
                    }
                }
            }

            copy.Selected = false;
            return copy;
        }

        /// <summary>
        /// Snap the item to the drawing grid.
        /// </summary>
        public override void SnapToGrid() {
            Rect bounds = GetBounds();
            double left = GraphicsManager.Instance.GridValue(bounds.X);
            double top = GraphicsManager.Instance.GridValue(bounds.Y);
            double right = GraphicsManager.Instance.GridValue(bounds.Right());
            double bottom = GraphicsManager.Instance.GridValue(bounds.Bottom());
            double width = right - left;
            double height = bottom - top;
            SetBounds(left, top, width, height);
        }


    }
}
