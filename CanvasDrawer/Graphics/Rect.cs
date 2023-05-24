using System;
using System.Xml.Schema;

namespace CanvasDrawer.Graphics {

    public class Rect {

        //usual rectangle parameters
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        //empty constructor
        public Rect() {
        }

        /// <summary>
        /// Creat a rectangle
        /// </summary>
        /// <param name="x">The left of the rectangle.</param>
        /// <param name="y">The top of the rectangle.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        public Rect(double x, double y, double width, double height) {
            Set(x, y, width, height);
       }

        //copy constructor
        public Rect(Rect src) {
            Set(src.X, src.Y, src.Width, src.Height);
        }

        /// <summary>
        /// Create a Rect from two points
        /// </summary>
        /// <param name="p1">One corner</param>
        /// <param name="p2">Opposite corner</param>
        public Rect(DoublePoint p1, DoublePoint p2) {
            Set(p1, p2);
         }

		/// <summary>
		/// Set the Rect from two points
		/// </summary>
		/// <param name="p1">One corner</param>
		/// <param name="p2">Opposite corner</param>
		public void Set(DoublePoint p1, DoublePoint p2) {
            double x = Math.Min(p1.X, p2.X);
            double y = Math.Min(p1.Y, p2.Y);
            double width = Math.Abs(p2.X - p1.X);
            double height = Math.Abs(p2.Y - p1.Y);
            Set(x, y, width, height);
        }


        //set from another rect
        public void Set(Rect src) {
            Set(src.X, src.Y, src.Width, src.Height);
        }

        public void Set(double x, double y, double width, double height) {

            //keep in canonical form
            if (width < 0) {
                width = -width;
                x -= width;
            }

            if (height < 0) {
                height = -height;
                y -= height;
            }

            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        //create a rectangle from two points
        public static Rect FromTwoPoints(double x1, double y1, double x2, double y2) {
            double w = Math.Abs(x2 - x1);
            double h = Math.Abs(y2 - y1);
            double l = Math.Min(x1, x2);
            double t = Math.Min(y1, y2);
            return new Rect(l, t, w, h);
        }

        //get the top of the rectangle
        public double Right() {
            return X + Width;
        }

        //get the bottom of the rectangle
        public double Bottom() {
            return Y + Height;
        }

        //does the rectangle contain a point?
        public bool Contains(double x, double y) {
            bool xgood = ((x > X) && (x < Right()));
            return xgood && (y > Y) && (y < Bottom());
        }

        //does the rectangle contain a point?
        public bool Contains(DoublePoint pp) {
            return Contains(pp.Y, pp.Y);
        }

        //see if this rectangle overlaps with another
        public bool Intersects(Rect r) {
            if ((X > r.Right()) || (r.X > Right())) {
                return false;
            }

            if ((Y > r.Bottom()) || (r.Y > Bottom())) {
                return false;
            }

            return true;
        }

        //Does this rect full contain another?
        public bool Contains(Rect r) {
            if ((X > r.X) || Right() < r.Right()) {
                return false;
            }

            if ((Y > r.Y) || Bottom() < r.Bottom()) {
                return false;
            }

            return true;
        }

        //move the rectangle
        public void Move(double dx, double dy) {
            X += dx;
            Y += dy;
        }

        //Get the center of the rectangle
        public DoublePoint Center() {
            DoublePoint dp = new DoublePoint();
            Center(dp);
            return dp;
        }


        //Get the center of the rectangle
        public void Center(DoublePoint dp) {
            dp.X = Xc();
            dp.Y = Yc();
        }

        //get the x center
        public double Xc() {
            return X + Width / 2;
        }

        //get the y center
        public double Yc() {
            return Y + Height / 2;
        }

        //string representation
        public override string ToString() {
            return String.Format("L: {0:0.##} T: {1:0.##} R: {2:0.##} B: {3:0.##} W: {4:0.##} H: {5:0.##}",
                X, Y, Right(), Bottom(), Width, Height);
        }

        public void GrowRect(double dx, double dy) {
            X -= dx;
            Width += (2 * dx);

            Y -= dy;
            Height += (2 * dy);
        }

        /// <summary>
        /// Uninion with another rectangle. Make this rect just big enough to containg both.
        /// </summary>
        /// <param name="r">The other rectangle to union with.</param>
        public void Union(Rect r) {
            double left = Math.Min(X, r.X);
            double top = Math.Min(Y, r.Y);
            double right = Math.Max(Right(), r.Right());
            double bottom = Math.Max(Bottom(), r.Bottom());
            Set(left, top, right - left, bottom - top);
        }

    }

}
