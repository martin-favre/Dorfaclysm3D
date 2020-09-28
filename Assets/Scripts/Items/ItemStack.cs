namespace Items
{

    public class ItemStack
    {
        private readonly IItem item;
        private uint count;

        public ItemStack(IItem item, uint count)
        {
            this.item = item;
            this.count = count;
        }

        public bool IsEmpty()
        {
            return count == 0;
        }

        public IItem GetItem()
        {
            if (count <= 0) throw new System.Exception("Trying to get item from empty stack");
            count--;
            return (IItem)item.Clone();
        }

        public bool MayAddItem(IItem item)
        {
            return !this.item.IsUnique() && this.item.GetItemType() == item.GetItemType() && count < this.item.GetStackSize();
        }

        public void AddItem(IItem item)
        {
            if(!MayAddItem(item)) throw new System.Exception("May not add item to stack");
            count++;
        }

    }
}