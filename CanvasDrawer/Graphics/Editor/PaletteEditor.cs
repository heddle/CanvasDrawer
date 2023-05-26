using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CanvasDrawer.Graphics.Selection;
using CanvasDrawer.Graphics.Items;
using CanvasDrawer.Pages;
using CanvasDrawer.Graphics.Popup;

namespace CanvasDrawer.Graphics.Editor {
    public sealed class PaletteEditor : BaseEditor {

        private static PaletteEditor _instance;

        public PaletteEditor() : base() {
        }

        /// <summary>
        /// Acessor for the Palette Editor singleton.
        /// </summary>
        public static PaletteEditor Instance {
            get {
				if (_instance == null) {
					_instance = new PaletteEditor();
				}
				return _instance;
			}
        }


        /// <summary>
        /// A single click occurred on the map. 
        /// </summary>
        /// <param name="item">The item that was clicked.</param>
        public void HandleSingleClick(Item item) {

            if (_hotItem == item) {
                return;
            }

            if ((item != null) && !item.IsConnector()) {
                Restore();
                return;
            }

            if ((item == null) || !item.Selected) {
                Restore();
                return;
            }

        }

        /// <summary>
        /// Prepare the editor
        /// </summary>
        /// <param name="item">The connector item that whose color will be modified.</param>
        public void PrepareColorChange(LineConnectorItem item) {

            if ((item == null) || (_hotItem == item)) {
                return;
            }

            _hotItem = item;
            SetHotItem(_hotItem);
            GraphicsManager.Instance.ForceDraw();
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
