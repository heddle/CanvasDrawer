using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CanvasDrawer.Graphics.Toolbar;
using CanvasDrawer.Graphics.Selection;
using CanvasDrawer.Graphics.Items;
using CanvasDrawer.Graphics.Editor;

//This class handles the popup menu.

namespace CanvasDrawer.Graphics.Popup {
    public class PopupManager : IItemObserver {

        //all possible menu labels
        private static readonly string EDITPARAMS = "Edit Parameters...";
        private static readonly string CHANGECOLOR = "Change Color...";
        private static readonly string VIEWPARAMS = "View Parameters...";
        private static readonly string SENDBACK = "Send To Back";
        private static readonly string BRINGFRONT = "Bring To Front";

        //the item whose popup is actve
        private Item _hotItem;

        //location of popup x
        public String PopupX { get; set; } = "0px";

        //location of popup y
        public String PopupY { get; set; } = "0px";

        //is the popup visible?
        public bool PopupVisible { get; set; } = false;

        public string[] EditingPopupLabels { get; } = {EDITPARAMS, SENDBACK, BRINGFRONT};
        public string[] ViewOnlyPopupLabels { get; } = { VIEWPARAMS};

        public string[] ConnectorPopupLabels { get;} = { CHANGECOLOR };

        public string[] TextItemPopupLabels { get; } = { EDITPARAMS, CHANGECOLOR };

        public string[] NoLabels { get;} = { };

        //used to update the status of the corresponding razor component
        public delegate void Refresh();
        public Refresh Refresher { get; set; }

        private static PopupManager _instance;

        /// <summary>
        /// Create the PopupManager singleton.
        /// </summary>
        public PopupManager() : base() {
            ItemManager.Instance.Subscribe(this);
        }

		/// <summary>
		/// Get the appropriate set of popup labels
		/// </summary>
		/// <returns>the appropriate set of popup labels</returns>
		public string[] GetPopupLabels() {

			if (_hotItem == null) {
				return NoLabels;
			}


			if (_hotItem.IsLineConnector()) {
				return ConnectorPopupLabels;
			}
			else {
				if (_hotItem.IsLocked()) {
					return ViewOnlyPopupLabels;
				}
				else {
					if (_hotItem.IsText()) {
						return TextItemPopupLabels;
					}
					else {
						return EditingPopupLabels;
					}
				}

			}
		}

		/// <summary>
		/// Public access to the singleton.
		/// </summary>
		public static PopupManager Instance {
            get {
				if (_instance == null) {
					_instance = new PopupManager();
				}
				return _instance;
			}
        }

        //popup menu item selected. We are passed the label.
        public void MenuSelected(string label) {

            if (EDITPARAMS.Equals(label)) {
                HandleEditParameters();
            }
            else if (CHANGECOLOR.Equals(label)) {
                if (_hotItem.IsText()) {
                    TextColorEditor.Instance.PrepareColorChange((TextItem)_hotItem);
                }
                else {
                    PaletteEditor.Instance.PrepareColorChange((LineConnectorItem)_hotItem);
                }
            }
            else if (VIEWPARAMS.Equals(label)) {
                HandleEditParameters();
            }
            else if (SENDBACK.Equals(label)) {
                HandleSendToBack();
            }
            else if (BRINGFRONT.Equals(label)) {
                HandleBringToFront();
            }
            _hotItem = null;
            PopupVisible = false;
            Refresher();
        }

        private void HandleEditParameters() {
            if (_hotItem != null) {
                DisplayManager.Instance.SetEditorVisible();
  //              SelectionManager.Instance.SelectItem(_hotItem);
                GraphicsManager.Instance.HandleSingleClickOnItem(_hotItem);
            }
        }

        private void HandleSendToBack() {
            if (_hotItem != null) {
                _hotItem.Layer.SendToBack(_hotItem);
            }
        }

        private void HandleBringToFront() {
            if (_hotItem != null) {
                _hotItem.Layer.BringToFront(_hotItem);
            }
        }

        /// <summary>
        /// The popup manager gets first crack at mouse
        /// button events. If the event is handled here and
        /// should not be further processed, this returns true.
        /// Otherwise the event just passes through
        /// </summary>
        /// <param name="ue">The mouse event</param>
        /// <returns>true if the event has been processed here.</returns>
        public bool ConsumedMouseButtonEvent(UserEvent ue) {

            switch (ue.Type) {
                case EUserEventType.MOUSEDOWN:
                    
                    if (ue.Button == 2) {
                        return HandleRightClick(ue);
                    }
                    else {
                        if (PopupVisible) {
                            PopupVisible = false;
                            _hotItem = null;
                            Refresher();
                        }
                    }
                    break;

                case EUserEventType.MOUSEUP:
                    break;

                case EUserEventType.SINGLECLICK:
                    break;

            }

            return false; //event passed through
        }


        /// <summary>
        /// Handle a right click.
        /// </summary>
        /// <param name="ue">The mouse event.</param>
        /// <returns>true if the event was handle here</returns>
        private bool HandleRightClick(UserEvent ue) {
            if (!PopupVisible) {
                if (ToolbarManager.Instance.SelectedButton == EToolbarButton.POINTER) {
                    Item item = SelectionManager.Instance.ItemAtEvent(ue);

                    if ((item != null) && item.IsLineConnector()) {
                        LineConnectorItem cnx = (LineConnectorItem)item;
                    }


                    if (item != null) {
                        _hotItem = item;
                        PopupVisible = true;
                        PopupX = (int)(ue.Xpage) + "px";
                        PopupY = (int)(ue.Ypage) + "px";
                        Refresher();
                        return true; //event consumed
                    }
                }
            }

            return false; //event passed through
        }

        public void ItemChangeEvent(ItemEvent e) {
            if (PopupVisible) {
                PopupVisible = false;
                _hotItem = null;
                Refresher();
            }
        }
    }
}
