using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Concurrent;
using System.Threading;
using System;
using Items;
public class BlockUpdate
{
    readonly Vector3Int position;
    readonly Block block;

    public BlockUpdate(Vector3Int position, Block block)
    {
        this.position = position;
        this.block = block;
    }

    public Vector3Int Position => position;

    public Block Block => block;
}

public class GridMap : IGridMap
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
    static ConcurrentDictionary<IObserver<BlockUpdate>, IObserver<BlockUpdate>> observers = new ConcurrentDictionary<IObserver<BlockUpdate>, IObserver<BlockUpdate>>();

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

    public void GenerateMap(IMapGenerator generator)
    {
        generated = false;
        EnterWriteLock();
        blocks.Clear();
        blockLock.ExitWriteLock();
        generator.Generate(this, SetGenerationDone);
    }

    private void SetGenerationDone()
    {
        generated = true;
    }

    public bool IsGenerationDone()
    {
        return generated;
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
            return block.SupportsWalkingThrough();
        }
        return false; // If we don't have a block, you can't walk there
    }

    public bool TryGetBlock(Vector3Int pos, out Block block)
    {
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

    private void RunCallbacks(BlockUpdate update)
    {
        if (!callbackLock.TryEnterReadLock(lockTimeout)) throw new Exception("Readlock timeout");
        try
        {
            foreach (var a in observers)
            {
                a.Value.OnNext(update);
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

    public void SetBlock(Vector3Int pos, Block block)
    {
        EnterWriteLock();

        Block prevBlock;
        blocks.TryGetValue(pos, out prevBlock);
        blocks[pos] = block;
        blockLock.ExitWriteLock();
        if (IsGenerationDone())
        {
            RunCallbacks(new BlockUpdate(pos, block));
            if (prevBlock != null && block is AirBlock)
            {
                DroppedItemManager.PutItem(pos, prevBlock.GetItem());
            }

        }
    }

    public IDisposable Subscribe(IObserver<BlockUpdate> observer)
    {
        return new ConcurrentUnsubscriber<BlockUpdate>(observers, observer);
    }
}
