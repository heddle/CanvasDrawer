using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CanvasDrawer.DataModel;
using CanvasDrawer.Pages;
using CanvasDrawer.Graphics.Theme;
using CanvasDrawer.Util;

namespace CanvasDrawer.Graphics.Items {
    public class TextItem : RectItem {

        //the auto sizing text region
        // private TextRegion _textRegion;

        /// <summary>
        /// Create a TextItem from properties, most likely importing JSON.
        /// </summary>
        /// <param name="properties">The model properties.</param>
        public TextItem(Properties properties) : base(GraphicsManager.AnnotationLayer, properties) {

            //if name is there, remove it (backward compatibility)
            Property nameProp = properties.GetProperty(DefaultKeys.NAME_KEY);
			if (nameProp != null) {
                properties.Remove(nameProp);
			}

            Properties = properties;
            ItemManager.Instance.NotifyObservers(new ItemEvent(this, EItemChange.ADDED));
        }

        /// <summary>
        /// Create a text item.
        /// </summary>
        /// <param name="layer">The drawing layer.</param>
        /// <param name="left">The left of the start of the text</param>
        /// <param name="bottom">The bottom of the start of the text</param>
        public TextItem(Layer layer, double left, double bottom) : base(layer, new Rect(left, bottom-1, 1, 1)) {
            CustomizeProperties();
            ItemManager.Instance.NotifyObservers(new ItemEvent(this, EItemChange.ADDED));
        }

        /// <summary>
        /// Add the custom properties for a text item
        /// </summary>
        public void CustomizeProperties() {
            FeedbackableOnly(DefaultKeys.TYPE_KEY, EItemType.Text.ToString());
            
            FeedbackableOnly(DefaultKeys.BG_COLOR, Properties.DefaultTextFill);
            FeedbackableOnly(DefaultKeys.FG_COLOR, Properties.DefaultForeground);

            FeedbackableOnly(DefaultKeys.FONTFAMILY, "Roboto");
            NotDisplayable(DefaultKeys.FONTSIZE, "14");
            FeedbackableOnly(DefaultKeys.MARGINH, "2");
            FeedbackableOnly(DefaultKeys.MARGINV, "2");
            FeedbackableOnly(DefaultKeys.FONTFAMILY, "white");

            AllFeatures(DefaultKeys.TEXT_KEY, "Edit this text.");

        }

		/// <summary>
        /// Convenience method to get the font family
        /// </summary>
        /// <param name="item">The text item in question.</param>
        /// <returns>The font family.</returns>
		public static String GetFontFamily(TextItem item) {
            Property prop = item.Properties.GetProperty(DefaultKeys.FONTFAMILY);
			if (prop == null) {
                prop = item.FeedbackableOnly(DefaultKeys.FONTFAMILY, "Roboto");
            }

			return prop.Value;
		}

        //get the font size in pixels
        /// <summary>
        /// Get the text item's font size in pixels
        /// </summary>
        /// <param name="item">The text item in question.</param>
        /// <returns>The text item's font size in pixels</returns>
        public static int GetFontSize(TextItem item) {
            Property prop = item.Properties.GetProperty(DefaultKeys.FONTSIZE);
            if (prop == null) {
                prop = item.FeedbackableOnly(DefaultKeys.FONTSIZE, "12");
            }

			int fs;
			try {
                fs = Int32.Parse(prop.Value);
			}
			catch (Exception) {
                fs = 12;
			}
            return fs;
        }

        /// <summary>
        /// Get the horizontal margin in pixels.
        /// </summary>
        /// <param name="item">The text item in question.</param>
        /// <returns>The horizontal margin in pixels.</returns>
        public static int GetMarginH(TextItem item) {
            Property prop = item.Properties.GetProperty(DefaultKeys.MARGINH);
            if (prop == null) {
                prop = item.FeedbackableOnly(DefaultKeys.MARGINH, "2");
            }

            int margin;
            try {
                margin = Int32.Parse(prop.Value);
            }
            catch (Exception) {
                margin = 2;
            }
            return margin;
        }

        /// <summary>
        /// Get the vertical margin in pixels.
        /// </summary>
        /// <param name="item">The text item in question.</param>
        /// <returns>The vertical margin in pixels.</returns>
        public static int GetMarginV(TextItem item) {
            Property prop = item.Properties.GetProperty(DefaultKeys.MARGINV);
            if (prop == null) {
                prop = item.FeedbackableOnly(DefaultKeys.MARGINV, "2");
            }

            int margin;
            try {
                margin = Int32.Parse(prop.Value);
            }
            catch (Exception) {
                margin = 2;
            }
            return margin;
        }

        //get the line spacing
        private static double LineGap(TextItem item) {
            return 0.2 * GetFontSize(item);
        }

        //size the bounding box
        private void SizeBounds() {

			//left and top are the "anchor"
            double left = GetLeft();
            double top = GetTop();

            string[] lines = StringUtil.NewLineTokens(GetText());
            int numLines = (lines == null) ? 0 : lines.Length;


            double height = 2 * GetMarginV(this) + (numLines * GetFontSize(this) + numLines*LineGap(this));
            double width = 2 * GetMarginH(this) + StringUtil.MaxWidth(lines, GetFontFamily(this), GetFontSize(this));

            SetBounds(left, top, width, height);

        }


        /// <summary>
        /// Get the text of the text item.
        /// </summary>
        /// <returns>The text item's text</returns>
        public string GetText() {
            return Properties.GetValue(DefaultKeys.TEXT_KEY);
        }

        public override void DrawAnnotations(Graphics2D g) {
        }

        /// <summary>
        /// Draw the text item.
        /// </summary>
        /// <param name="g">The graphics context.</param>
        public override void CustomDraw(Graphics2D g) {

            string[] lines = StringUtil.NewLineTokens(GetText());
			if (lines == null) {
                return;
			}

            SizeBounds();

            double x = GetLeft() + GetMarginH(this);
            double y = GetTop() + GetMarginV(this);

            Graphics2D gsave = g.Save();

            gsave.FontAlign = "left";
            gsave.FontFamily = GetFontFamily(this);
            gsave.FontSize = GetFontSize(this);

            double lineHeight = GetFontSize(this);
            double gap = LineGap(this);


            gsave.TextColor = ThemeManager.GetTextItemColor(this);

            foreach (string line in lines) {
                y += lineHeight;
                gsave.DrawText(x, y, line);
                y += gap;
            }
        }

        /// <summary>
        /// Duplicate this item.
        /// </summary>
        /// <param name="dx">an offset location parameter (horizontal).</param>
        /// <param name="dy">an offset location parameter (vertical).</param>
        /// <returns>A clone, but with its own Guid.</returns>
        public override Item Duplicate(double dx, double dy) {

            TextItem copy = new TextItem(Layer, 0, 0);

            //have to copy the properties (but use the new guid)
            Property newGuidProp = Properties.GetProperty(DefaultKeys.GUID_KEY);
            copy.Properties = new Properties(Properties);
            Properties.CreateProperty(newGuidProp);

            copy.OffsetItem(dx, dy);
            copy.SetLocked(false);

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
