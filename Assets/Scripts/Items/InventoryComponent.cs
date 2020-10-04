using System;
using System.Collections;
using System.Collections.Generic;
using Items;
using UnityEngine;

public class InventoryComponent : MonoBehaviour
{
    Dictionary<ItemType, ItemStack> itemStacks = new Dictionary<ItemType, ItemStack>();

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
        return itemStacks.Count != 0;
    }

    public bool HasItem(ItemType type)
    {
        return itemStacks.ContainsKey(type);
    }
    public Item GetItem(ItemType type)
    {
        ItemStack stack = itemStacks[type];
        Item item = stack.GetItem();
        if (!stack.HasItems())
        {
            itemStacks.Remove(type);
        }
        RunOnItemRemovedCallbacks();
        return item;
    }

    public void AddItem(Item item)
    {
        ItemStack stack;
        bool success = itemStacks.TryGetValue(item.GetItemType(), out stack);
        if (success)
        {
            stack.AddItem(item);
        }
        else
        {
            itemStacks[item.GetItemType()] = new ItemStack(item, 1);
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


}
