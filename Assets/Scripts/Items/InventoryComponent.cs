using System;
using System.Collections;
using System.Collections.Generic;
using Items;
using UnityEngine;

namespace Items
{

    public class InventoryComponent : MonoBehaviour, ISaveableComponent, IObservable<InventoryUpdateEvent>
    {
        [Serializable]
        private class SaveData : GenericSaveData<InventoryComponent>
        {
            public Dictionary<Type, ItemStack> itemStacks = new Dictionary<Type, ItemStack>();
        }

        SaveData data = new SaveData();
        private List<IObserver<InventoryUpdateEvent>> observers = new List<IObserver<InventoryUpdateEvent>>();

        public bool HasItems()
        {
            return data.itemStacks.Count != 0;
        }

        public Item GetMostCommonItem()
        {
            ItemStack mostCommonItem = null;

            foreach (KeyValuePair<Type, ItemStack> stack in data.itemStacks)
            {
                if (mostCommonItem == null || stack.Value.Count > mostCommonItem.Count)
                {
                    mostCommonItem = stack.Value;
                }
            }

            if (mostCommonItem != null)
            {
                return mostCommonItem.Item;
            }
            else
            {
                return null;
            }
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
            GenerateItemUpdateEvents(item, InventoryUpdateEvent.UpdateType.Removed);
            return item;
        }

        public int GetItemCount(Type type) 
        {
            ItemStack stack; 
            bool success = data.itemStacks.TryGetValue(type, out stack);
            if(!success) return 0;
            return (int)stack.Count;
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
            GenerateItemUpdateEvents(item, InventoryUpdateEvent.UpdateType.Added);
        }

        void GenerateItemUpdateEvents(Item item, InventoryUpdateEvent.UpdateType type)
        {
            InventoryUpdateEvent newEvent = new InventoryUpdateEvent(item, type);
            foreach (var observer in observers)
            {
                observer.OnNext(newEvent);
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

        public IDisposable Subscribe(IObserver<InventoryUpdateEvent> observer)
        {
            IDisposable sub = new GenericUnsubscriber<InventoryUpdateEvent>(observers, observer);
            return sub;
        }
    }

}