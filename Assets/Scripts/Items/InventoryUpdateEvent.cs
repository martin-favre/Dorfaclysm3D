namespace Items {
    public class InventoryUpdateEvent {
        public enum UpdateType {
            Added,
            Removed
        }
        readonly Item item;
        readonly UpdateType type;

        public Item Item => item;

        public UpdateType Type => type;

        public InventoryUpdateEvent(Item item, UpdateType type)
        {
            this.item = item;
            this.type = type;
        }
    }
}