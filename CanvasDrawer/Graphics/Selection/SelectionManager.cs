using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CanvasDrawer.Graphics.Items;
using CanvasDrawer.Graphics.Connection;
using CanvasDrawer.State;
using CanvasDrawer.Util;

namespace CanvasDrawer.Graphics.Selection {
    public sealed class SelectionManager {

        //use delegate for filter
        public delegate bool SelectFilter(EItemType type);

        //use thread safe singleton pattern
        private static SelectionManager? _instance;

        //interested in item selection changes
        private List<ISelectionObserver> _observers;

        //Manage the selection and deselection of items on the map
        SelectionManager() : base() {
            _observers = new List<ISelectionObserver>();
        }

        //accessor to the singleton
        public static SelectionManager Instance {
            get {
				if (_instance == null) {
					_instance = new SelectionManager();
				}
				return _instance;
			}
        }

        /// <summary>
        /// Notify the observers of a selection change.
        /// </summary>
        public void NotifyObservers() {

            List<Item> selectedItems = SelectedItems();
            try {
                foreach (var observer in _observers) {
                    observer.SelectionChange(selectedItems);
                }
            }
            catch (Exception e) {
                System.Console.WriteLine("Exception in SelectionManager notify Observers: " + e.Message);
            }
        }

        //subscribe as an item observer
        public void Subscribe(ISelectionObserver observer) {
            if (!(_observers.Contains(observer))) {
                _observers.Add(observer);
            }
        }

        //unsubscribe as an item observer
        public void Unsubscribe(ISelectionObserver observer) {
            if (_observers.Contains(observer)) {
                _observers.Remove(observer);
            }
        }

		/// <summary>
        /// Select an item programatically.
        /// </summary>
        /// <param name="item">The item to select.</param>
		public void SelectItem(Item item) {
			if (item != null) {
				item.Selected = true;
				NotifyObservers();
			}
		}

        /// <summary>
        /// Respond to a mouse down event.
        /// </summary>
        /// <param name="ue">The mouse event.</param>
        /// <returns></returns>
        public Item UpdateSelection(UserEvent ue) {
            Item item = ItemAtEvent(ue);
            ue.ResizeRectIndex = -1;

            //did not click on item, unselect all and return null
            if (item == null) {
                UnselectAll();
                NotifyObservers();
                return null;
            }

            //item already selected, do nothing except set resize rect
            if (item.Selected) {
                //set the resize rect index for possible resize
                item.SetResizeRectIndex(ue);
                return item;
            }

            //no modifier unselect all
            if (!ue.AnyModifier()) {
                UnselectAll();
            }

            //set the resize index for possible resize
            item.SetResizeRectIndex(ue);

            item.Selected = true;

            NotifyObservers();
            return item;
        }

        /// <summary>
        /// Get all non-connector items completey enclosed by a rect.
        /// </summary>
        /// <param name="ue">The mouse event</param>
        /// <param name="r">The rectangle.</param>
        /// <returns></returns>
        public List<Item> UpdateSelection(UserEvent ue, Rect r) {

            //no modifier, unselect all
            if (!ue.AnyModifier()) {
                UnselectAll();
            }

            List<Item> containedItems = new List<Item>();
            GraphicsManager.NodeLayer.AddContainedItems(containedItems, r, true);
            GraphicsManager.SubnetLayer.AddContainedItems(containedItems, r, true);
            GraphicsManager.AnnotationLayer.AddContainedItems(containedItems, r, true);

            NotifyObservers();
            return containedItems;
        }

        //get the topmost item at the location of the event
        public Item ItemAtEvent(UserEvent ue) {
            return ItemAtEvent(ue, TakeAll);
        }

        private bool TakeAll(EItemType type) {
            return true;
        }

