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


        //force the editor to be visible
        public void SetEditorVisible() {

            if (!_showEditor) {
                _showEditor = true;
                GraphicsManager.Instance.FullRefresh();
            }
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
            return PaletteEditor.Instance.GetHotItem() != null;
        }

        /// <summary>
        /// Should the text item color palette be displayed?
        /// </summary>
        /// <returns>true if the text item color palette should be displayed?</returns>
        public bool IsTextColorEditorVisible() {
            return TextColorEditor.Instance.GetHotItem() != null;
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
