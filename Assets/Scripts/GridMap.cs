using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Concurrent;
using System.Threading;
using System;
using Items;

public class GridMap : IHasBlocks
{
    [System.Serializable]
    private class SaveData
    {
        public Vector3Int size;
        public Dictionary<Vector3Int, Block> blocks;
        public bool generated;

    }
    static readonly GridMap instance = new GridMap();
    Vector3Int mapSize = Vector3Int.zero;
    Dictionary<Vector3Int, Block> blocks = new Dictionary<Vector3Int, Block>();
    bool generated = false;
    const int lockTimeout = 10000; // ms
    ReaderWriterLockSlim blockLock = new ReaderWriterLockSlim();
    ReaderWriterLockSlim callbackLock = new ReaderWriterLockSlim();
    List<Action<Vector3Int>> runOnBlockChange = new List<Action<Vector3Int>>();

    public static GridMap Instance { get => instance; }

    GridMap() { }

    public object GetSave()
    {
        SaveData data = new SaveData();
        EnterReadLock();
        data.size = mapSize;
        data.generated = generated;
        data.blocks = new Dictionary<Vector3Int, Block>();
        foreach (var key in blocks.Keys)
        {
            data.blocks[key] = (Block)blocks[key].Clone();
        }
        blockLock.ExitReadLock();
        return data;
    }

    public void LoadSave(object data)
    {
        SaveData save = (SaveData)data;
        EnterWriteLock();
        mapSize = save.size;
        generated = save.generated;
        blocks = save.blocks;
        blockLock.ExitWriteLock();
    }

    public void GenerateMap(Vector3Int size, GenerationParameters parameters)
    {
        generated = false;
        new MapGenerator(this, parameters).Generate(size);
        SetGenerationDone();
    }

    private void SetGenerationDone()
    {
        generated = true;
    }

    public bool IsGenerationDone()
    {
        return generated;
    }
    public void RegisterCallbackOnBlockChange(Action<Vector3Int> func)
    {
        if (!callbackLock.TryEnterWriteLock(lockTimeout)) throw new Exception("WriteLock timeout");
        runOnBlockChange.Add(func);
        callbackLock.ExitWriteLock();
    }

    public void UnregisterCallbackOnBlockChange(Action<Vector3Int> func)
    {
        if (!callbackLock.TryEnterWriteLock(lockTimeout)) throw new Exception("WriteLock timeout");
        runOnBlockChange.Remove(func);
        callbackLock.ExitWriteLock();
    }

    public bool IsPosInMap(Vector3Int pos)
    {
        EnterReadLock();
        bool result = blocks.ContainsKey(pos);
        blockLock.ExitReadLock();
        return result;
    }

    public bool IsPosFree(Vector3Int pos)
    {
        if (!IsBlockFree(pos)) return false;
        return GridActorMap.IsPosFree(pos);
    }

    private bool IsBlockFree(Vector3Int pos)
    {
        Block block;
        TryGetBlock(pos, out block);
        if (block != null)
        {
            return block.supportsWalkingThrough();
        }
        return false; // If we don't have a block, you can't walk there
    }

    public bool TryGetBlock(Vector3Int pos, out Block block)
    {
        if (!IsGenerationDone())
        {
            block = null;
            return false;
        }
        EnterReadLock();
        bool result = blocks.TryGetValue(pos, out block);
        blockLock.ExitReadLock();
        return result;
    }

    public Block GetBlock(Vector3Int pos)
    {
        Block block;

        if (!TryGetBlock(pos, out block))
        {
            throw new KeyNotFoundException("No block in position");
        }
        return block;
    }
    public void SetSize(Vector3Int size)
    {
        EnterWriteLock();
        mapSize = size;
        blockLock.ExitWriteLock();
    }

    public Vector3Int GetSize()
    {
        EnterReadLock();
        Vector3Int size = mapSize;
        blockLock.ExitReadLock();
        return size;
    }

    bool InMap(Vector3Int pos)
    {
        EnterReadLock();
        bool inMap = blocks.ContainsKey(pos);
        blockLock.ExitReadLock();
        return inMap;
    }

    private void RunCallbacks(Vector3Int pos)
    {
        if (!callbackLock.TryEnterReadLock(lockTimeout)) throw new Exception("Readlock timeout");
        try
        {
            foreach (Action<Vector3Int> a in runOnBlockChange)
            {
                a(pos);
            }
        }
        finally
        {
            callbackLock.ExitReadLock();
        }
    }

    void EnterWriteLock()
    {
        if (!blockLock.TryEnterWriteLock(lockTimeout)) throw new Exception("Writelock timeout");
    }

    void EnterReadLock()
    {
        if (!blockLock.TryEnterReadLock(lockTimeout)) throw new Exception("Readlock timeout");
    }

    public void PutItem(Vector3Int pos, Item itemToAdd)
    {
        if (itemToAdd == null) return;
        Debug.Log("GridMap, Adding item to " + pos);
        GridActor[] actors = GridActorMap.GetGridActors(pos);
        DroppedItemComponent itemCont = null;
        foreach (GridActor actor in actors)
        {
            DroppedItemComponent tmpCont = actor.GetComponent<DroppedItemComponent>();
            if (tmpCont != null)
            {
                itemCont = tmpCont;
                break;
            }
        }
        if (itemCont == null)
        {
            itemCont = DroppedItemComponent.InstantiateNew(pos);

            Debug.Log("GridMap, Spawned new DroppedItemComponent");
        }

        InventoryComponent inv = itemCont.gameObject.GetComponent<InventoryComponent>();
        if (inv)
        {
            inv.AddItem(itemToAdd);
        }
        else
        {
            Debug.LogWarning("GridMap, No inventory to drop item in");
        }

    }

    public void SetBlock(Vector3Int pos, Block block)
    {
        EnterWriteLock();

        Block prevBlock;
        blocks.TryGetValue(pos, out prevBlock);
        blocks[pos] = block;
        blockLock.ExitWriteLock();
        if (IsGenerationDone())
        {
            RunCallbacks(pos);
            if (prevBlock != null && block.Type == Block.BlockType.airBlock)
            {
                PutItem(pos, prevBlock.GetItem());
            }

        }
    }

}
