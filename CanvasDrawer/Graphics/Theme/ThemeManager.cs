using System;
using System.IO;
using System.Collections.Generic;
using CanvasDrawer.Graphics.Items;
using CanvasDrawer.Graphics.Toolbar;
using CanvasDrawer.Pages;
using CanvasDrawer.Util;
using CanvasDrawer.DataModel;

namespace CanvasDrawer.Graphics.Theme {
    public static class ThemeManager {

        //button states
        public static readonly int SELECTED_TOOL = 0;
        public static readonly int SELECTED_NODE = 1;
        public static readonly int BG_TOOL = 2;
        public static readonly int BG_NODE = 3;
        public static readonly int MOUSEON = 4;
        public static readonly int DISABLED = 5;

        //common across themes
        public static string ButtonFontSize = "12px";
        public static string ButtonFont = "Roboto, Arial, Helvetica, sans-serif";
        public static string ButtonTextColor = "#ffffff";
        public static string EditorDividerColor = "#aaaaaa";

        //special colors for delete common all themes
        public static string DeleteNormal = "#c86432";
        public static string DeleteHighlight = "#c80000";

        //default connector line colors common all themes
        public static string DefaultConnectorLineColor = "#92d36e";
        public static readonly string ConnectorSelectColor = "#ff0000";

        //max number of characters for a display string under an icon
        //common for all themes
        public static int MaxDisplayChars = 18;

        //shape and connector menu commom all themes
        public static string ToolMenuBackgroundColor = "#555555";
        public static String ToolMenuHoverColor = "#a7a7af";

        //for the popup, common for all themes
        public static string PopupBackgroundColor = "#dddddd";
        public static string PopupTextColor = "#000000";
        public static String PopupHoverColor = "#b8b8c0";
        public static String PopupFont = "Roboto";
        public static int PopupFontSize = 12;

        public static string DefaultButtonBackground;
        public static string DefaultButtonBorder;


        public static string CanvasColor;
        public static string ContainerColor;
        public static string ContainerBorderColor;
        public static string FeedbackBackground;
        public static string FeedbackTextColor;
        public static string LabelColor;
        public static string GenericBorder;
        public static string EditorKeyBackground;
        public static string EditorKeyTextColor;

        //scroll bars
        public static string ScrollThumbBorder;
        public static string ScrollThumbBackground;
        public static string ScrollThumbHover;

        //button background colors for different states
        public static string[] ButtonBackgrounds = new string[6];

        //these are text colors. They are dictionaries that map a generic name
        //to the color for a specific theme. The colors can be very different
        //than they sound, for example "white" on the light theme is actually
        //black. But this will allow the property to be "white" and the color
        //to be appropriate for the theme.
        private static Dictionary<string, string> LightThemeMap = new Dictionary<string, string>();
        private static Dictionary<string, string> DarkThemeMap = new Dictionary<string, string>();

        //another dictionary that maps index to name
        private static Dictionary<int, string> TextColorKeyMap = new Dictionary<int, string>();



        //map grid color
        public static string CanvasGridColor;

        //annotation text nodes on map
        public static string NodeTextColor;

        //Text area on text item editor
        public static string TextAreaBackground;
        public static string TextAreaTextColor;

        //rect item line color and fill color
        public static string RectItemLineColor;
        public static string RectItemFillColor;

        //hot item coloring
        public static string HotItemBorderColor;
        public static string HotItemTextColor;

        //selected item border
        public static string SelectedItemBorderColor = "#ff0000";

        //editor container coloring
        public static string EditorBorderColor;
        public static string EditorBackgroundColor;

        //selected button border
        public static string SelectedButtonBorder;

        //is the active theme the light theme
        public static bool IsLight { get; private set; } = false;

        //is the active theme the blue theme
        public static bool IsBlue { get; private set; } = true;

        static ThemeManager() {

            //map index to generic color name
            TextColorKeyMap.Add(0, "white");
            TextColorKeyMap.Add(1, "gray");
            TextColorKeyMap.Add(2, "green");
            TextColorKeyMap.Add(3, "blue");
            TextColorKeyMap.Add(4, "red");
            TextColorKeyMap.Add(5, "purple");
            TextColorKeyMap.Add(6, "brown");
            TextColorKeyMap.Add(7, "yellow");


            //set the text theme maps
            LightThemeMap.Add("white",  "#000000");
            LightThemeMap.Add("gray",   "#a9a9a9");
            LightThemeMap.Add("green",  "#006400");
            LightThemeMap.Add("blue",   "#1e90ff");
            LightThemeMap.Add("red",    "#ff0000");
            LightThemeMap.Add("purple", "#a020f0");
            LightThemeMap.Add("brown",  "#a0522d");
            LightThemeMap.Add("yellow", "#ffa500");

            DarkThemeMap.Add("white",  "#ffffff");
            DarkThemeMap.Add("gray",   "#d3d3d3");
            DarkThemeMap.Add("green",  "#90ee90");
            DarkThemeMap.Add("blue",   "#87cefa");
            DarkThemeMap.Add("red",    "#ff4500");
            DarkThemeMap.Add("purple", "#ffb6c1");
            DarkThemeMap.Add("brown",  "#cd853f");
            DarkThemeMap.Add("yellow", "#ffff00");

            //default to dark theme
            SetThemeLight(IsLight, false);
        }

