using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CanvasDrawer.DataModel;
using CanvasDrawer.Graphics.Theme;

namespace CanvasDrawer.Graphics.Items {


    public abstract class ConnectorItem : Item {

        //connection points
        public DoublePoint Start { get; protected set; } = new DoublePoint();
        public DoublePoint End { get; protected set; } = new DoublePoint();

        //for proximity test
        protected static readonly double PROXIMATE = 25;

        //connection point drawing
        protected static readonly int CONNECTSIZE = 8;

        public Item? StartItem { get; set; }
        public Item? EndItem { get; set; }

        public ConnectorItem(Layer layer, Item startItem, 
            Item endItem) : base(layer, new Rect(0, 0, 0, 0)) {
            StartItem = startItem;
            EndItem = endItem;
        }

        //Create a Connector item from properties 
        public ConnectorItem(Layer layer, Properties properties) : base(layer, properties) {
        }

        /// <summary>
        /// Set the cusom properties for the connector.
        /// </summary>
        public virtual void CustomizeProperties() {
            FeedbackableOnly(DefaultKeys.STARTGUID, (StartItem == null) ? "???" : StartItem.GuidString());
            FeedbackableOnly(DefaultKeys.ENDGUID, (EndItem == null) ? "???": EndItem.GuidString());
            FeedbackableOnly(DefaultKeys.LINE_STYLE, ELineStyle.SOLID.ToString());
            FeedbackableOnly(DefaultKeys.LINE_WIDTH, "1");
            FeedbackableOnly(DefaultKeys.FG_COLOR, ThemeManager.DefaultConnectorLineColor);
            FeedbackableOnly(DefaultKeys.SELECT_COLOR, ThemeManager.ConnectorSelectColor);
        }

        /// <summary>
        /// Get the color for a selected connector.
        /// </summary>
        /// <returns></returns>
        public string GetSelectedColor() {
            Property prop = Properties.GetProperty(DefaultKeys.SELECT_COLOR);
            return ((prop != null) && (prop.Value != null)) ? prop.Value : ThemeManager.ConnectorSelectColor;
        }

        /// <summary>
        /// Initialzie the two resize rects.
        /// </summary>
        protected override void InitResizeRects() {
            _resizeRects = new Rect[2];

            for (int i = 0; i < _resizeRects.Length; i++) {
                _resizeRects[i] = new Rect();
            }
        }
        
        /// <summary>
        /// Get the item on the Connection that is farther to a given location.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <returns>The farther item, either the StartItem or the EndItem.</returns>
        public Item? FartherItem(double x, double y) {
            double delx1 = Start.X - x;
            double dely1 = Start.Y - y;

            double delx2 = End.X - x;
            double dely2 = End.Y - y;

            double rsq1 = delx1 * delx1 + dely1 * dely1;
            double rsq2 = delx2 * delx2 + dely2 * dely2;

            return (rsq1 > rsq2) ? StartItem : EndItem;
        }



        //Is the given item one of the connection endpoints?
            /// <summary>
            /// Is the given item one of the connection endpoints?
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
        public override bool Contains(Item item) {
            return (StartItem == item) || (EndItem == item);
        }

        /// <summary>
        /// Compute the distance to the connector.
        /// </summary>
        /// <param name="x">The X coordinate of the point.</param>
        /// <param name="y">The Y coordinate of the point.</param>
        /// <returns>The Euclidian distance.</returns>
        public virtual double DistanceTo(double x, double y) {
            return Double.NaN;
        }

        /// <summary>
        /// Check whether the connector "contains" the given point.
        /// </summary>
        /// <param name="x">The X coordinate of the point.</param>
        /// <param name="y">The Y coordinate of the point.</param>
        /// <returns>true if the item contains the given point.</returns>
        public override bool Contains(double x, double y) {

            if (Selected) {
                foreach (Rect rr in _resizeRects) {
                    if (rr.Contains(x, y)) {
                        return true;
                    }
                }
            }


            bool closeEnough = DistanceTo(x, y) < PROXIMATE;
            if (closeEnough) {
                return true;
            }

            if (Selected) {
                Rect[] resizeRects = GetResizeRects();
                if (resizeRects != null) {
                    foreach (Rect r in resizeRects) {
                        if (r.Contains(x, y)) {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Check if either endpoint is a subnet.
        /// </summary>
        /// <returns></returns>
        public Boolean ConnectedToSubnet() {
            return (StartItem != null) && StartItem.IsSubnet() && (EndItem != null) && EndItem.IsSubnet();
        }

        /// <summary>
        /// See if the connector has a start and end point item.
        /// Used to reconnect.
        /// </summary>
        /// <returns>true if the connector is full connected.</returns>
        public bool IsFullyConnected() {
            return (StartItem != null) && (EndItem != null);
        }

        /// <summary>
        /// Add to the feedback strings.
        /// </summary>
        /// <param name="ue">The mouse event.</param>
        /// <param name="feedbackStrings">The feedback string collection to add to.</param>
        public override void AddFeedback(UserEvent ue, List<string> feedbackStrings) {

            if (!IsFullyConnected()) {
                return;
            }

            if (Contains(ue.X, ue.Y)) {
                base.AddFeedback(ue, feedbackStrings);
                feedbackStrings.Add("Distance " + DistanceTo(ue.X, ue.Y));

                string name1 = (StartItem == null) ? "???" : StartItem.Name();
                string name2 = (EndItem == null) ? "???" : EndItem.Name();
                feedbackStrings.Add("connects " + name1 + " to " +name2);

            }
        }

    }
}
