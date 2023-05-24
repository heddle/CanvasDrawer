using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CanvasDrawer.DataModel;

namespace CanvasDrawer.Graphics.Items {
    public sealed class ItemManager {


        //use thread safe singleton pattern
        private static ItemManager? _instance;
        private static readonly object _padlock = new object();

        //interested in item events
        private List<IItemObserver> _observers;

        public ItemManager() : base() {
            _observers = new List<IItemObserver>();
        }

        //public access to singleton
        public static ItemManager Instance {
            get {
                lock (_padlock) {
                    if (_instance == null) {
                        _instance = new ItemManager();
                    }
                    return _instance;
                }
            }
        }

        //notify item observers
        public void NotifyObservers(ItemEvent ue) {

            if (_observers == null) {
                return;
            }

            try {
                foreach (var observer in _observers) {
                    observer.ItemChangeEvent(ue);
                }
            }
            catch (Exception e) {
                System.Console.WriteLine("Exception in ItemManager NotifyObservers: " + e.Message);
            }
       }

        //subscribe as an item observer
        public void Subscribe(IItemObserver observer) {

            if (!(_observers.Contains(observer))) {
                _observers.Add(observer);
            }
        }

        //unsubscribe as an item observer
        public void Unsubscribe(IItemObserver observer) {
            if (_observers.Contains(observer)) {
                _observers.Remove(observer);
            }
        }

        /// <summary>
        /// Find an item from its GUID
        /// </summary>
        /// <param name="guid">The GUID to match.</param>
        /// <returns></returns>
        public Item FromGuid(string guid) {
            Item item = GraphicsManager.NodeLayer.FromGuid(guid);
            if (item == null) {
                item = GraphicsManager.SubnetLayer.FromGuid(guid);
            }
            if (item == null) {
                item = GraphicsManager.ConnectorLayer.FromGuid(guid);
            }

            return item;
        }

        /// <summary>
        /// Get a list of all items on all layers.
        /// </summary>
        /// <returns>a list of all items on all layers.</returns>
        public static List<Item> GetAllItems() {
            List<Item> items = new List<Item>();
            items.AddRange(GraphicsManager.ConnectorLayer.Items);
            items.AddRange(GraphicsManager.SubnetLayer.Items);
            items.AddRange(GraphicsManager.NodeLayer.Items);
            items.AddRange(GraphicsManager.AnnotationLayer.Items);
            return items;
        }

        /// <summary>
        /// Get the data model, which is just a collection of all properties for all models
        /// </summary>
        /// <returns>The data model, i.e., a collection of all Properties for all items.</returns>
        public List<Properties> GetModel() {
            List<Properties> model = new List<Properties>();
            List<Item> items = GetAllItems();
            foreach (Item item in items) {
                model.Add(item.Properties);
            }
            return model;
        }

    }
}
