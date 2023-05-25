using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CanvasDrawer.Pages;
using CanvasDrawer.State;
using CanvasDrawer.Graphics.Popup;
using CanvasDrawer.Graphics.Theme;
using CanvasDrawer.Json;
using Microsoft.AspNetCore.Components.Web;
using CanvasDrawer.Graphics.Cloning;

namespace CanvasDrawer.Graphics.Toolbar {
    public sealed class ToolbarManager {

        //use thread safe singleton pattern
        private static ToolbarManager _instance;
        private static readonly object _padlock = new object();

        //used to update the status
        public delegate void Refresh();
        public Refresh RefresherNodes { get; set; }
        public Refresh RefresherTools { get; set; }
        public JSInteropManager JsManager { get; set; }

        public PageManager PageManager { get; set; }

        //the buttons
        private readonly List<ToolbarButtonData> _buttons;

        public EToolbarButton SelectedButton { get; private set; } = EToolbarButton.POINTER;

        ToolbarManager() : base() {
            _buttons = new List<ToolbarButtonData>();
            InitButtonData();
        }

        public static ToolbarManager Instance {
            get {
                lock (_padlock) {
                    if (_instance == null) {
                        _instance = new ToolbarManager();
                    }
                    return _instance;
                }
            }
        }

        //Set a buton enebaled or disabled
        public void SetEnabled(EToolbarButton etype, bool enabled) {
            ToolbarButtonData data = FromType(etype);
            data.Enabled = enabled;
            SetButtonBackground(data);
        }

        //initialize the button data
        private void InitButtonData() {
            CreateButtonData(EToolbarButton.POINTER, "pointer", null, true, 0);
            CreateButtonData(EToolbarButton.PAN, "pan", null, true, 0);
            CreateButtonData(EToolbarButton.SUBNET, "subnet", null, true, 0);
            CreateButtonData(EToolbarButton.TEXT, "text", null, true, 0);
            CreateButtonData(EToolbarButton.CONNECTOR, "connector", null, true, 0);
            CreateButtonData(EToolbarButton.ZOOMIN, "zoomin", null, true, 0);
            CreateButtonData(EToolbarButton.ZOOMOUT, "zoomout", null, true, 0);
            CreateButtonData(EToolbarButton.DUPLICATEITEMS, "duplicateitems", null, true, 0, false);
            CreateButtonData(EToolbarButton.DELETEITEMS, "deleteitems", null, true, 0, false);
            CreateButtonData(EToolbarButton.SNAP, "snap", null, true, 0);
            CreateButtonData(EToolbarButton.EDIT, "edit", null, true, 0);
            CreateButtonData(EToolbarButton.THEME, "theme", null, true, 0);

            CreateButtonData(EToolbarButton.WebService, "webservice", "img_webservice", false, 0);
            CreateButtonData(EToolbarButton.Router, "router", "img_router", false, 0);
            CreateButtonData(EToolbarButton.Wireless, "wireless", "img_wireless", false, 0);
            CreateButtonData(EToolbarButton.PLC, "plc", "img_plc", false, 0);
            CreateButtonData(EToolbarButton.Workstation, "workstation", "img_workstation", false, 0);
            CreateButtonData(EToolbarButton.Switch, "switch", "img_switch", false, 0);
            CreateButtonData(EToolbarButton.Server, "server", "img_server", false, 0);
            CreateButtonData(EToolbarButton.Laptop, "laptop", "img_laptop", false, 0);
            CreateButtonData(EToolbarButton.Cloud, "cloud", "img_cloud", false, 0);
            CreateButtonData(EToolbarButton.Printer, "printer", "img_printer", false, 0);
            CreateButtonData(EToolbarButton.Firewall, "firewall", "img_firewall", false, 0);
            CreateButtonData(EToolbarButton.Mirror, "mirror", "img_mirror", false, 0);

            (FromType(EToolbarButton.POINTER)).Selected = true;
        }

        //find the button data based on type
        public ToolbarButtonData FromType(EToolbarButton etype) {

            foreach (ToolbarButtonData data in _buttons) {

                if (data.Type == etype) {
                    return data;
                }
            }
            return null;
        }

        //find the button data based on Id
        public ToolbarButtonData FromId(string id) {

            foreach (ToolbarButtonData data in _buttons) {
                if (data.Id.Equals(id)) {
                    return data;
                }
            }
            return null;
        }

        //reset to pointer selection
        public void RestoreDefault() {
            if (SelectedButton != EToolbarButton.POINTER) {
                (FromType(SelectedButton)).Selected = false;
                (FromType(EToolbarButton.POINTER)).Selected = true;
                SetButtonBackground(FromType(SelectedButton));
                SetButtonBackground(FromType(EToolbarButton.POINTER));
                SelectedButton = EToolbarButton.POINTER;
                if (JSInteropManager.Instance != null) {
					JSInteropManager.Instance.SetCursor("default");
                }
            }
        }


        public void SetButtonBackground(string id) {

            if (id != null) {
                ToolbarButtonData data = GetButtonData(id);
                if (data != null) {
                    SetButtonBackground(data);
                }
            }
        }

