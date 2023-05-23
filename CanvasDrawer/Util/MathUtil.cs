using System;
using CanvasDrawer.Graphics;

namespace CanvasDrawer.Util
{
	public static class MathUtil
	{


		/// <summary>
		/// Check whether point p is to the left of the directed line
		/// from start to end.
		/// </summary>
		/// <param name="start">The start of the line.</param>
		/// <param name="end">The end of the line.</param>
		/// <param name="p">The point to check.</param>
		/// <returns>true if the point is to the left of the line.</returns>
		public static bool PointIsLeft(DoublePoint start, DoublePoint end, DoublePoint p)
		{
			double wx = p.X - start.X;
			double wy = p.Y - start.Y;
			double rx = end.X - start.X;
			double ry = end.Y - start.Y;

			double cross = rx * wy - ry * wx;
			return cross < 0;
		}

		/// <summary>
		/// Given two points p0 and p1, imagine a line from p0 to p1. Take the line to be
		/// parameterized by parameter t so that at t = 0 we are at p0 and t = 1 we are at p1.
		/// </summary>
		/// <param name="p0">Start point of main line</param>
		/// <param name="p1">End point of main line</param>
		/// <param name="wp">The point from which we drop a perpendicular to p0 -> p1</param>
		/// <param name="pintersect">The intersection point of the perpendicular and the line. 
		/// It may or may not actually be between p0 and p1, as specified by the value of t.</param>
		/// <returns>The perpendicular distance to the line. If t is between 0 and 1 the
		/// intersection is on the line. If t < 0 the intersection is on the
		/// "infinite line" but not on p0->p1, it is on the p0 side; this returns
		/// the distance to p0. If t > 1 the intersection is on the p1 side; this
		/// returns the distance to p1.</returns>

		public static double PerpendicularDistance(DoublePoint p0, DoublePoint p1, DoublePoint wp,
				DoublePoint pintersect, out double t)
		{
			double delx = p1.X - p0.X;
			double dely = p1.Y - p0.Y;

			double numerator = delx * (wp.X - p0.X) + dely * (wp.Y - p0.Y);
			double denominator = delx * delx + dely * dely;
			t = numerator / denominator;
			pintersect.X = p0.X + t * delx;
			pintersect.Y = p0.Y + t * dely;

			if (t < 0.0) { // intersection not on line, on p0 side
				return p0.Distance(wp);
			} else if (t > 1.0) {// intersection not on line, on p1 side
				return p1.Distance(wp);
			}
			// intersection on line
			return pintersect.Distance(wp);
		}


	}
}

