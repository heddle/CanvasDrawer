using System;
using System.Collections.Generic;
using CanvasDrawer.Pages;
using CanvasDrawer.Graphics.Toolbar;
using CanvasDrawer.State;
using CanvasDrawer.Graphics.Rubberband;
using CanvasDrawer.Graphics.Selection;
using CanvasDrawer.Graphics.Feedback;
using CanvasDrawer.Graphics.Dragging;
using CanvasDrawer.Graphics.Items;
using CanvasDrawer.Graphics.Connection;
using CanvasDrawer.Graphics.Reshape;
using CanvasDrawer.Graphics.Editor;
using CanvasDrawer.Graphics.Theme;
using CanvasDrawer.Graphics.Popup;
using CanvasDrawer.Graphics.Hover;
using CanvasDrawer.Util;

namespace CanvasDrawer.Graphics {
    public sealed class GraphicsManager {

        //map name
        public String DrawingName { get; set; } = "MyCanvas";

        //snap to grid
        public static readonly int VirtualGridSize = 15;

        //for things like panning
        private UserEvent _lastEvent = new UserEvent();

        //use thread safe singleton pattern
        private static GraphicsManager? _instance;
        private static readonly object _padlock = new object();

        //the visual rect, used to check if something neets to be drawn
        public Rect VisualRect { get; set; }

        //the confining rect, encloses all items
        public Rect ConfiningRect { get; set; }

        //used to refresh
        public delegate void MapRefresh();
        public MapRefresh CanvasRefresher { get; set; }

        //the shared graphics 2D object
        public Graphics2D G2D { get; set; }

        //the layers
        public static Layer ConnectorLayer { get; private set; } = new Layer("Connectors");
        public static Layer NodeLayer { get; private set; } = new Layer("Nodes");
        public static Layer SubnetLayer { get; private set; } = new Layer("Boxes");
        public static Layer AnnotationLayer { get; private set; } = new Layer("Annotations");


    //array of all layers
    private Layer[] _layers = { ConnectorLayer, NodeLayer, SubnetLayer, AnnotationLayer };

        //the page manager
        public PageManager PageManager { get; set; }

        GraphicsManager() : base() {
            G2D = new Graphics2D();
        }

        //public access to the singleton
        public static GraphicsManager Instance {
            get {
                lock (_padlock) {
                    if (_instance == null) {
                        _instance = new GraphicsManager();
                    }
                    return _instance;
                }
            }
        }

        /// <summary>
        /// Get the rectangle that encloses all items.
        /// </summary>
        /// <returns>The rectangle that encloses all items.</returns>
        public static Rect Confines() {
            List<Item> items = ItemManager.GetAllItems();
            if ((items == null) || (items.Count < 1)) {
                return null;
            }

            double left = Double.PositiveInfinity;
            double top = Double.PositiveInfinity;
            double right = Double.NegativeInfinity;
            double bottom = Double.NegativeInfinity;

            foreach (Item item in items) {
                Rect b = item.GetTotalBounds();

                left = Math.Min(left, b.X);
                top = Math.Min(top, b.Y);
                right = Math.Max(right, b.Right());
                bottom = Math.Max(bottom, b.Bottom());
            }
            
            return new Rect(left, top, right - left, bottom - top);
        }

        /// <summary>
        /// The top-level "draw everything" method. 
        /// </summary>
        public void DrawModel() {
            //so we can minimize drawing
            VisualRect = PageManager.VisualRect();

            //draw the grid?
            if (DisplayManager.Instance.ShowGrid) {
                DrawGrid(G2D);
            }

            //draw in a specific ordering

            ConnectorLayer.ItemFilter = NoSubnet;
            ConnectorLayer.Draw(G2D);

            SubnetLayer.Draw(G2D);

            ConnectorLayer.ItemFilter = YesSubnet;
            ConnectorLayer.Draw(G2D);

            ConnectorLayer.ItemFilter = null;

            //now the nodes
            NodeLayer.Draw(G2D);

            //now the annotations
            AnnotationLayer.Draw(G2D);

            //resize rects
            DrawResizeRects(G2D);

            //lastly see if there is a hover
            HoverManager.Instance.DrawHover(G2D);
        }

