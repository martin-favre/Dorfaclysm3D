namespace Items
{
    [System.Serializable]
    public abstract class Item : System.ICloneable
    {
        public abstract string GetName();
        public abstract string GetDescription();
        public abstract ItemType GetItemType();
        public abstract int GetValue();
        public abstract uint GetStackSize();
        public abstract object Clone();
    }
}