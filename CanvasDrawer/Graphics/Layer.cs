using System;
using System.Collections.Generic;
using CanvasDrawer.Graphics.Items;
using CanvasDrawer.DataModel;
using System.Linq;
using System.Threading.Tasks;
using CanvasDrawer.Util;

namespace CanvasDrawer.Graphics {
    public class Layer {

        public delegate bool Filter(Item item);

        public Filter ItemFilter { get; set; }

        //the items
        public List<Item> Items { get; set; }

        public string Name { get; set; } = "???";

        public Layer(string name) {
            Name = name;
            Items = new List<Item>();
        }

        public void BringToFront(Item item) {
            if (Items.Remove(item)) {
                Items.Add(item);
                GraphicsManager.Instance.ForceDraw();
            }
        }

        public void SendToBack(Item item) {
            if (Items.Remove(item)) {
                Items.Insert(0, item);
                GraphicsManager.Instance.ForceDraw();
            }
        }

        public void Add(Item item) {
            Items.Add(item);
        }

        /// <summary>
        /// Draw all the items. Check to see if they are visible
        /// and should be drawn.
        /// </summary>
        /// <param name="g"></param>
        public virtual void Draw(Graphics2D g) {
            foreach (Item item in Items) {
                if ((ItemFilter == null) || ItemFilter(item)) {
                    item.Update(g);
                }
            }
        }

        /// <summary>
        /// Are all the items visible? Used to decide whether we can hide the scroll bars.
        /// </summary>
        /// <returns>true if all the items on this layer are visble.</returns>
        public bool AllItemsFullyVisibleOnScreen() {
            foreach (Item item in Items) {
                if (!item.IsFullyVisibleOnScreen()) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Are all the items visible in a horizontal sense? Used to decide whether we can hide the horizontal scroll bar.
        /// </summary>
        /// <returns>true if all the items on this layer are visble in a horizontal.</returns>
        public bool AllItemsFullyHVisibleOnScreen() {
            foreach (Item item in Items) {
                if (!item.IsFullyHVisibleOnScreen()) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Are all the items visible in a vertical sense? Used to decide whether we can hide the vertical scroll bar.
        /// </summary>
        /// <returns>true if all the items on this layer are visble in a vertical sense.</returns>
        public bool AllItemsFullyVVisibleOnScreen() {
            foreach (Item item in Items) {
                if (!item.IsFullyVVisibleOnScreen()) {
                    return false;
                }
            }
            return true;
        }

        //offset the items (after a pan)
        public void OffsetLayer(double dx, double dy) {
            foreach (Item item in Items) {
                item.OffsetItem(dx, dy);
            }
        }

        //Get any item that contains the given point.
        //go backwards (top to bottom)
        public Item ItemAt(double x, double y) {
            for (int itemindex = Items.Count - 1; itemindex >= 0; itemindex--) {
                Item item = Items.ElementAt(itemindex);

                if (item.Contains(x, y)) {
                    return item;
                }
            }

            return null;
        }

        //Get the item from the guid
        public Item FromGuid(string guid) {
            foreach (Item item in Items) {
                if (guid.Equals(item.GuidString())) {
                    return item;
                }
            }

            return null;
        }

        /// <summary>
        /// Get all the items on this layer that fully contained by a rectangle.
        /// Optionally set them to be selected
        /// </summary>
        /// <param name="containedItems">Will hold a list of fully contained items.</param>
        /// <param name="r">The containing rectangle.</param>
        /// <param name="select">If true, also select the contained items.</param>
        public void AddContainedItems(List<Item> containedItems, Rect r, bool select) {
            foreach (Item item in Items) {
                if (r.Contains(item.GetBounds())) {
                    containedItems.Add(item);

                    if (select) {
                        item.Selected = true;
                    }
                }
            }
        }

        /// <summary>
        /// Get all the items on this layer whose bounds intersect a rectangle.
        /// /// </summary>
        /// <param name="intersectingItems">Will hold a list of intersecting items.</param>
        /// <param name="r">The intersecting rectangle.</param>
        public void AddIntersectingItems(List<Item> intersectingItems, Rect r) {
            foreach (Item item in Items) {
                Rect bounds = item.GetBounds();
                if (r.Intersects(bounds)) {
                    intersectingItems.Add(item);
                }
            }
        }

        //delete all items
        public void DeleteAllItems() {
            if (Items.Count > 0) {
                Item[] iarry = Items.ToArray();
                foreach (Item item in iarry) {
                    item.Delete();
                }
                Items.Clear();
            }

        }
        /// <summary>
        /// Are there any selected itemes on this layer?
        /// </summary>
        /// <param name="skipLocked"> if true, ignore locked items </param>
        /// <returns>true if there are any selected items</returns>
        public bool AnySelectedItems(bool skipLocked) {
            foreach (Item item in Items) {

                bool ignore = skipLocked && item.IsLocked();

                if (!ignore && item.Selected) {
                    return true;
                }
            }
            return false;
        }

        //snap all the items to a grid
        public void SnapToGrid() {
            foreach (Item item in Items) {
                item.SnapToGrid();
            }
        }

        //add any selected items on this layer to the list
        public void AddSelectedItems(List<Item> selectedItems) {
            foreach (Item item in Items) {
                if (item.Selected) {
                    selectedItems.Add(item);
                }
            }
        }

        //unselect all items on this layer
        public void UnselectAll() {
            foreach (Item item in Items) {
                item.Selected = false;
            }
        }

        //select all items on this layer
        public void SelectAll() {
            foreach (Item item in Items) {
                item.Selected = true;
            }
        }

        /// <summary>
        /// Find an item on this layer with a matching name.
        /// </summary>
        /// <param name="name">The name to match.</param>
        /// <returns>The first item with the matching name, or null</returns>
        public Item FindByName(string name) {

            if (name == null) {
                return null;
            }

            foreach (Item item in Items) {
                string iname = item.Name();
                if (iname != null) {
                    if (iname.Equals(name)) {
                        return item;
                    }
                }
            }

            return null;
        }

    }
}
