using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CanvasDrawer.Graphics.Selection;
using CanvasDrawer.Pages;
using CanvasDrawer.Graphics.Editor;
using CanvasDrawer.Util;

namespace CanvasDrawer.Graphics {
    public sealed class DisplayManager {

        private static DisplayManager? _instance;
     

        //is the GUI overall editable or not?
        private bool _editable = true;

        //Is the property editor to be shown?
        private bool _showEditor = false;

        //Is the feedback displayed?
        private bool _showFeedback = false;

        /// <summary>
        /// Do we display the background grid?
        /// </summary>
        public bool ShowGrid { get; set; } = true;

        /// <summary>
        /// public access to the singleton
        /// </summary>
        public static DisplayManager Instance {
            get {
                    if (_instance == null) {
                        _instance = new DisplayManager();
                    }
                    return _instance;
            }
        }

        //the menu button, which collapse/uncollapse the editor was hit
        public void ToggleEditorDisplay() {
            _showEditor = !_showEditor;
            GraphicsManager.Instance.FullRefresh();
        }


        //Toggle the editability state of the entire
        //map
        public void ToggleEditability() {
            SetEditable(!_editable);
            SharedTimer.Instance.OffsetGrabPending = true;
        }

        //force the editor to be visible
        public void SetEditorVisible() {

            if (!_showEditor) {
                _showEditor = true;
                GraphicsManager.Instance.FullRefresh();
            }
        }

        //set the GUI editiable or not
        public void SetEditable(bool editable) {
            if (_editable == editable) {
                return;
            }

            _editable = editable;

            if (!_editable) {
                SelectionManager.Instance.UnselectAll();
            }

            JsManager().WindowResized();
            GraphicsManager.Instance.FullRefresh();
        }

        //external hook to set map frame size
        public void SetMapFrameSize(int width, int height) {
            GraphicsManager gm = GraphicsManager.Instance;
            if (gm != null) {
                PageManager pm = gm.PageManager;
                if (pm != null) {
                    pm.SetMapFrameSize(width, height);
                }
            }
        }


        /// <summary>
        /// Is the GUI in an overall editable or view only state?
        /// </summary>
        /// <returns>true is GUI is in an editable state.</returns>
        public bool MapIsEditable() {
            return _editable;
        }

        //Is the editor visible?
        public bool IsPropertyEditorVisible() {
            bool palVis = IsPaletteEditorVisible();
            bool tcVis = IsTextColorEditorVisible();
            return !palVis && !tcVis && _showEditor;
        }


        /// <summary>
        /// Should the connector color palette be displayed?
        /// </summary>
        /// <returns>true if the connector color palette should be displayed?</returns>
        public bool IsPaletteEditorVisible() {
            return MapIsEditable() && (PaletteEditor.Instance.GetHotItem() != null);
        }

        /// <summary>
        /// Should the text item color palette be displayed?
        /// </summary>
        /// <returns>true if the text item color palette should be displayed?</returns>
        public bool IsTextColorEditorVisible() {
            return MapIsEditable() && (TextColorEditor.Instance.GetHotItem() != null);
        }

        //convenience method
        private JSInteropManager JsManager() {
            return GraphicsManager.Instance.PageManager.JsManager;
        }

        //Toggle the feedback debugging display
        public void ToggleFeedbackVisibility() {
            SetFeedbackVisibility(!_showFeedback);
        }

        //Set the feedback display flag. Note the overall flag for editable
        //must also be true for the feedback to be displayed.
        public void SetFeedbackVisibility(bool display) {
            if (_showFeedback == display) {
                return;
            }
            _showFeedback = display;
            GraphicsManager.Instance.FullRefresh();
        }

        //Is the feedback displayed? 
        public bool IsFeedbackVisible() {
            return _showFeedback;
        }


    }
}
