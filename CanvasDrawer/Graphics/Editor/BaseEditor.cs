using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CanvasDrawer.Graphics.Items;

namespace CanvasDrawer.Graphics.Editor {
    public abstract class BaseEditor : IItemObserver {

        //used to update the map
        public delegate void Refresh();
        public Refresh Refresher { get; set; }

        // delegate will be assigned to set the item being edited
        public delegate void HotItem(Item item);
        public HotItem SetHotItem { get; set; }

        //item being edited
        protected Item _hotItem;


        public BaseEditor() : base() {
            ItemManager.Instance.Subscribe(this);
        }

        public abstract void Restore();

        public void ItemChangeEvent(ItemEvent e) {
            switch (e.Type) {
                case EItemChange.DELETED:
                    if (e.Item == _hotItem) {
                        Restore();
                    }
                    break;
            }
        }

        //get the item being edited
        public Item GetHotItem() {
            return _hotItem;
        }

        protected void RefreshAllEditors() {
            PropertyEditor.Instance.Refresher();
            PaletteEditor.Instance.Refresher();
            TextColorEditor.Instance.Refresher();
        }

    }
}