        public void SetButtonBackground(ToolbarButtonData data) {

            if ((!data.Enabled)) {
                JsManager.ChangeBackground(data.Id, ThemeManager.ButtonBackgrounds[ThemeManager.DISABLED]);
                JsManager.ChangeBorder(data.Id, ThemeManager.ButtonBackgrounds[ThemeManager.DISABLED]);
            }
            else {
                if (data.Selected) {
                    string bg = data.IsTool ? ThemeManager.ButtonBackgrounds[ThemeManager.SELECTED_TOOL] : ThemeManager.ButtonBackgrounds[ThemeManager.SELECTED_NODE];
                    JsManager.ChangeBackground(data.Id, bg);
                    JsManager.ChangeBorder(data.Id, ThemeManager.SelectedButtonBorder);
                }
                else {

                    if (data.Id.Equals("deleteitems")) {
                        JsManager.ChangeBackground(data.Id, ThemeManager.DeleteNormal);
                        JsManager.ChangeBorder(data.Id, ThemeManager.DeleteNormal);
                    }
                    else {
                        string bg = data.IsTool ? ThemeManager.ButtonBackgrounds[ThemeManager.BG_TOOL] : ThemeManager.ButtonBackgrounds[ThemeManager.BG_NODE];
                        string bc = data.IsTool ? ThemeManager.ButtonBackgrounds[ThemeManager.BG_TOOL] : ThemeManager.DefaultButtonBorder;
                        JsManager.ChangeBackground(data.Id, bg);
                        JsManager.ChangeBorder(data.Id, bc);
                    }

                }
            }
        }

        //convenience method to get toolbar button data
        public ToolbarButtonData GetButtonData(string id) {
            return ToolbarManager.Instance.FromId(id);
        }

        //select the button. Unselect any in the same group if the group >= 0.
        //return the id of the previously selected button in that group, or null
        public string SelectButton(string id) {

            //find the button or give up
            ToolbarButtonData theButton = FromId(id);
            if ((theButton == null) || !theButton.Enabled) {
                return null;
            }

            theButton.Selected = !(theButton.Selected);

            if (theButton.Selected) {
                SelectedButton = theButton.Type;
            }
            else {
                SelectedButton = EToolbarButton.POINTER;
                (FromType(EToolbarButton.POINTER)).Selected = true;
            }

            //if not part of a set of radio buttons or now unselected, done
            if ((theButton.Group < 0) || !(theButton.Selected)) {
                return null;
            }

            //find previous selected if any
            foreach (ToolbarButtonData data in _buttons) {
                if ((data != theButton) && (data.Group == theButton.Group)) {
                    if (data.Selected) {
                        data.Selected = false;

                        if ((data.Id != null) && (data.Id.Equals("connector"))) {
                            GraphicsManager.Instance.ForceDraw();
                        }
                        return data.Id;
                    }
                }
            }
            return null;
        }

        public void SetAllBackgrounds() {
            foreach (ToolbarButtonData data in _buttons) {
                SetButtonBackground(data);
            }
        }

        private void CreateButtonData(EToolbarButton etype, string id, string imageId, bool isTool, int group, bool enabled = true) {
            ToolbarButtonData data = new ToolbarButtonData {
                Selected = false,
                Enabled = enabled,
                Type = etype,
                Group = group,
                Id = (string)id.Clone(),
                ImageId = (imageId == null) ? null : (string)imageId.Clone(),
                IsTool = isTool
            };
            _buttons.Add(data);
        }

        /// <summary>
        /// A node button has been selected
        /// </summary>
        /// <param name="data"></param>
        public void HandleNodesButtonClick(ToolbarButtonData data) {
            StateMachine.Instance.CurrentState = EState.Placing;
        }

        /// <summary>
        /// Handle a right click. Check to see if it is one of the buttons
        /// with a menu.
        /// </summary>
        /// <param name="me">The mouse event</param>
        /// <param name="name">The button name</param>
        public void HandleRightClick(MouseEventArgs me, string name) {

            if (name == null) {
                return;
            }

            //right click on subnet button or connector button?
            if (name.Equals("subnet")) {
                SubnetShapeMenu.Instance.MenuTrigger(me.ClientX, me.ClientY);
            }
            else if (name.Equals("connector")) {
                ConnectorMenu.Instance.MenuTrigger(me.ClientX, me.ClientY);
            }

        }

        /// <summary>
        /// A toolbar button has been clicked
        /// </summary>
        /// <param name="data">The data object for the clicked button.</param>
        public void HandleButtonClick(ToolbarButtonData data) {

            switch (data.Type) {
                case EToolbarButton.POINTER:
                    StateMachine.Instance.CurrentState = EState.Idle;
                    break;
                case EToolbarButton.ZOOMIN:
                    PageManager.ZoomIn();
                    RestoreDefault();
                    break;
                case EToolbarButton.ZOOMOUT:
                    PageManager.ZoomOut();
                    RestoreDefault();
                    break;
                case EToolbarButton.DELETEITEMS:
                    GraphicsManager.Instance.HandleDelete();
                    RestoreDefault();
                    break;
                case EToolbarButton.DUPLICATEITEMS:
                    CloneManager.DuplicateSelectedItems();
                    RestoreDefault();
                    break;
                case EToolbarButton.SNAP:
                    GraphicsManager.Instance.HandleSnap();
                    RestoreDefault();
                    break;
                case EToolbarButton.THEME:
                    ThemeManager.ToggleTheme();
                    RestoreDefault();
                    break;
                default:
                    break;
            }

        }  //Handle Button Click
    }
}
