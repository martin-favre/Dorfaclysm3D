namespace Items
{
    public interface IItem : System.ICloneable
    {
        string GetName();
        string GetDescription();
        ItemType GetItemType();
        int GetValue();
        bool IsUnique();
        uint GetStackSize();
    }
}