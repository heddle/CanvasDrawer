using System;
namespace CanvasDrawer.DataModel
{
	public class PropertyComparer : IComparer<Property>
	{

		public int Compare(Property? x, Property? y)
		{

			if ((x.Key == null) || (y.Key == null)) {
				return 0;
			}

			//keep name on top
			if (x.Key.Equals(DefaultKeys.NAME_KEY)) {
				return -1;
			} else if (y.Key.Equals(DefaultKeys.NAME_KEY)) {
				return 1;
			}

			
			//keep lock on bottom
			if (x.Key.Equals(DefaultKeys.LOCKED_KEY)) {
				return 1;
			} else if (y.Key.Equals(DefaultKeys.LOCKED_KEY)) {
				return -1;
			}


			return x.Key.CompareTo(y.Key);
		}

	}
}

