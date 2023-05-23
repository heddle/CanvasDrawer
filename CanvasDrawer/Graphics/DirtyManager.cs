using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using CanvasDrawer.Graphics.Items;

namespace CanvasDrawer.Graphics {

    /*
     * The DirtyManager keeps track of whether the map needs to be saved
     * Things that make the map dirty:
     *  1) Adding an item
     *  2) Deleting an Item;
     *  3) Dragging an item
     *  4) Editing an item's properties
     */
    public class DirtyManager : IItemObserver {

        //use thread safe singleton pattern
        private static DirtyManager _instance;
        private static readonly object _padlock = new object();

        //is the map dirty (needs to be saved)
        public bool IsDirty { get; private set; } = false;

        public String LastReason { get; private set; } = "";

        DirtyManager() : base() {
            ItemManager.Instance.Subscribe(this);
        }

        /// <summary>
        /// Public access for DirtyManager singleton.
        /// </summary>
        public static DirtyManager Instance {
            get {
                lock (_padlock) {
                    if (_instance == null) {
                        _instance = new DirtyManager();
                    }
                    return _instance;
                }
            }
        }

        //only called as a result of a save, unless we implement a
        //version of undo or restore.
        public void SetClean() {
            IsDirty = false;
         }

        public void SetDirty(string shortReason) {
            LastReason = shortReason;
            IsDirty = true;
        }

        public void ItemChangeEvent(ItemEvent e) {
            SetDirty("Item modification");
        }

        public override String ToString() {
            if (IsDirty) {
                return "Map needs saving, last change: " +  LastReason;
            }
            else {
                return "Map does not need saving";
            }
        }
    }
}