        /// <summary>
        /// Are all the items fully visible? Used to decide whether we can hide the scroll bars.
        /// </summary>
        /// <returns>true if all the items on all layers are visble.</returns>
        public bool AllItemsFullyVisibleOnScreen() {
            foreach (Layer layer in GetAllLayers()) {
                if (!layer.AllItemsFullyVisibleOnScreen()) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Are all the items fully visible horizontally? Used to decide whether we can hide the horizontal scroll bar.
        /// </summary>
        /// <returns>true if all the items on all layers are visble in a horizontal sense.</returns>
        public bool AllItemsFullyHVisibleOnScreen() {
            foreach (Layer layer in GetAllLayers()) {
                if (!layer.AllItemsFullyHVisibleOnScreen()) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Are all the items fully visible vertically? Used to decide whether we can hide the vertical scroll bar.
        /// </summary>
        /// <returns>true if all the items on all layers are visble in a vertical sense.</returns>
        public bool AllItemsFullyVVisibleOnScreen() {
            foreach (Layer layer in GetAllLayers()) {
                if (!layer.AllItemsFullyVVisibleOnScreen()) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Draw the resize rects
        /// </summary>
        private void DrawResizeRects(Graphics2D g) {
            List<Item> selectedItems = SelectionManager.Instance.SelectedItems();

            if (selectedItems != null) {
                foreach (Item item in selectedItems) {
                    item.DrawResizeRects(g);
                }
            }

        }

        //filter on connectors with a subnet endpoint
        private bool YesSubnet(Item item) {
            return ((ConnectorItem)item).ConnectedToSubnet();
        }

        //filter on connectors without a subnet endpoint
        private bool NoSubnet(Item item) {
            return !YesSubnet(item);
        }

        /// <summary>
        /// Draw the background grid. Trying to make it 1cm by 1cm
        /// </summary>
        /// <param name="g"></param>
        private void DrawGrid(Graphics2D g) {

            if (JSInteropManager.Instance != null) {

                double del = 96 * JSInteropManager.Instance.GetDPI() / 2.54;
                Rect r = PageManager.CanvasBounds();

                g.LineColor = ThemeManager.CanvasGridColor;
                g.LineStyle = ELineStyle.SOLID;
                g.LineWidth = 0;

                if (JSInteropManager.Instance != null) {
                    DoublePoint? scale = JSInteropManager.Instance.Scale;

                    if (scale != null) {
                        double delx = del / scale.X;
                        double dely = del / scale.Y;

                        int nrow = (int)Math.Ceiling(r.Height / dely);

                        for (int row = 1; row <= nrow; row++) {
                            double y = r.Y + dely * row;
                            g.DrawLine(r.X, y, r.Right(), y);
                        }

                        int ncol = (int)Math.Ceiling(r.Width / delx);

                        for (int col = 1; col <= ncol; col++) {
                            double x = r.X + delx * col;
                            g.DrawLine(x, r.Y, x, r.Bottom());
                        }
                    }
                }
            }
        }


        //have received an all important user interaction
        public void UserEvent(UserEvent ue) {

            //notify the hover manager
            HoverManager.Instance.UserEvent(ue);

            switch (ue.Type) {
                case EUserEventType.MOUSEMOVE:
                    HandleMouseMove(ue);
                    break;

                case EUserEventType.MOUSEEXIT:
                    HandleMouseExit(ue);
                    break;

                case EUserEventType.MOUSEDOWN:
                    if (!PopupManager.Instance.ConsumedMouseButtonEvent(ue)) {
                        if (ue.Button == 0) {
                            HandleMouseDown(ue);
                        }
                    }
                    FixState();
                    break;

                case EUserEventType.MOUSEUP:
                    if (!PopupManager.Instance.ConsumedMouseButtonEvent(ue)) {
                        if (ue.Button == 0) {
                            HandleMouseUp(ue);
                        }
                    }
                    FixState();
                    break;

                case EUserEventType.SINGLECLICK:
                    if (!PopupManager.Instance.ConsumedMouseButtonEvent(ue)) {
                        if (ue.Button == 0) {
                            HandleSingleClick(ue);
                        }
                    }

                    break;
            }
        }

        /// <summary>
        /// A single click has occurred.
        /// </summary>
        /// <param name="ue">The mouse event.</param>
        private void HandleSingleClick(UserEvent ue) {
            if (ToolbarManager.Instance.SelectedButton == EToolbarButton.TEXT) {
                TextItem textItem = new TextItem(AnnotationLayer, ue.X, ue.Y);

                //auto bring up editor for new text item
                if (!DisplayManager.Instance.IsPropertyEditorVisible()) {
                    DisplayManager.Instance.ToggleEditorDisplay();
                }

                SelectionManager.Instance.SelectItem(textItem);
                PropertyEditor.Instance.HandleSingleClick(textItem);
                Refresh();
                RestoreDefault();
            }
            else {
                Item item = SelectionManager.Instance.ItemAtEvent(ue);
                HandleSingleClickOnItem(item);
            }
        }

        /// <summary>
        /// Handle the single click on an item.
        /// </summary>
        /// <param name="item">The item that was clicked, might be null.</param>
        public void HandleSingleClickOnItem(Item item) {
            PropertyEditor.Instance.HandleSingleClick(item);
            PaletteEditor.Instance.HandleSingleClick(item);
            ForceDraw();
        }


        //Fix the selection states
        private void FixState() {
            bool anySelected = SelectionManager.Instance.AnySelectedItems(false);
            bool anySelectedIgnore = anySelected && SelectionManager.Instance.AnySelectedItems(true);

            ToolbarManager.Instance.SetEnabled(EToolbarButton.DELETEITEMS, anySelectedIgnore);
            ToolbarManager.Instance.SetEnabled(EToolbarButton.DUPLICATEITEMS, anySelected);
        }

        /// <summary>
        /// Handle the mouse move event
        /// </summary>
        /// <param name="currentEvent">The mose event.</param>
        private void HandleMouseMove(UserEvent currentEvent) {

            if (DisplayManager.Instance.IsFeedbackVisible() && !currentEvent.AnyModifier()) {
                FeedbackManager.Instance.Update(currentEvent);
            }

            //what we do now depends on the state
            switch (StateMachine.Instance.CurrentState) {

                case EState.Drag:
                    DragManager.Instance.UpdateDragging(currentEvent);
                    ForceDraw();
                    SharedTimer.Instance.RedrawPending = true;
					break;

                case EState.Reshape:
					ReshapeManager.Instance.UpdateReshape(currentEvent);
					Refresh();
					break;

                case EState.Banding:
					RubberbandManager.Instance.UpdateRubberbanding(currentEvent);
					break;

                case EState.Connect:
                case EState.Reconnect:
                    ConnectionManager.Instance.UpdateConnector(currentEvent);
                    break;

                case EState.Pan:

					JSInteropManager? jsm = JSInteropManager.Instance;
                    if (jsm != null) {

                        double dx = (currentEvent.X - _lastEvent.X);
                        double dy = (currentEvent.Y - _lastEvent.Y);

                        double cw = jsm.CanvasWidth;
                        double ch = jsm.CanvasHeight;

                        //don't pan off canvas limits
                        if (((ConfiningRect.X + dx) < 0) || ((ConfiningRect.Right() + dx) > cw)) {
                            _lastEvent.Set(currentEvent);
                            return;
                        }
                        if (((ConfiningRect.Y + dy) < 0) || ((ConfiningRect.Bottom() + dy) > ch)) {
                            _lastEvent.Set(currentEvent);
                            return;
                        }

                        ConfiningRect.Move(dx, dy);
                        if ((Math.Abs(dx) > 2) || ((Math.Abs(dy) > 2))) {
                            foreach (Layer layer in GetAllLayers()) {
                                layer.OffsetLayer(dx, dy);
                                _lastEvent.Set(currentEvent);
                            }
                        }

                        PageManager.IsDirty = true;
                        ForceDraw();
                    }
					break;

                default:
                    break;
            }

        }

        /// <summary>
        /// Handle a mousedown event.  We initiate most things on mouse down,
        /// not on a single click.
        /// </summary>
        /// <param name="currentEvent">The current mouse event.</param>
        private void HandleMouseDown(UserEvent currentEvent) {

            _lastEvent.Set(currentEvent);

            switch (ToolbarManager.Instance.SelectedButton) {

                //default pointer button
                case EToolbarButton.POINTER:

                    Item selectedItem = SelectionManager.Instance.UpdateSelection(currentEvent);

                    //if we haven't clicked on anything, rubberband for multiple select
                    if (selectedItem == null) {
                        StateMachine.Instance.CurrentState = EState.Banding;
                        RubberbandManager.Instance.InitRubberbanding(ERubberbandMode.RECTANGLEDRAG, currentEvent, 1);
                    }
                    else {
                        if (!selectedItem.IsLocked()) {
                            //if a box or text and good select rect, we are resizing, else dragging
                            if (selectedItem.IsSubnet() && (currentEvent.ResizeRectIndex >= 0)) {
                                Refresh();
                                StateMachine.Instance.CurrentState = EState.Reshape;
                                ReshapeManager.Instance.InitReshape(selectedItem, currentEvent);
                            }
                            else {
                                Refresh();
                                StateMachine.Instance.CurrentState = EState.Drag;
                                DragManager.Instance.InitDragging(currentEvent);
                            }
                        }
                    }
                    break;

                case EToolbarButton.CONNECTOR:
                    Item item = SelectionManager.Instance.ItemAtEvent(currentEvent, NoConnectors);
                    if (item == null) {
                        item = ConnectionManager.Instance.BrokenLinkItem;
                        ConnectionManager.Instance.BrokenLinkItem = null;
                    }


                    if (item == null) {
                        ConnectionManager.Instance.Reset();
                        RestoreDefault();
                    }
                    else if (item is TextItem) { //cannot connect to text
                        ConnectionManager.Instance.Reset();
                        RestoreDefault();
                    }
                    else {
                        if (ConnectionManager.Instance.StartItem == null) {
                            ConnectionManager.Instance.StartItem = item;
                            StateMachine.Instance.CurrentState = EState.Connect;

                            EConnectionType etype = EConnectionType.LINE;

                            ConnectionManager.Instance.InitConnection(etype);
                        }
                        else {
                            //cannot connect to self
                            if (item == ConnectionManager.Instance.StartItem) {

                                if (JSInteropManager.Instance != null) {
                                    JSInteropManager.Instance.Alert("Cannot connect an item to itself.");
                                }
                                ConnectionManager.Instance.Reset();
                                RestoreDefault();
                            }
                            else {
                                ConnectionManager.Instance.EndItem = item;
                                ConnectionManager.Instance.MakeConnection();
                                Refresh();
                                RestoreDefault();
                            }
                        }

                    }
                    break;

                case EToolbarButton.SUBNET:
                    //StateMachine.Instance.CurrentState = EState.Banding;
                    //int currentShape = SubnetShapeMenu.Instance.CurrentSelection;

                    //if (currentShape == SubnetShapeMenu.RECTANGLE) {
                    //    RubberbandManager.Instance.InitRubberbanding(ERubberbandMode.RECTANGLEDRAG, currentEvent, 0);
                    //}
                    //else if (currentShape == SubnetShapeMenu.ELLIPSE) {
                    //    RubberbandManager.Instance.InitRubberbanding(ERubberbandMode.ELLIPSE, currentEvent, 0);
                    //}
                    //else if (currentShape == SubnetShapeMenu.CLOUD) {
                    //    RubberbandManager.Instance.InitRubberbanding(ERubberbandMode.CLOUD, currentEvent, 0);
                    //}


                    break;



                case EToolbarButton.WebService:
                case EToolbarButton.Router:
                case EToolbarButton.Wireless:
                case EToolbarButton.PLC:
                case EToolbarButton.Workstation:
                case EToolbarButton.Server:
                case EToolbarButton.Switch:
                case EToolbarButton.Laptop:
                case EToolbarButton.Cloud:
                case EToolbarButton.Printer:
                case EToolbarButton.Firewall:
                case EToolbarButton.Mirror:
                    CreateIconItem(currentEvent.X, currentEvent.Y, ToolbarManager.Instance.SelectedButton);
                    Refresh();
                    RestoreDefault();
                    break;

                case EToolbarButton.PAN:
                    StateMachine.Instance.CurrentState = EState.Pan;
                    ConfiningRect = Confines();
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// A filter that excludes connectors.
        /// </summary>
        /// <param name="type">The item type.</param>
        /// <returns>true if the type is not a coonector</returns>
        private bool NoConnectors(EItemType type) {
            return type != EItemType.LineConnector;
        }

        /// <summary>
        /// A filter that only passes nodes.
        /// </summary>
        /// <param name="type">The item type.</param>
        /// <returns>true if the type is a node.</returns>
        private bool NodesOnly(EItemType type) {
            return (type == EItemType.Node);
        }

        //handle a mouse up event
        private void HandleMouseUp(UserEvent ue) {
            switch (ToolbarManager.Instance.SelectedButton) {

                case EToolbarButton.PAN:
                    StateMachine.Instance.CurrentState = EState.Idle;
                    //        RestoreDefault();
                    break;

                case EToolbarButton.POINTER:
                    if (StateMachine.Instance.CurrentState == EState.Banding) {
                        UserEvent startEvent = RubberbandManager.Instance.EndRubberbanding();

                        double w = ue.X - startEvent.X;
                        double h = ue.Y - startEvent.Y;
                        SelectionManager.Instance.UpdateSelection(ue, new Rect(startEvent.X, startEvent.Y, w, h));
                        RestoreDefault();
                        Refresh();
                    }
                    else if (StateMachine.Instance.CurrentState == EState.Reshape) {
                        ReshapeManager.Instance.EndReshape();
                        RestoreDefault();
                        DirtyManager.Instance.SetDirty("Reshape");
                        Refresh();
                    }
                    else if (StateMachine.Instance.CurrentState == EState.Drag) {
                        RestoreDefault();
                        DirtyManager.Instance.SetDirty("Drag");
                        Refresh();
                    }
                    break;

                case EToolbarButton.SUBNET:

                    if (StateMachine.Instance.CurrentState != EState.Banding) {
                        StateMachine.Instance.CurrentState = EState.Banding;
                        int currentShape = SubnetShapeMenu.Instance.CurrentSelection;

                        if (currentShape == SubnetShapeMenu.RECTANGLE) {
                            RubberbandManager.Instance.InitRubberbanding(ERubberbandMode.RECTANGLEDRAG, ue, 0);
                        }
                        else if (currentShape == SubnetShapeMenu.ELLIPSE) {
                            RubberbandManager.Instance.InitRubberbanding(ERubberbandMode.ELLIPSE, ue, 0);
                        }
                        else if (currentShape == SubnetShapeMenu.CLOUD) {
                            RubberbandManager.Instance.InitRubberbanding(ERubberbandMode.CLOUD, ue, 0);
                        }
                    }

                    else { //banding

                        RubberbandManager.Instance.ClickCount++;

                        if (RubberbandManager.Instance.ClickCount > 0) {
                            UserEvent startEvent = RubberbandManager.Instance.EndRubberbanding();

                            //create a box item from the two events
                            CreateBoxItem(startEvent.X, startEvent.Y, ue.X, ue.Y);
                            RestoreDefault();
                            Refresh();
                        }
                    }
                    break;

                default:
                    break;
            }

        }

        //handle a mouse exit event
        //for some things handle like a mouse up
        private void HandleMouseExit(UserEvent ue) {
            switch (ToolbarManager.Instance.SelectedButton) {

                case EToolbarButton.POINTER:
                case EToolbarButton.SUBNET:
                case EToolbarButton.TEXT:
                case EToolbarButton.PAN:
                    HandleMouseUp(ue);
                    break;

                default:
                    break;
            }

        }


        /// <summary>
        /// Handle the delete action.
        /// </summary>
        public void HandleDelete() {
            Item[] selectedItems = SelectionManager.Instance.SelectedItems().ToArray();

            //delete connections first to avoid concurrancy problem
            foreach (Item item in selectedItems) {
                if (item.IsConnector()) {
                    ConnectorItem cnx = (ConnectorItem)item;
                    bool startUnlocked = !cnx.StartItem.IsLocked();
                    bool endUnlocked = !cnx.EndItem.IsLocked();

                    if (startUnlocked || endUnlocked) {
                        item.Delete();
                    }
                }
            }

            //everything else
            foreach (Item item in selectedItems) {
                if (!item.IsConnector()) {
                    if (!item.IsLocked()) {
                        item.Delete();
                    }
                }
            }
            ForceDraw();
            Refresh();
            RestoreDefault();
        }

        //handle the snap to gerid 
        public void HandleSnap() {
            NodeLayer.SnapToGrid();
            SubnetLayer.SnapToGrid();
            AnnotationLayer.SnapToGrid();

            Refresh();
            RestoreDefault();
        }

        //create an icon item
        private Item CreateIconItem(double x, double y, EToolbarButton etype) {
            string imageid = ToolbarManager.Instance.FromType(etype).ImageId;

            return new NodeItem(NodeLayer, x, y, imageid);
        }

        //create a node box item that will be a container for nodes
        private Item CreateBoxItem(double x1, double y1, double x2, double y2) {

            Rect r = Rect.FromTwoPoints(x1, y1, x2, y2);

            if ((r.Width < 4) || (r.Height < 4)) {
                return null;
            }
            return new SubnetItem(SubnetLayer, r);
        }

        //Get the current scale in each direction
        public DoublePoint? CurrentScale() {
            if (JSInteropManager.Instance != null) {
                return JSInteropManager.Instance.Scale;
            }
            return null;
        }

        //convenience method set selected button to pointer 
        private void RestoreDefault() {
            StateMachine.Instance.CurrentState = EState.Idle;
            ToolbarManager.Instance.RestoreDefault();
        }

        /// <summary>
        /// Delete all items on all layers.
        /// </summary>
        public void DeleteAllItems() {
            ConnectorLayer.DeleteAllItems();
            SubnetLayer.DeleteAllItems();
            NodeLayer.DeleteAllItems();
            AnnotationLayer.DeleteAllItems();
            Refresh();
        }


        //force a redraw of everything
        public void Refresh() {
            PageManager.IsDirty = true;
            PageManager.ForceDraw();
            PageManager.Refresher();
        }

        //Force the map to redraw
        public void ForceDraw() {
            if (PageManager != null) {
                PageManager.ForceDraw();
            }
        }

        /// <summary>
        /// Refresh everything
        /// </summary>
        public void FullRefresh() {
            FixState();
            PageManager.IsDirty = true;

            ToolbarManager.Instance.SetAllBackgrounds();
            PropertyEditor.Instance.Refresher();
            FeedbackManager.Instance.Refresher();
            PaletteEditor.Instance.Refresher();
            TextColorEditor.Instance.Refresher();
            ToolbarManager.Instance.RefresherTools();
            ToolbarManager.Instance.RefresherNodes();

			CanvasRefresher();

			JSInteropManager? jsm = JSInteropManager.Instance;
            if (jsm != null) {
                jsm.SetOffsetsDirty();
                jsm.FixBlur();
            }
            ForceDraw();
        }

        //convert a pixel value to the closest spot on the virtual grid
        public double GridValue(double x) {
            double vs2 = ((double)VirtualGridSize) / 2.0;
            long lx = (long)(x + vs2);
            double dx = VirtualGridSize * (lx / VirtualGridSize);
            return dx;
        }

        //get all the layers
        public Layer[] GetAllLayers() {
            return _layers;
        }
    }
}
