using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CanvasDrawer.Graphics.Items;
using CanvasDrawer.Pages;
using CanvasDrawer.Graphics.Editor;

namespace CanvasDrawer.Graphics.Reshape {
    public sealed class ReshapeManager {

        private static readonly string TRANSCOLOR = "rgba(95%,85%,85%,0.25)";
        private static readonly string BORDCOLOR = "#555555";


        public PageManager PageManager { get; set; }

        //use thread safe singleton pattern
        private static ReshapeManager _instance;
        private static readonly object _padlock = new object();

        private UserEvent _startEvent;
        private Rect _startRect;

        private Item _hotItem;

        ReshapeManager() : base() {
        }

        //public accessor for the singleton
        public static ReshapeManager Instance {
            get {
                lock (_padlock) {
                    if (_instance == null) {
                        _instance = new ReshapeManager();
                    }
                    return _instance;
                }
            }
        }

        //Initiate the reshaping
        public void InitReshape(Item item, UserEvent ue) {

            _hotItem = item;
            _hotItem.Reshaping = true;
            _startEvent = new UserEvent(ue);


            if (_hotItem.IsSubnet() || _hotItem.IsText()) {
                _startRect = new Rect(item.GetBounds());
            }

            PageManager.SaveCanvasInBackgoundImage();
        }

        /// <summary>
        /// Stop the reshaping.
        /// </summary>
        public void EndReshape() {
            PageManager.RestoreCanvasFromBackgroundImage();
            _hotItem.Reshaping = false;
            _hotItem.AfterReshape();
            _hotItem = null;
            _startEvent = null;
            _startRect = null;
        }

        /// <summary>
        /// Update the reshaping.
        /// </summary>
        /// <param name="ue"></param>
        public void UpdateReshape(UserEvent ue) {
            if (_hotItem.IsSubnet()) {
                UpdateBox(ue);
            }
        }

        private void UpdateBox(UserEvent ue) {
            double dx = ue.X - _startEvent.X;
            double dy = ue.Y - _startEvent.Y;

            Rect bounds = _hotItem.GetBounds();

            PageManager.RestoreRectangularAreaFromBackgroundImage(bounds.X, bounds.Y,
                bounds.Width, bounds.Height);

            switch (_startEvent.ResizeRectIndex) {
                case 0:  //top left
                    bounds.X = _startRect.X + dx;
                    bounds.Y = _startRect.Y + dy;
                    bounds.Width = _startRect.Width - dx;
                    bounds.Height = _startRect.Height - dy;
                    break;

                case 1:  //top right
                    bounds.Y = _startRect.Y + dy;
                    bounds.Width = _startRect.Width + dx;
                    bounds.Height = _startRect.Height - dy;
                    break;

                case 2:  //bottom right
                    bounds.Width = _startRect.Width + dx;
                    bounds.Height = _startRect.Height + dy;
                    break;

                case 3:  //bottom left
                    bounds.X = _startRect.X + dx;
                    bounds.Width = _startRect.Width - dx;
                    bounds.Height = _startRect.Height + dy;
                    break;
            }

            //do this to set the relevant properties
            _hotItem.SetBounds(bounds.X, bounds.Y, bounds.Width, bounds.Height);

            PageManager.DrawRect(bounds,
                        TRANSCOLOR, BORDCOLOR, 1, 0);
        }
    }
}
