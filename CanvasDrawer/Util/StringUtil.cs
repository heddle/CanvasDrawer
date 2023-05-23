using System;
using CanvasDrawer.Pages;
using CanvasDrawer.Graphics;

namespace CanvasDrawer.Util
{
	public static class StringUtil {
        /// <summary>
        /// Unicode triangle with an exclamation mark used for warnings
        /// </summary>
        public static readonly string UniWarning = "\u26A0";

        //Contains that could be used for case insensitive
        public static bool Contains(this string source, string substr, StringComparison comp)
        {
            return source?.IndexOf(substr, comp) >= 0;
        }

        /// <summary>
        /// Get the pixel width of a string
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="fontFamily">The font family.</param>
        /// <param name="fontSize">The font size.</param>
        /// <returns>The width in pixels.</returns>
        public static double TextWidth(string text, string fontFamily, int fontSize)
        {
            PageManager pmgr = GraphicsManager.Instance.PageManager;
            return pmgr.TextWidth(text, fontFamily, fontSize);
        }

        /// <summary>
        /// Get the maximum width of an array of strings
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="text">The text.</param>
        /// <param name="fontFamily">The font family.</param>
        /// <param name="fontSize">The font size.</param>
        /// <returns>The maximum width in pixels.</returns>
        public static double MaxWidth(string[] lines, string fontFamily, int fontSize)
        {

            double maxWidth = 0;
            if (lines != null)
            {
                foreach (string line in lines)
                {
                    maxWidth = Math.Max(maxWidth, TextWidth(line, fontFamily, fontSize));
                }
            }

            return maxWidth;
        }


        /// <summary>
        /// Break a string into an array of strings, one element for each new line.
        /// </summary>
        /// <param name="text">The text to split at newlines.</param>
        /// <returns>The array of lines.</returns>
        public static String[] NewLineTokens(string text)
        {
            if (text == null)
            {
                return null;
            }
            string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            return lines;
        }
    }
}

