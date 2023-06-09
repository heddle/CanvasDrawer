﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CanvasDrawer.Util;
using CanvasDrawer.DataModel;
using CanvasDrawer.Graphics.Popup;
using System.ComponentModel.DataAnnotations;

namespace CanvasDrawer.Graphics.Items {
    public sealed class LineConnectorItem : ConnectorItem {

        /// <summary>
        /// Create a line connector.
        /// </summary>
        /// <param name="layer">The layer it will live on.</param>
        /// <param name="startItem">The start item.</param>
        /// <param name="endItem">The end Item</param>
        public LineConnectorItem(Layer layer, Item startItem, Item endItem) :
            base(layer, startItem, endItem) {
            CustomizeProperties();
            ItemManager.Instance.NotifyObservers(new ItemEvent(this, EItemChange.ADDED));
        }

        /// <summary>
        /// Create a line connector from a Properties object. Probably Json deserialization.
        /// </summary>
        /// <param name="properties">the Properties object.</param>
        public LineConnectorItem(Properties properties) : base(GraphicsManager.Instance.ConnectorLayer, properties) {
            string startGuidStr = properties.GetValue(DefaultKeys.STARTGUID);
            string endGuidStr = properties.GetValue(DefaultKeys.ENDGUID);

            Properties = properties;

            //backward compatibility for when there was no lightening bolts

            Property prop = Properties.GetProperty(DefaultKeys.CNXTYPE);
            if (prop == null) {
                prop = new Property(DefaultKeys.CNXTYPE, ConnectorMenu.LINECNX.ToString(), 0);
                Properties.Add(prop);
            }


            StartItem = ItemManager.Instance.FromGuid(startGuidStr);
            EndItem = ItemManager.Instance.FromGuid(endGuidStr);

            ItemManager.Instance.NotifyObservers(new ItemEvent(this, EItemChange.ADDED));
        }

        /// <summary>
        /// Add the custom the properties.
        /// </summary>
        public override void CustomizeProperties() {
            base.CustomizeProperties();
            FeedbackableOnly(DefaultKeys.TYPE_KEY, EItemType.LineConnector.ToString());
            AllFeatures(DefaultKeys.NAME_KEY, EItemType.LineConnector.ToString());

            //type is based on current selection
            Hidden(DefaultKeys.CNXTYPE, ConnectorMenu.Instance.CurrentSelection.ToString());
        }

        /// <summary>
        /// Draw the line connector.
        /// </summary>
        /// <param name="g">The graphics context.</param>
        public override void CustomDraw(Graphics2D g) {

            //for subnets there are 8 possible connection points
            if (StartItem.IsSubnet()) {
                ((RectItem)StartItem).ConnectionPoint(EndItem.GetFocus(), Start);
            }
            else { //for nodes, connect to focus (center)
                Start.Set(StartItem.GetFocus());
            }

            if (EndItem.IsSubnet()) {
                ((RectItem)EndItem).ConnectionPoint(Start, End);
            }
            else {
                End.Set(EndItem.GetFocus());
            }

            Graphics2D gsave = g.Save();
            gsave.LineWidth = GetLineWidth();
            gsave.LineColor = GetForeground();

            if (Selected) {
                gsave.LineStyle = ELineStyle.DASHED;
            }
            else {
                gsave.LineStyle = GetLineStyle();

            }

            SetBounds(Start, End);

            if (GetConnectorType() == ConnectorMenu.LINECNX) {
                gsave.DrawLine(Start.X, Start.Y, End.X, End.Y);
            }
            else {
                // lightening bolt
                double delX = End.X - Start.X;
                double delY = End.Y - Start.Y;
                double len = Math.Sqrt(delX * delX + delY * delY);
                len = Math.Max(60, len - 48);
                double angle = Math.Atan2(delY, delX);
                double xc = (End.X + Start.X) / 2;
                double yc = (End.Y + Start.Y) / 2;
                double width = len;
                double height = Math.Min(20, 10 * (len / 60));
                 if (len < 280) {
                    gsave.DrawRotatedImage(xc, yc, width, height, angle, "boltSmall");
                }
                else {
                    gsave.DrawRotatedImage(xc, yc, width, height, angle, "boltLarge");
                }
            }
        }

        /// <summary>
        /// Get a bounding rectangle of the item.
        /// </summary>
        /// <returns>A bounding rectangle</returns>
        public override Rect GetBounds() {
            //for subnets there are 8 possible connection points
            if (StartItem.IsSubnet()) {
                ((RectItem)StartItem).ConnectionPoint(EndItem.GetFocus(), Start);
            }
            else { //for nodes, connect to focus (center)
                Start.Set(StartItem.GetFocus());
            }

            if (EndItem.IsSubnet()) {
                ((RectItem)EndItem).ConnectionPoint(Start, End);
            }
            else {
                End.Set(EndItem.GetFocus());
            }
            SetBounds(Start, End);
            return _bounds;
        }

        /// <summary>
        /// Get the shape of this line connector.
        /// </summary>
        /// <returns>The shape of the line connector.</returns>
        public int GetConnectorType() {
            Property prop = Properties.GetProperty(DefaultKeys.CNXTYPE);
            if (prop == null) {
                return ConnectorMenu.LINECNX;
            }
            else {
                return Int32.Parse(prop.Value);
            }
        }

        /// <summary>
        /// Get the resize rects.
        /// </summary>
        /// <returns>The rects used for connecting and reconnecting.</returns>
        public override Rect[] GetResizeRects() {

            DoublePoint scale = GraphicsManager.Instance.CurrentScale();
            double w = RESIZERECTSIZE / scale.X;
            double h = RESIZERECTSIZE / scale.Y;

            double delX = End.X - Start.X;
            double delY = End.Y - Start.Y;



            _resizeRects[0].Set(Start.X + 0.1 * delX - w / 2, Start.Y + 0.1 * delY - h / 2, w, h);
            _resizeRects[1].Set(Start.X + 0.9 * delX - w / 2, Start.Y + 0.9 * delY - h / 2, w, h);
            return _resizeRects;
        }

        /// <summary>
        /// Get the distance to a point for proximity test
        /// </summary>
        /// <param name="x">The horizontal coordinate of the point.</param>
        /// <param name="y">The vertical coordinate of the point.</param>
        /// <returns>The perpendicular distance.</returns>
        public override double DistanceTo(double x, double y) {
            DoublePoint wp = new DoublePoint(x, y);
            DoublePoint pinter = new DoublePoint();

            double t;  //parameterization variable
            double dist = MathUtil.PerpendicularDistance(Start, End, wp, pinter, out t);

            //don't use the whole line, it will collide with nodes
            if (Double.IsNaN(t) || (t < 0.1) || (t > 0.9)) {
                return Double.PositiveInfinity;
            }

            DoublePoint scaledp = GraphicsManager.Instance.PageManager.CurrentScale();
            double scale = (scaledp.X + scaledp.Y) / 2;
            return dist * scale;
        }

    }
}
