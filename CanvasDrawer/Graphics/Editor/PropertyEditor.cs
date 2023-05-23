using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CanvasDrawer.Graphics.Items;

namespace CanvasDrawer.Graphics.Editor {
    public sealed class PropertyEditor : BaseEditor {

        //use thread safe singleton pattern
        private static PropertyEditor _instance;
        private static readonly object _padlock = new object();

        public PropertyEditor() : base() {
        }

        //public accessor for the singleton
        public static PropertyEditor Instance {
            get {
                lock (_padlock) {
                    if (_instance == null) {
                        _instance = new PropertyEditor();
                    }
                    return _instance;
                }
            }
        }

        public void HandleSingleClick(Item item) {

            if (_hotItem == item) {
                return;
            }

            if ((item != null) && item.IsConnector()) {
                return;
            }

            if (item == null) {
            //    if ((item == null) || !item.Selected) {
                    Restore();
                return;
            }

            _hotItem = item;
            SetHotItem(_hotItem);
            RefreshAllEditors();
        }


        public override void Restore() {
            _hotItem = null;
            SetHotItem(null);
            GraphicsManager.Instance.Refresh();
            RefreshAllEditors();
        }

    }
}
