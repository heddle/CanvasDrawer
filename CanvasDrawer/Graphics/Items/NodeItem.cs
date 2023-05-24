using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CanvasDrawer.DataModel;
using CanvasDrawer.Graphics.Theme;
using CanvasDrawer.Util;
using System.Dynamic;
using CanvasDrawer.Pages;

namespace CanvasDrawer.Graphics.Items {
    public sealed class NodeItem : RectItem {

        private static readonly string IMGPrefix = "img_";

        //default fill and line colors for icons
 

        //icon items have image is strings
        public string ImageId { get; set; }

        //the subnet item that contains this node (if any)
        public SubnetItem Subnet { get; set; }

        public string Icon { get; set; }

        //does it make sense that the node has an operating system?
        public bool HasOS { get; set; } = false;

        /// <summary>
        /// Create a Node Item from properties, probably from Json deserialization.
        /// </summary>
        /// <param name="properties">The properties collection.</param>
        public NodeItem(Properties properties) : base(GraphicsManager.NodeLayer, properties) {
            //must get the ICON before calling custom
            Icon = properties.GetValue(DefaultKeys.ICON_KEY);

            ImageId = IMGPrefix + Icon;
            Properties = properties;

            //not all nodes have OS's
            HasOS = (IsLaptop() || IsPLC() || IsRouter() || IsWireless() || IsServer() || IsWorkstation()
                || IsFirewall() || IsSwitch());

            //backwards compatibility, make sure it has a locked property
            //add a locked property if it doesn't have one
            Property prop = Properties.GetProperty(DefaultKeys.LOCKED_KEY);
            if (prop == null) {
                NotDisplayable(DefaultKeys.LOCKED_KEY, "false");
            }


            ItemManager.Instance.NotifyObservers(new ItemEvent(this, EItemChange.ADDED));
        }

        /// <summary>
        /// Get the text to display for a hover
        /// </summary>
        /// <returns>The text to display on a mouse hover</returns>
        public override String GetHoverText() {
			return "Everything is valid.";
		}

        /// <summary>
        /// /Create a NodeItem.
        /// </summary>
        /// <param name="layer">Which drawing layer it lives on.</param>
        /// <param name="xc">The horzontal location of the center or focus.</param>
        /// <param name="yc">The vertical location of the center or focus.</param>
        /// <param name="imageId">The id of the item's icon image.</param>
        public NodeItem(Layer layer, double xc, double yc, string imageId) :
            base(layer, xc, yc) {

            //must get the ICON before calling custom
            ImageId = imageId;
            Icon = imageId.Replace(IMGPrefix, "");

            //not all nodes have OS's
            HasOS = (IsLaptop() || IsPLC() || IsRouter() || IsWireless() || IsServer() || IsWorkstation()
                || IsFirewall() || IsSwitch());

            CustomizeNodeProperties();
            ItemManager.Instance.NotifyObservers(new ItemEvent(this, EItemChange.ADDED));

            //see if we have been placed on a subnet
        }

       

        /// <summary>
        /// Add the cusom properties for this node.
        /// </summary>
        public void CustomizeNodeProperties() {
           
            FeedbackableOnly(DefaultKeys.BG_COLOR, Properties.DefaultNodeFill);
            FeedbackableOnly(DefaultKeys.FG_COLOR, "none");
            FeedbackableOnly(DefaultKeys.LINE_WIDTH, "0.5");
            FeedbackableOnly(DefaultKeys.ICON_KEY, Icon);
            FeedbackableOnly(DefaultKeys.TYPE_KEY, string.Copy(EItemType.Node.ToString()));

            AllFeatures(DefaultKeys.NAME_KEY, Icon);

            NotDisplayable(DefaultKeys.LOCKED_KEY, "false");

            
        }