        /// <summary>
        /// Get the topmost item, if any, at the location of a mouse event.
        /// </summary>
        /// <param name="ue">The mouse event.</param>
        /// <param name="filter">To filter on objects searched.</param>
        /// <returns></returns>
        public Item ItemAtEvent(UserEvent ue, SelectFilter filter) {


            //two passes, one for resize rect selection on subnets and connetors

            Item connectorItem  = null;
            Item subnetItem = null;

            //reconnecting a connector?
            if (filter(EItemType.LineConnector)) {
                connectorItem = GraphicsManager.ConnectorLayer.ItemAt(ue.X, ue.Y);

                if (connectorItem != null) {
                    connectorItem.SetResizeRectIndex(ue);

                    if (ue.Type == EUserEventType.SINGLECLICK) {
                        if (ue.ResizeRectIndex >= 0) {
                            ConnectorItem cnx = (ConnectorItem)connectorItem;
                            if (cnx.IsFullyConnected()) {
                                Item anchor = cnx.FartherItem(ue.X, ue.Y);
                                ConnectionManager.Instance.BrokenLinkItem = (anchor == cnx.StartItem) ? cnx.EndItem : cnx.StartItem;

                                cnx.StartItem = anchor;
                                cnx.EndItem = null;
                                ConnectionManager.Instance.Reconnect(cnx, ue.X, ue.Y);
                                
                            }
                        }
                    }
                    return connectorItem;
                }
                
            }

            //resizing a subnet?
            if ((subnetItem == null) && filter(EItemType.NodeBox)) {
                subnetItem = GraphicsManager.SubnetLayer.ItemAt(ue.X, ue.Y);
                if (subnetItem != null) {
                    subnetItem.SetResizeRectIndex(ue);

                    if (ue.ResizeRectIndex >= 0) {
                        return subnetItem;
                    }
                }
            }

            Item hitItem = null;

            if ((hitItem == null) && (filter(EItemType.Node))) {
                hitItem = GraphicsManager.NodeLayer.ItemAt(ue.X, ue.Y);
            }

           if ((hitItem == null) && filter(EItemType.NodeBox)) {
                //have we already detected a subnet hit?
                if (subnetItem != null) {
                    return subnetItem;
                }

                hitItem = GraphicsManager.SubnetLayer.ItemAt(ue.X, ue.Y);
            }


            if ((hitItem == null) && (filter(EItemType.LineConnector))) {
                //have we already detected a connector hit?
                if (connectorItem != null) {
                    return connectorItem;
                }

                hitItem = GraphicsManager.ConnectorLayer.ItemAt(ue.X, ue.Y);
            }

            if ((hitItem == null) && filter(EItemType.Annotation)) {
                hitItem = GraphicsManager.AnnotationLayer.ItemAt(ue.X, ue.Y);
            }

            return hitItem;
        }

        /// <summary>
        /// Are there any selected itemes on this layer?
        /// </summary>
        /// <param name="skipLocked"> if true, ignore locked items </param>
        /// <returns>true if there are any selected items</returns>
        public bool AnySelectedItems(bool skipLocked) {

            bool anySelected = GraphicsManager.AnnotationLayer.AnySelectedItems(skipLocked);
            anySelected = anySelected || GraphicsManager.NodeLayer.AnySelectedItems(skipLocked);
            anySelected = anySelected || GraphicsManager.SubnetLayer.AnySelectedItems(skipLocked);
            anySelected = anySelected || GraphicsManager.ConnectorLayer.AnySelectedItems(skipLocked);

            return anySelected;
        }

        //Get all the selected items
        public List<Item> SelectedItems() {
            List<Item> list = new List<Item>();

            GraphicsManager.SubnetLayer.AddSelectedItems(list);
            GraphicsManager.NodeLayer.AddSelectedItems(list);
            GraphicsManager.ConnectorLayer.AddSelectedItems(list);
            GraphicsManager.AnnotationLayer.AddSelectedItems(list);
            return list;
        }

        //get just the selected nodes
        public List<Item> SelectedNodes() {
            List<Item> list = new List<Item>();

            GraphicsManager.NodeLayer.AddSelectedItems(list);
            return list;
        }

        /// <summary>
        /// Set all the items to unselected.
        /// </summary>
        public void UnselectAll() {
            GraphicsManager.NodeLayer.UnselectAll();
            GraphicsManager.SubnetLayer.UnselectAll();
            GraphicsManager.AnnotationLayer.UnselectAll();
            GraphicsManager.ConnectorLayer.UnselectAll();
        }

        /// <summary>
        /// Set all the items to selected.
        /// </summary>
        public void SelectAll() {
			GraphicsManager.NodeLayer.SelectAll();
			GraphicsManager.SubnetLayer.SelectAll();
			GraphicsManager.ConnectorLayer.SelectAll();
			GraphicsManager.AnnotationLayer.SelectAll();
			GraphicsManager.Instance.ForceDraw();
		}

    }
}
