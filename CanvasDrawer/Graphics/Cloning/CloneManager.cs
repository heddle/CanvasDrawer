using System;
using CanvasDrawer.Graphics.Connection;
using CanvasDrawer.Graphics.Items;
using CanvasDrawer.Graphics.Selection;
namespace CanvasDrawer.Graphics.Cloning
{
	public sealed class CloneManager
	{

		//used in duplication offset;
		private static int _dupCount;
		private static double _dupDel = 30;

		/// <summary>
		/// This duplicates everything that is selected.
		/// If a subnet is selected, all the nodes it contains are duplicated,
		/// even if they are not selected. Connections are ignored (sort of). If both ends are duplicated,
		/// a connection of the same type will be established.
		/// </summary>
		public static void DuplicateSelectedItems()
		{
			//get the selected nodes
			List<Item> items = SelectionManager.Instance.SelectedItems();
			if ((items == null) || (items.Count < 1)) {
				return;
			}

			//remove those in subnets that will get duplicated anyway regardless
			//of whether they were selected. This avoids double copies.
			List<Item> inSelectedSubnet = new List<Item>();

			//first find them
			foreach (Item item in items) {
				//ignore connectors. They will be "fixed" later
				if (item.IsConnector()) {
					continue;
				}

				if (item.IsSubnet()) {
					SubnetItem subnet = (SubnetItem)item;
					List<Item> containedItems = subnet.GetAllContainedNodes();
					if (containedItems != null) {
						foreach (Item conItem in containedItems) {
							inSelectedSubnet.Add(conItem);
						}
					}
				}
			} // for

			//now remove those contained in subnets
			foreach (Item item in inSelectedSubnet) {
				items.Remove(item);
			}

			//now duplicate
			if (items.Count > 0) {

				int subCount = 1 + (_dupCount % 4);
				int xsign = (_dupCount < 8) ? 1 : -1;
				int ysign = (((_dupCount / 4) % 2) == 0) ? 1 : -1;

				double dx = xsign * subCount * _dupDel;
				double dy = ysign * subCount * _dupDel;
				foreach (Item item in items) {
					item.Duplicate(dx, dy);
				}

				_dupCount = (_dupCount + 1) % 16;

				GraphicsManager.Instance.ForceDraw();
			}

			//create necessary conections
			Item[] allConnections = ConnectionManager.Instance.GetAllConnections().ToArray();
			foreach (Item item in allConnections) {
				ConnectorItem connector = (ConnectorItem)item;
				Item startItem = connector.StartItem.TempClone;
				Item endItem = ((ConnectorItem)item).EndItem.TempClone;

				if ((startItem != null) && (endItem != null)) {
					if (connector.IsLineConnector()) {
						LineConnectorItem line = new LineConnectorItem(GraphicsManager.Instance.ConnectorLayer, startItem, endItem);
						line.SetForeground(connector.GetForeground());
					}
				}
			}

			//reset the tempClone fields
			foreach (Item item in ItemManager.GetAllItems()) {
				item.TempClone = null;
			}

			DirtyManager.Instance.SetDirty("Duplication");
			GraphicsManager.Instance.FullRefresh();
		} //Duplicate items
	}
}

