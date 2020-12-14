using System;
using System.Collections;
using System.Collections.Generic;
using Items;
using UnityEngine;

namespace Items
{

    public class InventoryComponent : MonoBehaviour, ISaveableComponent
    {
        [Serializable]
        private class SaveData : GenericSaveData<InventoryComponent>
        {
            public Dictionary<Type, ItemStack> itemStacks = new Dictionary<Type, ItemStack>();
        }

        SaveData data = new SaveData();

        List<Action> onItemAdded = new List<Action>();
        List<Action> onItemRemoved = new List<Action>();

        public void RegisterOnItemAddedCallback(Action action)
        {
            onItemAdded.Add(action);
        }

        public void UnregisterOnItemAddedCallback(Action action)
        {
            onItemAdded.Remove(action);
        }

        public void RegisterOnItemRemovedCallback(Action action)
        {
            onItemRemoved.Add(action);
        }

        public void UnregisterOnItemRemovedCallback(Action action)
        {
            onItemRemoved.Remove(action);
        }

        public bool HasItems()
        {
            return data.itemStacks.Count != 0;
        }

        public bool HasItem(Type type)
        {
            return data.itemStacks.ContainsKey(type);
        }
        public Item GetItem(Type type)
        {
            ItemStack stack = data.itemStacks[type];
            Item item = stack.GetItem();
            if (!stack.HasItems())
            {
                data.itemStacks.Remove(type);
            }
            RunOnItemRemovedCallbacks();
            return item;
        }

        public void AddItem(Item item)
        {
            ItemStack stack;
            bool success = data.itemStacks.TryGetValue(item.GetType(), out stack);
            if (success)
            {
                stack.AddItem(item);
            }
            else
            {
                data.itemStacks[item.GetType()] = new ItemStack(item, 1);
            }
            RunOnItemAddedCallbacks();
        }

        void RunOnItemAddedCallbacks()
        {
            foreach (Action action in onItemAdded)
            {
                action();
            }
        }

        void RunOnItemRemovedCallbacks()
        {
            foreach (Action action in onItemRemoved)
            {
                action();
            }
        }

        public IGenericSaveData Save()
        {
            return data;
        }

        public void Load(IGenericSaveData data)
        {
            this.data = (SaveData)data;
        }
    }

}