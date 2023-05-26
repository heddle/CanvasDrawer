using System.Collections.Generic;
using Microsoft.AspNetCore.Components.Web;
using CanvasDrawer.Graphics.Selection;
using CanvasDrawer.Graphics.Cloning;
using CanvasDrawer.Graphics.Items;
using CanvasDrawer.Json;
using CanvasDrawer.Graphics.Toolbar;
using CanvasDrawer.Graphics.Theme;
using System;
using CanvasDrawer.Util;

namespace CanvasDrawer.Graphics.Keyboard {
    public class KeyboardManager {

        private static KeyboardManager? _instance;

        KeyboardManager() : base() {
        }

        public static KeyboardManager Instance {
            get {
				if (_instance == null) {
					_instance = new KeyboardManager();
				}
				return _instance;
			}
        }


        //a key down-- has to have correct focus
        //since the broswer traps many of the desired keys lower-case,
        //we compromise and take the upper case. So, for example,
        //Control-Shift-A to select all
        public void KeyDown(KeyboardEventArgs e) {
            string key = e.Key.ToLower();

            if (SelectionManager.Instance.AnySelectedItems(true)
                && ("backspace".Equals(key) || ("delete".Equals(key)))) {
                GraphicsManager.Instance.HandleDelete();
                return;
            }


            bool mod = e.CtrlKey && e.ShiftKey;

            if (mod && "d".Equals(key)) {
                HandleModShiftD(); //duplicate
            }
            else if (mod && "a".Equals(key)) {
                HandleModShiftA(); //select all
            }
            else if (mod && "j".Equals(key)) {
                HandleModShiftJ();
            }
            else if (mod && "l".Equals(key)) {
                HandleModShiftL();  //for locking
            }
            else if (mod && "u".Equals(key)) {
                HandleModShiftU();  //for unlocking
            }
            else if (mod && "v".Equals(key)) {
                HandleModShiftV();  //for deserializing
            }
            else if (mod && "g".Equals(key)) {
                HandleModShiftG();  //for toggling grid
            }
            else if (mod && "f".Equals(key)) {
                HandleModShiftF();  //for toggling feedback display
            }
            else if (mod && "z".Equals(key)) {
                HandleModShiftZ();  //test canvas
            }

        } //KeyDown


        //Control shift D duplicates selected nodes
        private static void HandleModShiftD() {
            CloneManager.DuplicateSelectedItems();
        } //Control Shift D


        //control shift G toggles enabled and disabled
        private static void HandleModShiftG() {
            bool showGrid = DisplayManager.Instance.ShowGrid;
            DisplayManager.Instance.ShowGrid = !showGrid;
            GraphicsManager.Instance.ForceDraw();
        }

        //control shift F toggles feedback display used for debugging
        private static void HandleModShiftF() {
            DisplayManager.Instance.ToggleFeedbackVisibility();
        }

        //write out the JSON to the JSON page 
        private static void HandleModShiftJ() {
            JsonManager.Instance.FillJSonPage();
        }

      

        //control shift A selects all
        private static void HandleModShiftA() {
            SelectionManager.Instance.SelectAll();
            GraphicsManager.Instance.FullRefresh();
        }

        //control shift L for locking selected nodes and subnets
        private static void HandleModShiftL() {
            List<Item> items = SelectionManager.Instance.SelectedItems();
            if (items != null) {
                foreach (Item item in items) {
                    if ((item.IsSubnet() || item.IsNode())) {
                        item.SetLocked(true);
                    }
                }

                GraphicsManager.Instance.ForceDraw();
            }
        }

        //control shift U for unlocking selected nodes and subnets
        private static void HandleModShiftU() {
            List<Item> items = SelectionManager.Instance.SelectedItems();
            if (items != null) {
                foreach (Item item in items) {
                    if ((item.IsSubnet() || item.IsNode())) {
                        item.SetLocked(false);
                    }
                }

                GraphicsManager.Instance.ForceDraw();
            }
        }

        //control shift V for deserializing from local storage
        private static void HandleModShiftV() {
            JsonManager.Instance.DeserializeFromLocalStorage();
        }


        //test map
        private static void HandleModShiftZ() {
            string jsonStr = JsonManager.Instance.TestCanvas();
            JsonManager.Instance.Deserialize(jsonStr);
        }




    }
}