        /// <summary>
        /// Get the bounds that includes all displayed adornments.
        /// </summary>
        /// <returns>Get the total effective rectangular bounds for decluttering</returns>
        public override Rect GetTotalBounds() {
            Rect bounds = GetBounds();
            Rect totalBounds = new Rect();
            totalBounds.Y = bounds.Y;

            Graphics2D g = new Graphics2D();
            g.FontAlign = "center";
            double xc = GetBounds().Xc();

            double fheight = 1.2 * g.FontSize;
            int fontslop = 1;
            double maxWidth = bounds.Width;

            double y = GetBounds().Bottom() + fheight + fontslop;

            JSInteropManager jsm = JSInteropManager.Instance;

            if (jsm != null) {
                Property nameProp = Properties.GetProperty(DefaultKeys.NAME_KEY);
                if ((nameProp != null) && nameProp.DisplayedOnCanvas) {
                    String s = TruncateString(nameProp.Value, true);
                    maxWidth = Math.Max(maxWidth, jsm.TextWidth(s, g.FontFamily, g.FontSize));
                    y += fheight + fontslop;
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

                            maxWidth = Math.Max(maxWidth, jsm.TextWidth(displayStr, g.FontFamily, g.FontSize));
                            y += fheight + fontslop;
                        }  //displayed on map, but not name
                    }
                } // not zoomed in too much
            }

            totalBounds.Height = y - totalBounds.Y;
            totalBounds.X = xc - maxWidth / 2;
            totalBounds.Width = maxWidth;
            return totalBounds;
        }


        //snap the node to the grid
        public override void SnapToGrid() {
            double oldxc = GetBounds().Xc();
            double oldyc = GetBounds().Yc();
            double newxc = GraphicsManager.Instance.GridValue(oldxc);
            double newyc = GraphicsManager.Instance.GridValue(oldyc);
           OffsetItem(newxc - oldxc, newyc - oldyc);
        }

        public override void CustomDraw(Graphics2D g) {
            //g.FillColor = ThemeManager.RectItemFillColor;
            //g.LineColor = ThemeManager.RectItemLineColor;
            //g.DrawRect(GetBounds());

            Rect b = GetBounds();
			g.DrawImage(b.X, b.Y, b.Width, b.Height, ImageId);
        }


        public override void AddFeedback(UserEvent ue, List<string> feedbackStrings) {
            feedbackStrings.Add("focus: " + GetFocus());

            //in a subnet box?

            if (Subnet != null) {
                feedbackStrings.Add(Subnet.NameDisplayString());
            }

            base.AddFeedback(ue, feedbackStrings);


        }

        /// <summary>
        /// Duplicate this item.
        /// </summary>
        /// <param name="dx">an offset location parameter (horizontal).</param>
        /// <param name="dy">an offset location parameter (vertical).</param>
        /// <returns>A clone, but with its own Guid.</returns>
        public override Item Duplicate(double dx, double dy) {

            NodeItem copy = new NodeItem(Layer, 0, 0, ImageId);

            //used only to establish connections after duplication
            TempClone = copy;

            //have to copy the properties (but use the new guid)
            Property newGuidProp = Properties.GetProperty(DefaultKeys.GUID_KEY);
            copy.Properties = new Properties(Properties);
            copy.Properties.GetProperty(DefaultKeys.GUID_KEY).Value = Guid.NewGuid().ToString();

            copy.OffsetItem(dx, dy);
            copy.SetLocked(false);

            copy.Selected = false;
            return copy;
        }

        //Is this node a web service?
        public bool IsWebService() {
            return Icon.Contains("WEBSERVICE", StringComparison.OrdinalIgnoreCase);
        }

        //Is this node a router?
        public bool IsRouter() {
            return Icon.Contains("ROUTER", StringComparison.OrdinalIgnoreCase);
        }

        //is this a wireless access point?
        public bool IsWireless() {
            return Icon.Contains("WIRELESS", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsPLC() {
            return Icon.Contains("PLC", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsWorkstation() {
            return Icon.Contains("WORKSTATION", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsSwitch() {
            return Icon.Contains("SWITCH", StringComparison.OrdinalIgnoreCase);
        }

        //Is this node a server?
        public bool IsServer() {
            return Icon.Contains("SERVER", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsLaptop() {
            return Icon.Contains("LAPTOP", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsCloud() {
            return Icon.Contains("CLOUD", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsPrinter() {
            return Icon.Contains("PRINTER", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsFirewall() {
            return Icon.Contains("FIREWALL", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsMirror() {
            return Icon.Contains("MIRROR", StringComparison.OrdinalIgnoreCase);
        }

    }
}
