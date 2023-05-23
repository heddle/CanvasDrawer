using System;
namespace CanvasDrawer.Graphics
{
	public sealed class DoublePoint
	{
		public double X { get; set; }
		public double Y { get; set; }

		//standard 2D point
		public DoublePoint()
		{
		}

		public DoublePoint(DoublePoint src)
		{
			Set(src);
		}

		public DoublePoint(double x, double y)
		{
			Set(x, y);
		}

		//Set the point
		public void Set(double x, double y)
		{
			X = x;
			Y = y;
		}

		//Set from another point
		public void Set(DoublePoint src)
		{
			Set(src.X, src.Y);
		}

		//Move the point
		public void Move(double dx, double dy)
		{
			X += dx;
			Y += dy;
		}

		//Euclidean distance to another point
		public double Distance(DoublePoint p)
		{
			return Math.Sqrt(DistanceSq(p));
		}

		//square of Euclidean distance to another point
		public double DistanceSq(DoublePoint p)
		{
			double dx = p.X - X;
			double dy = p.Y - Y;
			return dx * dx + dy * dy;
		}

		public override string ToString()
		{
			return String.Format("[{0:0.#}, {1:0.#}]",
				X, Y);
		}
	}
}

