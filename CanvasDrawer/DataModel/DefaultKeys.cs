using System;

namespace CanvasDrawer.DataModel
{
    public static class DefaultKeys
    {

        public static readonly string NAME_KEY = "Name";
        public static readonly string TYPE_KEY = "Type";
        public static readonly string ICON_KEY = "Icon";

        public static readonly string GUID_KEY = "Guid";
        public static readonly string TEXT_KEY = "Text";
        public static readonly string FONTFAMILY = "FontFamily";
        public static readonly string FONTSIZE = "FontSize";
        public static readonly string MARGINH = "MarginH";
        public static readonly string MARGINV = "MarginV";


        public static readonly string LEFT = "Left";
        public static readonly string TOP = "Top";
        public static readonly string WIDTH = "Width";
        public static readonly string HEIGHT = "Height";
        public static readonly string STARTGUID = "Connect1";
        public static readonly string ENDGUID = "Connect2";
        public static readonly string HORIZFIRST = "HorizFirst";
        public static readonly string FG_COLOR = "Foreground";
        public static readonly string BG_COLOR = "Background";
        public static readonly string SELECT_COLOR = "SelectColor";
        public static readonly string LINE_WIDTH = "LineWidth";
        public static readonly string LINE_STYLE = "LineStyle";

        public static readonly string LOCKED_KEY = "Locked";

		public static readonly string SHAPE = "Shape";

        //global properties, not per item
        public static readonly string GLOBAL_PREFIX = "GLOBAL_";
        public static readonly string MAPNAME = GLOBAL_PREFIX + "PicName";
        public static readonly string SHOWGRID = GLOBAL_PREFIX + "ShowGrid";


    }
}
