namespace Items
{
    [System.Serializable]
    public class RockBlockItem : Item
    {
        public override object Clone()
        {
            return new RockBlockItem();
        }

        public override string GetDescription()
        {
            return "A block of rock";
        }

        public override ItemType GetItemType()
        {
            return ItemType.RockBlock;
        }

        public override string GetName()
        {
            return "Rock Block";
        }

        public override uint GetStackSize()
        {
            return 100;
        }

        public override int GetValue()
        {
            return 1;
        }
    }
}