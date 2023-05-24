using System;
using System.Text.Json;
using CanvasDrawer.Graphics;

namespace CanvasDrawer.DataModel
{
	public class Properties : List<Property>
	{

		//some defaults
		public static readonly String DefaultBackground = "rgba(85%,85%,85%,0.5)";
		public static readonly String DefaultForeground = "#000000";
		public static readonly String DefaultSelect = "#ff0000";
		public static readonly String DefaultTextFill = "rgba(75%,75%,95%,0.20)";
		public static readonly string DefaultNodeFill = "rgba(75%,95%,75%,0.20)";

		//possible font sizes
		public static String[] FontSizes { get; } = { "  10  ", "  11  ", "  12  ", "  14  ", "16  ", "  18  ", "  20  ", "  24  ", "  28  ", "  32  ", "  36  ", "  40  " };

		//used for sorting
		public static PropertyComparer Comparer { get; private set; } = new PropertyComparer();

		//constructor
		public Properties() : base()
		{
		}

		//copy constructor
		public Properties(Properties src) : base()
		{
			foreach (Property prop in src) {
				Add(new Property(prop));
			}
		}

		/// <summary>
		/// Remove a property based on its key
		/// </summary>
		/// <param name="key">The key of the property</param>
		public void Remove(string key)
		{
			Property prop = GetProperty(key);
			if (prop != null) {
				Remove(prop);
			}
		}


		/// <summary>
		/// Convenience method to get the name property
		/// </summary>
		/// <returns>The name property.</returns>
		public string? GetName()
		{
			return GetValue(DefaultKeys.NAME_KEY);
		}

		/// <summary>
		/// Convenience method to get the icon property
		/// </summary>
		/// <returns>The icon property.</returns>
		public string? GetIcon()
		{
			return GetValue(DefaultKeys.ICON_KEY);
		}

		/// <summary>
		/// Convenience method to get the font size property
		/// </summary>
		/// <returns>The font size property.</returns>
		public string? GetFontSize()
		{
			return GetValue(DefaultKeys.FONTSIZE);
		}

		
		//set a property. return false if it fails because can't find key
		public bool SetProperty(string key, string value)
		{
			Property prop = GetProperty(key);

			if ((prop != null) && (prop.Value != null)) {
				prop.Value = (string)value.Clone();
				return true;
			} else {
				return false;
			}
		}

		//Get a value from a key. Return null if
		//a property with that key is not found
		public string? GetValue(string key)
		{
			Property prop = GetProperty(key);
			if ((prop == null) || (prop.Value == null)) {
				return null;
			}
			return (string)prop.Value.Clone();
		}

		//get the property from a key
		public Property GetProperty(string key)
		{
			foreach (Property prop in this) {
				if (prop.Key.Equals(key)) {
					return prop;
				}
			}
			return null;
		}

		//get the name property from the properties.
		//it should be the first entry but this
		//doesn't rely on it
		public string Name()
		{
			foreach (var prop in this) {
				if (prop.Key != null) {
					if (prop.Key.Equals(DefaultKeys.NAME_KEY)) {
						return prop.Value;
					}
				}
			}

			return "???";
		}

		//create a property with no control bits set
		public Property CreateProperty(String key, string value)
		{
			return CreateProperty(key, value, 0);
		}

		//Create a property
		public Property CreateProperty(String key, string value, int controlBits)
		{
			Property prop = GetProperty(key);
			if (prop == null) {
				prop = new Property(key, value, controlBits);
				Add(prop);
				Sort(Comparer);
			} else {
				prop.Value = (string?)value.Clone();
				prop.ControlBits = controlBits;
			}

			return prop;
		}

		//sort he properties
		public void SortProperties()
		{
			Sort(Comparer);
		}

		//Create a property from a copy
		public Property CreateProperty(Property src)
		{
			return CreateProperty(src.Key, src.Value, src.ControlBits);
		}

		//does this properties contain a given key?
		public bool Contains(string key)
		{
			foreach (var prop in this) {
				if (prop.Key != null) {
					if (prop.Key.Equals(key)) {
						return true;
					}
				}
			}
			return false;
		}

		//shallow clone properties to array
		public Property[] Clone()
		{
			return ToArray();
		}

		//set properties from a property array
		public void Set(Property[] props)
		{
			foreach (Property prop in props) {
				CreateProperty(prop);
			}
			Sort(Comparer);
		}

		//write the properties to a json string
		public string Serialize()
		{
			string jsonStr = JsonSerializer.Serialize<Properties>(this);
			return jsonStr;
		}

		/// <summary>
		/// Designate this as a global properties as opposed to a per item collection.
		/// </summary>
		public void SetGlobal()
		{
			CreateProperty(DefaultKeys.MAPNAME,
				GraphicsManager.Instance.DrawingName, 0);

			CreateProperty(DefaultKeys.SHOWGRID,
				DisplayManager.Instance.ShowGrid.ToString(), 0);
		}

		/// <summary>
		/// check if this is a global properties (as opposed to per item) collection
		/// </summary>
		/// <returns>true if these are global properties</returns>
		public bool IsGlobal()
		{
			//if it has a MAPNAME property, this collection is global
			return (GetProperty(DefaultKeys.MAPNAME) != null);
		}

	}

}

