using System;
using System.Collections.Generic;
using CanvasDrawer.Graphics.Selection;
using CanvasDrawer.Graphics.Items;
using CanvasDrawer.Pages;

namespace CanvasDrawer.Graphics.Dragging {
    public sealed class DragManager {


        //use thread safe singleton pattern
        private static DragManager? _instance;
        private static readonly object _padlock = new object();

        private UserEvent _currentEvent;
        private List<Item> _selectedItems;

        private Rect _confineRect;
        private double _cw, _ch;

        DragManager() : base() {
        }

        //public accessor for the singleton
        public static DragManager Instance {
            get {
                lock (_padlock) {
                    if (_instance == null) {
                        _instance = new DragManager();
                    }
                    return _instance;
                }
            }
        }

        public void InitDragging(UserEvent ue) {
			JSInteropManager? jsm = JSInteropManager.Instance;
			if (jsm == null) {
				return;
			}
			_currentEvent = new UserEvent(ue);
            _selectedItems = SelectionManager.Instance.SelectedItems();
            _confineRect = GraphicsManager.Confines();
            _cw = jsm.CanvasWidth;
            _ch = jsm.CanvasHeight;

        }

        //A mouse move while dragging
        public void UpdateDragging(UserEvent ue) {
            double dx = ue.X - _currentEvent.X;
            double dy = ue.Y - _currentEvent.Y;


            //keep in canvas boundaries
            double newLeft = _confineRect.X + dx;
            double newRight = _confineRect.Right() + dx;

            if (newLeft < 0) {
                dx = -_confineRect.X;
            }

            if (newRight > _cw) {
                dx = _cw - _confineRect.Right();
            }

            double newTop = _confineRect.Y + dy;
            double newBottom = _confineRect.Bottom() + dy;

            if (newTop < 0) {
                Console.WriteLine("Off top");
                dy = -_confineRect.Y;
            }

            if (newBottom > _ch) {
                dy = _ch - _confineRect.Bottom();
            }



            if ((Math.Abs(dx) > 2) || (Math.Abs(dy) > 2)){
                _currentEvent.Set(ue);

                _confineRect.Move(dx, dy);

                foreach (Item item in _selectedItems) {
                    if (!item.IsLocked()) {
                        item.DragMove(dx, dy);
                    }
                }
            }

        }
    }
}
