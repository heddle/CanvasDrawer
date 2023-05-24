using CanvasDrawer.Graphics.Theme;
using CanvasDrawer.Pages;

/*
 * This is the graphics context.
 */

namespace CanvasDrawer.Graphics
{
    public class Graphics2D
	{

		//default fill color is transparent gray
		public string FillColor { get; set; } = ThemeManager.RectItemFillColor;

		//default line color is black
		public string LineColor { get; set; } = ThemeManager.RectItemLineColor;

		//default line width is 1
		public double LineWidth { get; set; } = 1.0;

		public ELineStyle LineStyle { get; set; } = ELineStyle.SOLID;

		public int FontSize { get; set; } = 9;

		public string FontFamily { get; set; } = "Roboto";

		// "left", "center", or "end"
		public string FontAlign { get; set; } = "left";

		public string TextColor { get; set; } = "#BB2222";

		//empty constructor
		public Graphics2D()
		{
		}

		//copy constructor
		public Graphics2D(Graphics2D src)
		{
			FillColor = src.FillColor;
			LineColor = src.LineColor;
			LineWidth = src.LineWidth;
			LineStyle = src.LineStyle;
			FontSize = src.FontSize;
			FontFamily = (string)src.FontFamily.Clone();
			TextColor = (string)src.TextColor.Clone();
			FontAlign = (string)src.FontAlign.Clone();
		}

		/// <summary>
		/// Restore the default values
		/// </summary>
		public void RestoreDefaults()
		{
			FillColor = ThemeManager.RectItemFillColor;
			LineColor = ThemeManager.RectItemLineColor;
			LineWidth = 1.0;
			LineStyle = ELineStyle.SOLID;
			FontSize = 9;
			FontFamily = "Roboto";
			FontAlign = "left";
			TextColor = "#BB2222";
		}

		/// <summary>
		/// Used to prevent drawing null or empty strings
		/// </summary>
		/// <param name="s">The string to check</param>
		/// <returns>true if it is safe to draw or measure the text.</returns>
		private bool GoodString(string s)
		{
			return (s != null) && (s.Length > 0);
		}

		/// <summary>
		/// Chck whether the rectangle values are safe.
		/// </summary>
		/// <param name="x">The left.</param>
		/// <param name="y">The top.</param>
		/// <param name="w">The width.</param>
		/// <param name="h">The height.</param>
		/// <returns>true id the values are safe.</returns>
		private bool GoodRect(double x, double y, double w, double h)
		{
			return GoodValue(x) && GoodValue(y) && GoodValue(w) && GoodValue(h);
		}

		/// <summary>
		/// Used to prevent drawing objects with bad numerical values
		/// </summary>
		/// <param name="val">The value to check.</param>
		/// <returns>true if the value is safe.</returns>
		private bool GoodValue(double val)
		{
			return !Double.IsNaN(val) && !Double.IsInfinity(val);
		}

		/// <summary>
		/// Draw txt based on the current setting in this 2D graphics context.
		/// </summary>
		/// <param name="x">The horizontal pixel location</param>
		/// <param name="y">The vertical pixel location</param>
		/// <param name="text">The text to draw</param>
		public void DrawText(double x, double y, string text)
		{
			if (GoodString(text) && (JSInteropManager.Instance != null)) {
				JSInteropManager.Instance.DrawText(x, y, text, FontSize, FontFamily, TextColor, FontAlign);
			}
		}

		//get the width of the text
		public double TextWidth(string text)
		{
			if (GoodString(text) && (JSInteropManager.Instance != null)) {
				return JSInteropManager.Instance.TextWidth(text, FontFamily, FontSize);
			}
			return 0;
		}

		//draw a rectangle
		public void DrawRect(Rect rect)
		{
			if (rect != null) {
				DrawRect(rect.X, rect.Y, rect.Width, rect.Height);
			}
		}

		//draw a rectangle
		public void DrawRect(double x, double y, double width, double height)
		{

			if (GoodRect(x, y, width, height) && (JSInteropManager.Instance != null)) {
				double dashLength = (LineStyle == ELineStyle.SOLID) ? 0 : 5;
				JSInteropManager.Instance.DrawRectangle(x, y, width, height, FillColor, LineColor, LineWidth, dashLength);
			}
		}

		public void DrawEllipse(double xc, double yc, double radx, double rady)
		{
			if (JSInteropManager.Instance != null) {
				JSInteropManager.Instance.DrawEllipse(xc, yc, radx, rady, FillColor, LineColor, LineWidth);
			}
		}


		public void DrawArc(double x, double y, double rad,
		double startAngle, double endAngle)
		{
			if (JSInteropManager.Instance != null) {
				double dashLength = (LineStyle == ELineStyle.SOLID) ? 0 : 5;
				JSInteropManager.Instance.DrawArc(x, y, rad, startAngle, endAngle, FillColor, LineColor, LineWidth, dashLength);
			}
		}

		/// <summary>
		/// Draw an image
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="imageId"></param>
		public void DrawImage(double x, double y, double width, double height, string imageId)
		{
			if (GoodRect(x, y, width, height) && GoodString(imageId) && (JSInteropManager.Instance != null)) {
				JSInteropManager.Instance.DrawImage(x, y, width, height, imageId);
			}
		}


		/// <summary>
		/// Draw a rotated image.
		/// </summary>
		/// <param name="x">The horizontal center of where the image is drawn.</param>
		/// <param name="y">The horizontal center of where the image is drawn.</param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="angle">The rotation angle in radians.</param>
		/// <param name="imageId"></param>
		public void DrawRotatedImage(double x, double y, double width, double height, double angle, string imageId)
		{
			if (GoodRect(x, y, width, height) && GoodString(imageId) && (JSInteropManager.Instance != null)) {
				JSInteropManager.Instance.DrawRotatedImage(x, y, width, height, angle, imageId);
			}
		}

		/// <summary>
		/// Low level line drawing
		/// </summary>
		/// <param name="x1">The x coordinate of one point.</param>
		/// <param name="y1">The y coordinate of one point.</param>
		/// <param name="x2">The x coordinate of the other point.</param>
		/// <param name="y2">The x coordinate of the other point.</param>
		public void DrawLine(double x1, double y1, double x2, double y2)
		{
			if (JSInteropManager.Instance != null) {
				double dashLength = (LineStyle == ELineStyle.SOLID) ? 0 : 5;
				JSInteropManager.Instance.DrawLine(x1, y1, x2, y2, LineColor, LineWidth, dashLength);
			}
		}

		//save the "this" Graphics2D
		public Graphics2D Save()
		{
			return new Graphics2D(this);
		}

	}
}
