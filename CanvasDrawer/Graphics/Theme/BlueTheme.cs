using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CanvasDrawer.Graphics.Theme {
    public static class BlueTheme {

        public static void Configure() {

            if (ThemeManager.IsLight) {
                ThemeManager.DefaultButtonBackground = "#35549b";
                ThemeManager.ButtonBackgrounds[ThemeManager.SELECTED_TOOL] = ThemeManager.DefaultButtonBackground;
                ThemeManager.ButtonBackgrounds[ThemeManager.SELECTED_NODE] = "none";
                ThemeManager.ButtonBackgrounds[ThemeManager.BG_TOOL] = ThemeManager.DefaultButtonBackground;
                ThemeManager.ButtonBackgrounds[ThemeManager.BG_NODE] = "none";
                ThemeManager.ButtonBackgrounds[ThemeManager.MOUSEON] = "#6880cc";
                ThemeManager.ButtonBackgrounds[ThemeManager.DISABLED] = "gray";

                ThemeManager.DefaultButtonBorder = "#bbbbbb";

                //scroll bars
                ThemeManager.ScrollThumbBorder = ThemeManager.DefaultButtonBackground;
                ThemeManager.ScrollThumbBackground = ThemeManager.DefaultButtonBackground;
                ThemeManager.ScrollThumbHover = "#3554ab";

                ThemeManager.CanvasGridColor = "#efefef";
                ThemeManager.NodeTextColor = "#444444";

                ThemeManager.CanvasColor = "white";
                ThemeManager.ContainerColor = "#e4e4e4";
                ThemeManager.ContainerBorderColor = "#d6d6d6";
                ThemeManager.FeedbackBackground = "#f9f9f9";
                ThemeManager.FeedbackTextColor = "black";
                ThemeManager.LabelColor = "black";
                ThemeManager.GenericBorder = "black";
                ThemeManager.EditorKeyBackground = "#00000000";
                ThemeManager.EditorKeyTextColor = "black";

                //Text area
                ThemeManager.TextAreaBackground = "white";
                ThemeManager.TextAreaTextColor = "black";

                ThemeManager.SelectedButtonBorder = "#000000";

                ThemeManager.RectItemLineColor = "#aaaaaa";
                ThemeManager.RectItemFillColor = "rgba(75%,75%,75%,0.25)";

                ThemeManager.HotItemBorderColor = "#cccc00";
                ThemeManager.HotItemTextColor = "#4c7a34";

                ThemeManager.EditorBorderColor = "#d6d6d6";
                ThemeManager.EditorBackgroundColor = "#f4f4f4";

            }
            else { //dark
                ThemeManager.DefaultButtonBackground = "#1e88f3";
                ThemeManager.ButtonBackgrounds[ThemeManager.SELECTED_TOOL] = ThemeManager.DefaultButtonBackground;
                ThemeManager.ButtonBackgrounds[ThemeManager.SELECTED_NODE] = "none";
                ThemeManager.ButtonBackgrounds[ThemeManager.BG_TOOL] = ThemeManager.DefaultButtonBackground;
                ThemeManager.ButtonBackgrounds[ThemeManager.BG_NODE] = "none";
                ThemeManager.ButtonBackgrounds[ThemeManager.MOUSEON] = "#6db7ff";
                ThemeManager.ButtonBackgrounds[ThemeManager.DISABLED] = "gray";

                ThemeManager.DefaultButtonBorder = "#808080";

                //scroll bars
                ThemeManager.ScrollThumbBorder = ThemeManager.DefaultButtonBackground;
                ThemeManager.ScrollThumbBackground = ThemeManager.DefaultButtonBackground;
                ThemeManager.ScrollThumbHover = "#3554ab";

                ThemeManager.CanvasGridColor = "#222222";
                ThemeManager.NodeTextColor = "#ebebeb";

                ThemeManager.CanvasColor = "black";
                ThemeManager.ContainerColor = "#333333";
                ThemeManager.ContainerBorderColor = "#7a7a7a";
                ThemeManager.FeedbackBackground = "#222222";
                ThemeManager.FeedbackTextColor = "yellow";
                ThemeManager.LabelColor = "white";
                ThemeManager.GenericBorder = "gray";

                ThemeManager.EditorKeyBackground = "#00000000";
                ThemeManager.EditorKeyTextColor = "white";

                //Text area
                ThemeManager.TextAreaBackground = "#333333";
                ThemeManager.TextAreaTextColor = "white";

                ThemeManager.SelectedButtonBorder = "#ffffff";

                ThemeManager.RectItemLineColor = "#444444";
                ThemeManager.RectItemFillColor = "rgba(50%,50%,50%,0.25)";

                ThemeManager.HotItemBorderColor = "#ffff40";
                ThemeManager.HotItemTextColor = "#ffff40";

                ThemeManager.EditorBorderColor = "#7a7a7a";
                ThemeManager.EditorBackgroundColor = "#444444";

            }

        }

    }
}
