using System;
using System.Collections.Generic;

namespace Items
{

    [Serializable]
    public class ItemStack 
    {
        private readonly Item item;
        private uint count;

        public ItemStack(Item item, uint count)
        {
            this.item = item;
            this.count = count;
        }

        public bool HasItems()
        {
            return count != 0;
        }

        public Item GetItem()
        {
            if (count <= 0) throw new System.Exception("Trying to get item from empty stack");
            count--;
            return (Item)item.Clone();
        }

        public bool MayAddItem(Item item)
        {
            return this.item.GetItemType() == item.GetItemType() && count < this.item.GetStackSize();
        }

        public void AddItem(Item item)
        {
            if(!MayAddItem(item)) throw new System.Exception("May not add item to stack");
            count++;
        }

    }
}