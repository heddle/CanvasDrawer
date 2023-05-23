using System;
using System.Timers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CanvasDrawer.Graphics.Toolbar;
using CanvasDrawer.Graphics.Selection;
using CanvasDrawer.Graphics.Items;
using CanvasDrawer.State;
using CanvasDrawer.Util;
using CanvasDrawer.Graphics.Theme;

namespace CanvasDrawer.Graphics.Hover {
    public class HoverManager : IItemObserver {

        //the item who is being hovered (or potentially hovered)
        private Item _hotItem;

        //the trigger time
        private long _triggerTime;

        //has the hover been triggered
        private bool _triggered;

        //the pause duration in ms
        private long _duration = 500;

        //the text bounds
        private Rect _bounds = new Rect();

        //horizontal location of hover
        public double HoverX { get; set; } = Double.NaN;

        //vertical location of hover
        public double HoverY { get; set; } = Double.NaN;

        //is the hover visible?
        public bool HoverVisible { get; set; } = false;

        //use thread safe singleton pattern
        private static HoverManager _instance;
        private static readonly object _padlock = new object();

        //margins
        private int _marginH = 2;
        private int _marginV = 2;

        public HoverManager() : base() {
            ItemManager.Instance.Subscribe(this);
            SharedTimer.Instance.Timer.Elapsed += OnTimedEvent;
        }

        //public access to the singleton
        public static HoverManager Instance {
            get {
                lock (_padlock) {
                    if (_instance == null) {
                        _instance = new HoverManager();
                    }
                    return _instance;
                }
            }
        }

        /// <summary>
        /// Convenience method to get the time in ms.
        /// </summary>
        /// <returns>The time in ms.</returns>
        private static long TimeInMillis() {
            long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            return milliseconds;
        }

        private void HoverUp() {
            if (!_triggered) {
                _triggered = true;
                _triggerTime = long.MaxValue;
                DrawHover(GraphicsManager.Instance.G2D);
            }
        }

        private void HoverDown() {
            if (_triggered) {
                _triggered = false;
                GraphicsManager.Instance.ForceDraw();
            }
        }

        private void Reset() {
            _hotItem = null;
            if (_triggered) {
                HoverDown();
            }
        }

        /// <summary>
        /// Called when the timer fires
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnTimedEvent(Object source, ElapsedEventArgs e) {
            long deltaT = TimeInMillis() - _triggerTime;

            if (_hotItem != null) {
                if (deltaT > _duration) {
                    HoverUp();
                }
            }
        }

        //selection filter
        private bool NodesOnly(EItemType type) {
            return type == EItemType.Node;
        }

        /// <summary>
        /// Handle a user event, i.e. it's effect on a potential hover
        /// </summary>
        /// <param name="ue">The mose event.</param>
        public void UserEvent(UserEvent ue) {

            switch (ue.Type) {
                case EUserEventType.MOUSEMOVE:

                    if (StateMachine.Instance.CurrentState != EState.Idle) {
                        Reset();
                        return;
                    }
                    Item item = SelectionManager.Instance.ItemAtEvent(ue, NodesOnly);
                    if (item == null) {
                        Reset();
                    }
                    else if (item != _hotItem) {
                        _hotItem = item;
                        HoverX = ue.X;
                        HoverY = ue.Y;
                        _triggerTime = TimeInMillis();
                    }
                    break;

                default:
                    Reset();
                    break;
            }
        }



        private void SizeBounds(string text) {

            //left and top are the "anchor"
            double left = HoverX;
            double top = HoverY;

            string[] lines = StringUtil.NewLineTokens(text);
            int numLines = (lines == null) ? 0 : lines.Length;

            double height = 2 * _marginV + (numLines * 1.05*ThemeManager.PopupFontSize) + 1;
            double width = 2 * _marginH + 1.05*StringUtil.MaxWidth(lines, ThemeManager.PopupFont, ThemeManager.PopupFontSize) + 2;

            _bounds.Set(left, top, width, height);

        }
        public void DrawHover(Graphics2D g) {
            if (_triggered && (_hotItem != null)) {

                string text = _hotItem.GetHoverText();

                if ((text != null) && (text.Length > 0)) {
                    string[] lines = StringUtil.NewLineTokens(text);
                    SizeBounds(text);

                    Graphics2D gsave = g.Save();
                    gsave.LineColor = "#000000";
                    gsave.FillColor = "#eeeeee";
                    gsave.TextColor = "#000000";
                    gsave.FontAlign = "left";
                    gsave.FontFamily = ThemeManager.PopupFont;
                    gsave.FontSize = ThemeManager.PopupFontSize;

                    gsave.DrawRect(_bounds);

                    double x = HoverX + _marginH;
                    double y = HoverY + _marginV + ThemeManager.PopupFontSize;

                    double lineHeight = 1.2 * ThemeManager.PopupFontSize;

                    foreach (string line in lines) {
                        gsave.DrawText(x, y, line);
                        y += lineHeight;
                    }
                }
            }
        }

        /// <summary>
        /// The item observer interface
        /// </summary>
        /// <param name="e"></param>
        public void ItemChangeEvent(ItemEvent e) {
            Reset();
        }
    }
}