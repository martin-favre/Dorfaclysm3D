using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Concurrent;
using System.Threading;
using System;
using Items;

public static class GridMap
{
    [System.Serializable]
    private class SaveData
    {
        public Vector3Int size;
        public Dictionary<Vector3Int, Block> blocks;
        public bool generated;

    }
    static Vector3Int mapSize = Vector3Int.zero;
    static Dictionary<Vector3Int, Block> blocks = new Dictionary<Vector3Int, Block>();
    static bool generated = false;
    const int lockTimeout = 10000; // ms
    static ReaderWriterLockSlim blockLock = new ReaderWriterLockSlim();
    static ReaderWriterLockSlim callbackLock = new ReaderWriterLockSlim();
    static List<Action<Vector3Int>> runOnBlockChange = new List<Action<Vector3Int>>();

    static GridMap()
    {
    }

    static public object GetSave()
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

    static public void LoadSave(object data)
    {
        SaveData save = (SaveData)data;
        EnterWriteLock();
        mapSize = save.size;
        generated = save.generated;
        blocks = save.blocks;
        blockLock.ExitWriteLock();
    }

    public static void GenerateMap(Vector3Int size)
    {
        SetSize(size);
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int z = 0; z < mapSize.z; z++)
                {
                    Vector3Int pos = new Vector3Int(x, y, z);

                    if (y == x && (z == 5 || z == 10))
                    {
                        SetBlock(pos, new StairUpDownBlock());
                    }
                    else if (y == x)
                    {
                        SetBlock(pos, new GrassBlock());
                    }
                    else if (y < x)
                    {
                        SetBlock(pos, new RockBlock());
                    }
                    else
                    {
                        SetBlock(pos, new AirBlock());
                    }
                }
            }
        }
        SetGenerationDone();
    }

    static private void SetGenerationDone()
    {
        generated = true;
    }

    static public bool IsGenerationDone()
    {
        return generated;
    }
    static public void RegisterCallbackOnBlockChange(Action<Vector3Int> func)
    {
        if (!callbackLock.TryEnterWriteLock(lockTimeout)) throw new Exception("WriteLock timeout");
        runOnBlockChange.Add(func);
        callbackLock.ExitWriteLock();
    }

    static public void UnregisterCallbackOnBlockChange(Action<Vector3Int> func)
    {
        if (!callbackLock.TryEnterWriteLock(lockTimeout)) throw new Exception("WriteLock timeout");
        runOnBlockChange.Remove(func);
        callbackLock.ExitWriteLock();
    }

    static public bool IsPosInMap(Vector3Int pos)
    {
        GridMap.EnterReadLock();
        bool result = blocks.ContainsKey(pos);
        blockLock.ExitReadLock();
        return result;
    }

    static public bool IsPosFree(Vector3Int pos)
    {
        if (!IsBlockFree(pos)) return false;
        return GridActorMap.IsPosFree(pos);
    }

    static private bool IsBlockFree(Vector3Int pos)
    {
        Block block;
        TryGetBlock(pos, out block);
        if (block != null)
        {
            return block.supportsWalkingThrough();
        }
        return false; // If we don't have a block, you can't walk there
    }

    static public bool TryGetBlock(Vector3Int pos, out Block block)
    {
        if (!IsGenerationDone())
        {
            block = null;
            return false;
        }
        GridMap.EnterReadLock();
        bool result = blocks.TryGetValue(pos, out block);
        blockLock.ExitReadLock();
        return result;
    }

    static public Block GetBlock(Vector3Int pos)
    {
        Block block;

        if (!TryGetBlock(pos, out block))
        {
            throw new KeyNotFoundException("No block in position");
        }
        return block;
    }
    static private void SetSize(Vector3Int size)
    {
        EnterWriteLock();
        mapSize = size;
        blockLock.ExitWriteLock();
    }

    static public Vector3Int GetSize()
    {
        GridMap.EnterReadLock();
        Vector3Int size = mapSize;
        blockLock.ExitReadLock();
        return size;
    }

    static bool InMap(Vector3Int pos)
    {
        GridMap.EnterReadLock();
        bool inMap = blocks.ContainsKey(pos);
        blockLock.ExitReadLock();
        return inMap;
    }

    static private void RunCallbacks(Vector3Int pos)
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

    static void EnterWriteLock()
    {
        if (!blockLock.TryEnterWriteLock(lockTimeout)) throw new Exception("Writelock timeout");
    }

    static void EnterReadLock()
    {
        if (!blockLock.TryEnterReadLock(lockTimeout)) throw new Exception("Readlock timeout");
    }

    private static void SpawnBlockItem(Vector3Int pos, Block prevBlock)
    {
        IItem itemToAdd = prevBlock.GetItem();
        if (itemToAdd == null) return;
        GridActor[] actors = GridActorMap.GetGridActors(pos);
        ItemContainerComponent itemCont = null;
        foreach (GridActor actor in actors)
        {
            ItemContainerComponent tmpCont = actor.GetComponent<ItemContainerComponent>();
            if (tmpCont != null && tmpCont.MayAddItem(itemToAdd))
            {
                itemCont = tmpCont;
            }
        }
        if (itemCont == null)
        {
            itemCont = ItemContainerComponent.InstantiateNew(itemToAdd, 1);
            itemCont.transform.position = pos + new Vector3(.5f, -.5f, .5f);
        }
        else
        {
            itemCont.AddItem(itemToAdd);
        }
    }

    static public void SetBlock(Vector3Int pos, Block block)
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
                SpawnBlockItem(pos, prevBlock);
            }

        }
    }

}
