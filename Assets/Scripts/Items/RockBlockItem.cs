namespace Items {
    public class RockBlockItem : IItem
    {
        public object Clone()
        {
            return new RockBlockItem();
        }

        public string GetDescription()
        {
            return "A block of rock";
        }

        public ItemType GetItemType()
        {
            return ItemType.RockBlock;
        }

        public string GetName()
        {
            return "Rock Block";
        }

        public uint GetStackSize()
        {
            return 100;
        }

        public int GetValue()
        {
            return 1;
        }

        public bool IsUnique()
        {
            return false;
        }
    }
}