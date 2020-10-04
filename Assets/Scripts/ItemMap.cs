
using System;
using System.Collections.Generic;
using System.Threading;
using Items;
using UnityEngine;

public static class ItemMap
{
    const int lockTimeout = 10000; // ms
    static ReaderWriterLockSlim itemLock = new ReaderWriterLockSlim();
    static Dictionary<Vector3Int, List<InventoryComponent>> items = new Dictionary<Vector3Int, List<InventoryComponent>>();

    static public InventoryComponent[] GetInventories(Vector3Int pos)
    {
        if (!itemLock.TryEnterReadLock(lockTimeout)) throw new Exception("Readlock timeout");
        InventoryComponent[] foundItems;
        try
        {
            if (items.ContainsKey(pos))
            {
                foundItems = items[pos].ToArray();
            }
            else
            {
                foundItems = new InventoryComponent[0];
            }
        }
        finally
        {
            itemLock.ExitReadLock();
        }

        return foundItems;

    }

    static public void RegisterInventory(InventoryComponent inventory, Vector3Int pos)
    {
        if (!itemLock.TryEnterWriteLock(lockTimeout)) throw new Exception("Writelock timeout");
        if (!items.ContainsKey(pos))
        {
            items[pos] = new List<InventoryComponent>();
        }
        items[pos].Add(inventory);
        itemLock.ExitWriteLock();
    }
    static public void UnregisterInventory(InventoryComponent inventory, Vector3Int pos)
    {
        if (!itemLock.TryEnterWriteLock(lockTimeout)) throw new Exception("Writelock timeout");
        try
        {
            items[pos].Remove(inventory);
        }
        finally
        {
            itemLock.ExitWriteLock();
        }

    }

}