        /// <summary>
        /// Get the theme based color used to draw text
        /// </summary>
        /// <param name="textItem"></param>
        /// <returns></returns>
        public static string GetTextItemColor(TextItem textItem) {

            Property prop = textItem.Properties.GetProperty(DefaultKeys.FG_COLOR);
            String propColor = (prop != null) ? prop.Value : "white";

            //for backwards compatibility
            if (propColor.Contains("#")) {
                propColor = "white";
            }

            string color;

            if (IsLight) {
                if (!LightThemeMap.ContainsKey(propColor)) {
                    propColor = "white";
                }
                LightThemeMap.TryGetValue(propColor, out color);
            }
            else { 
                if (!DarkThemeMap.ContainsKey(propColor)) {
                    propColor = "white";
                }
                DarkThemeMap.TryGetValue(propColor, out color);
            }

            return color;
        }

        /// <summary>
        /// Get the generic color, i.e., the dictionary key.
        /// </summary>
        /// <param name="index">The index into the avaialable colors</param>
        /// <returns>the generic color</returns>
        public static string GetGenericColor(int index) {
            string genericColor = "white";
            if ((index >= 0) || (index < TextColorKeyMap.Count)) {
                TextColorKeyMap.TryGetValue(index, out genericColor);
            }
            else {
                genericColor = "white";
            }

            return genericColor;
        }

        /// <summary>
        /// Get a text color from an index, 0..(num-1) where num is
        /// the number of availaible colors.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The color, based on which theme is active.</returns>
        public static string GetAvailableTextColor(int index) {
   
            string color;
            if (IsLight) {
                LightThemeMap.TryGetValue(GetGenericColor(index), out color);
            }
            else {
                DarkThemeMap.TryGetValue(GetGenericColor(index), out color);
            }
            return color;
        }

        /// <summary>
        /// Get a text color from an index, 0..(num-1) where num is
        /// the number of availaible colors.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The color, based on which theme is active.</returns>
        public static string GetAvailableTextColorComplement(int index) {

            string color;
            if (IsLight) {
                DarkThemeMap.TryGetValue(GetGenericColor(index), out color);
            }
            else {
                LightThemeMap.TryGetValue(GetGenericColor(index), out color);
            }
            return color;
        }
        /// <summary>
        /// Toggle the theme between light and dark.
        /// </summary>
        public static void ToggleTheme() {
            SetThemeLight(!IsLight);
        }

        /// <summary>
        /// Update the theme, as a result of a backend message via CanvasDrawer.razor
        /// </summary>
        /// <param name="theme">SHould be wither "light" or "dark"</param>
        public static void UpdateTheme(String theme) {
            if (theme != null) {
                SetThemeLight(theme.ToLower().Contains("ight"));
            }

        }

        /// <summary>
        /// Set the theme.
        /// </summary>
        /// <param name="theme">Should be from {"light", "dark"}. </param>
        public static void SetTheme(string theme) {
            bool light = theme.Contains("ight");
            SetThemeLight(light, true);
        }

        /// <summary>
        /// Set the theme to light or dark.
        /// </summary>
        /// <param name="isLight">if true, the theme will be light</param>
        /// <param name="refresh">if true, a redraw occurs.</param>
        public static void SetThemeLight(bool isLight, bool refresh = true) {

            IsLight = isLight;

            if (IsBlue) {
                BlueTheme.Configure();
            }
            else {
                GreenTheme.Configure();
            }

            if (refresh) {

                JSInteropManager? jsm = JSInteropManager.Instance;

                if (jsm != null) {
					jsm.ChangeBackground("maincanvas", CanvasColor);
					jsm.ChangeContainerBackground(ContainerColor);
					jsm.ChangeContainerBorderColor(ContainerBorderColor);
                    GraphicsManager.Instance.FullRefresh();
                }
            }

        }
    }
}
