namespace CanvasDrawer.Graphics.Items {
    public sealed class ItemEvent {
        public Item Item { get; set; }

        public EItemChange Type { get; set; }

        public ItemEvent(Item item, EItemChange etype) {
            Item = item;
            Type = etype;
        }
    }
}
