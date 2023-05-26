using System;
using CanvasDrawer.DataModel;
using CanvasDrawer.Graphics.Items;
using CanvasDrawer.Graphics.Popup;
using CanvasDrawer.Graphics.Selection;
using CanvasDrawer.Graphics.Toolbar;
using CanvasDrawer.Pages;
using CanvasDrawer.State;
using CanvasDrawer.Graphics.Editor;


namespace CanvasDrawer.Graphics.Connection
{
	public sealed class ConnectionManager : IItemObserver
	{

		private static readonly string MAKECONNECTIONCOLOR = "#CC1111";

		public Item? StartItem { get; set; }
		public Item? EndItem { get; set; }

		public DoublePoint? _prevPoint;

		private EConnectionType? _connectionType;

		//the item you broke from when reconnecting.
		public Item? BrokenLinkItem { get; set; }

		private static ConnectionManager? _instance;

		//have to cache the line color for reconnects
		private string? _reconnectLineColor;

		ConnectionManager() : base()
		{
			ItemManager.Instance.Subscribe(this);
		}

		//public accessor for the singleton
		public static ConnectionManager Instance {
			get {
				if (_instance == null) {
					_instance = new ConnectionManager();
				}
				return _instance;
			}
		}

		/// <summary>
		/// Change the line color of all selected connectors.
		/// </summary>
		/// /// <param name="item">The connector being chnged</param>
		/// <param name="color">The new line color.</param>
		public void ChangeColor(LineConnectorItem item, string color)
		{

			if (item != null) {
				item.SetForeground(color);
			}

			//also change any selected connectors
			ChangeColorOfSelectedConnectors(color);
		}

		/// <summary>
		/// Change the line color of all selected connectors.
		/// </summary>
		/// <param name="color">The new line color.</param>
		public void ChangeColorOfSelectedConnectors(string color)
		{
			List<Item> cnxList = GraphicsManager.ConnectorLayer.Items;

			if (cnxList != null) {
				foreach (Item item in cnxList) {
					if (item.Selected) {
						item.SetForeground(color);
					}
				}
			}
			DirtyManager.Instance.SetDirty("Color change");
			PaletteEditor.Instance.Restore();
			GraphicsManager.Instance.ForceDraw();
		}

		/// <summary>
		/// Reconnect a connection, i.e. braking one link and allow it to graphically reconnect.
		/// </summary>
		/// <param name="item">The connector that will be reconnected.</param>
		/// <param name="x">Current horizontal mouse position.</param>
		/// <param name="y">Current vertical mouse position.</param>
		public void Reconnect(ConnectorItem item, double x, double y)
		{
			//if we get here we are breaking a connection and reconnecting.
			StartItem = item.StartItem;

			//need to save line color
			Property prop = item.Properties.GetProperty(DefaultKeys.FG_COLOR);

			if (prop.Value != null) {
				_reconnectLineColor = (string)prop.Value.Clone();
			}
			item.Delete();

			GraphicsManager.Instance.ForceDraw();
			InitConnection(EConnectionType.LINE);
			StateMachine.Instance.CurrentState = EState.Reconnect;
			ToolbarManager.Instance.SelectButton("connector");
			ToolbarManager.Instance.RefresherTools();
		}

		/// <summary>
		/// Initialize the connection.
		/// </summary>
		/// <param name="etype">The type of connection.</param>
		public void InitConnection(EConnectionType etype)
		{
			_connectionType = etype;
			if (JSInteropManager.Instance != null) {
				JSInteropManager.Instance.SaveCanvasInBackgoundImage();
			}
		}

		/// <summary>
		/// Update the connector as a result of a mouse move.
		/// </summary>
		/// <param name="ue">The mouse event.</param>
		public void UpdateConnector(UserEvent ue)
		{
			if (_prevPoint == null) {
				_prevPoint = new DoublePoint();
			} else {
				//restore the old

				if (StartItem != null) {
					DoublePoint foc = StartItem.GetFocus();
					Rect r = new Rect(foc, _prevPoint);

					JSInteropManager? jsm = JSInteropManager.Instance;
					if (jsm == null) {
						return;
					}
					jsm.RestoreCanvasFromBackgroundImage();

					//draw the new

					jsm.DrawLine(foc.X, foc.Y, ue.X, ue.Y, MAKECONNECTIONCOLOR, 2, 5);
				}

			}
			_prevPoint.Set(ue.X, ue.Y);
		}


		public void Reset()
		{
			StartItem = null;
			EndItem = null;
			_prevPoint = null;
			BrokenLinkItem = null;
		}

		/// <summary>
		/// Create a Connector
		/// </summary>
		public void MakeConnection()
		{

			if (EndItem == null) {
				EndItem = BrokenLinkItem;
			}

			if (AreConnected(StartItem, EndItem)) {
				EndItem = BrokenLinkItem;
			}

			BrokenLinkItem = null;

			if ((StartItem != null) && (EndItem != null) && (StartItem != EndItem) &&
				!AreConnected(StartItem, EndItem)) {

				switch (_connectionType) {
					case EConnectionType.LINE:
						LineConnectorItem cnx = new LineConnectorItem(GraphicsManager.ConnectorLayer, StartItem, EndItem);

						if (StateMachine.Instance.CurrentState == EState.Reconnect && (_reconnectLineColor != null)) {
							Property prop = cnx.Properties.GetProperty(DefaultKeys.FG_COLOR);
							prop.Value = (string)_reconnectLineColor.Clone();
						}
						break;

				}

				Reset();
			}
		}

		/// <summary>
		/// Find the connector between the two items, or null if none.
		/// </summary>
		/// <param name="item1">One item of the potential connection.</param>
		/// <param name="item2">Other item of the potential connection.</param>
		/// <returns>The connector item, or null if the items are not connected,</returns>
		public ConnectorItem? GetConnector(Item item1, Item item2)
		{

			foreach (Item item in GetAllConnections()) {

				if (item != null) {

					ConnectorItem citem = (ConnectorItem)item;

					if ((citem.StartItem == null) || (citem.EndItem == null)) {
						continue;
					}
					Item startItem = citem.StartItem;
					Item endItem = citem.EndItem;

					if ((item1 == startItem) && (item2 == endItem)) {
						return citem;
					}
					if ((item2 == startItem) && (item1 == endItem)) {
						return citem;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Check if these two items are connected.
		/// </summary>
		/// <param name="item1">One item of the potential connection.</param>
		/// <param name="item2">Other item of the potential connection.</param>
		/// <returns>true if the items are connected.</returns>
		public bool AreConnected(Item? item1, Item? item2)
		{

			if ((item1 == null) || (item2 == null)) {
				return false;
			}
			return GetConnector(item1, item2) != null;
		}


		/// <summary>
		/// Get all the connections in the model.
		/// </summary>
		/// <returns>ll the connections in the model.</returns>
		public List<Item> GetAllConnections()
		{
			return GraphicsManager.ConnectorLayer.Items;
		}

		//an item has changed
		//if deleted. need to remove any connections
		public void ItemChangeEvent(ItemEvent e)
		{
			switch (e.Type) {
				case EItemChange.DELETED:

					if (e.Item.IsConnector()) {
						return;
					}

					//delete all the connectors connected to this item
					Item[] connections = GetAllConnections().ToArray();

					foreach (Item connector in connections) {
						if (connector.Contains(e.Item)) {
							connector.Delete();
						}
					}
					break;
			}
		}
	}
}

