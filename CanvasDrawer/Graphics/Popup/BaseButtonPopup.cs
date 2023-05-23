using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using CanvasDrawer.Graphics.Toolbar;

namespace CanvasDrawer.Graphics.Popup {
    public class BaseButtonPopup {

        //location of popup x
        public String PopupX { get; set; } = "0px";

        //location of popup y
        public String PopupY { get; set; } = "0px";

        //is the popup visible?
        public bool PopupVisible { get; set; } = false;

        //the images on the popup
        public string[] ImageFiles { get; set; }

        //the current slection
        public int CurrentSelection { get; set; }

        //used to update the status of the corresponding razor component
        public delegate void Refresh();
        public Refresh Refresher { get; set; }

        //the id of the button having the popup
        private string _buttonId;

        public BaseButtonPopup(string[] imageFiles, string buttonId) {
            ImageFiles = imageFiles;
            _buttonId = buttonId;
        }

        /// <summary>
        /// Get the image file for the current selection.
        /// </summary>
        /// <returns>the image file for the current selection.</returns>
        public string CurrentImageFile() {
            return ImageFiles[CurrentSelection];
        }


        /// <summary>
        /// A mouse down somewhere on the page. Use it to close down the menu.
        /// </summary>
        /// <param name="e">The mouse down event.</param>
        public void GlobalMouseDown(MouseEventArgs e) {
            if (PopupVisible) {
                PopupVisible = false;
                Refresher();
            }
        }

        /// <summary>
        /// The menu has been selected.
        /// </summary>
        /// <param name="name">Thus will be the image file name of the selection.</param>
        public void MenuSelection(String name) {
            int selection = -1;

            for (int i = 0; i < ImageFiles.Length; i++) {
                if (ImageFiles[i].Equals(name)) {
                    selection = i;
                    break;
                }
            }

            if ((selection >= 0) && (selection != CurrentSelection)) {
                CurrentSelection = selection;
                ToolbarManager.Instance.RefresherTools();
            }


        }

        /// <summary>
        /// The menu has been triggered
        /// </summary>
        /// <param name="x">The horizontal location for the popup.</param>
        /// <param name="y">The vertical location for the popup</param>
        public void MenuTrigger(double x, double y) {
            if (!PopupVisible) {
                PopupVisible = true;
                PopupX = (int)(x) + "px";
                PopupY = (int)(y) + "px";
            }
            else {
                PopupVisible = false;
            }

            ToolbarManager.Instance.SelectButton(_buttonId);
            Refresher();
        }


    }
}
