using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Concurrent;
using System.Threading;
using System;
public static class GridMap
{
    [System.Serializable]
    private class SaveData
    {
        public SerializeableVector3Int size;
        public Dictionary<SerializeableVector3Int, Block> blocks;
        public bool generated;

    }
    static private Vector3Int mSize = Vector3Int.zero;
    static private Dictionary<Vector3Int, Block> mBlocks = new Dictionary<Vector3Int, Block>();
    static private bool mGenerated = false;
    const int mLockTimeout = 10000; // ms
    static ReaderWriterLockSlim mLock = new ReaderWriterLockSlim();
    static ReaderWriterLockSlim mCallbackLock = new ReaderWriterLockSlim();
    static private List<Action<Vector3Int>> mRunOnBlockChange = new List<Action<Vector3Int>>();

    static GridMap()
    {
    }

    static public object GetSave()
    {
        SaveData data = new SaveData();
        EnterReadLock();
        data.size = new SerializeableVector3Int(mSize);
        data.generated = mGenerated;
        data.blocks = new Dictionary<SerializeableVector3Int, Block>();
        foreach (var key in mBlocks.Keys)
        {
            data.blocks[new SerializeableVector3Int(key)] = (Block)mBlocks[key].Clone();
        }
        mLock.ExitReadLock();
        return data;
    }

    static public void LoadSave(object data)
    {
        SaveData save = (SaveData)data;
        EnterWriteLock();
        mSize = save.size.Get();
        mGenerated = save.generated;
        mBlocks.Clear();
        foreach (var key in save.blocks.Keys)
        {
            mBlocks[key.Get()] = (Block)save.blocks[key].Clone();
        }
        mLock.ExitWriteLock();
    }

    public static void GenerateMap(Vector3Int size)
    {
        SetSize(size);
        for (int x = 0; x < mSize.x; x++)
        {
            for (int y = 0; y < mSize.y; y++)
            {
                for (int z = 0; z < mSize.z; z++)
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
        mGenerated = true;
    }

    static public bool IsGenerationDone()
    {
        return mGenerated;
    }
    static public void RegisterCallbackOnBlockChange(Action<Vector3Int> func)
    {
        if (!mCallbackLock.TryEnterWriteLock(mLockTimeout)) throw new Exception("WriteLock timeout");
        mRunOnBlockChange.Add(func);
        mCallbackLock.ExitWriteLock();
    }

    static public void UnregisterCallbackOnBlockChange(Action<Vector3Int> func)
    {
        if (!mCallbackLock.TryEnterWriteLock(mLockTimeout)) throw new Exception("WriteLock timeout");
        mRunOnBlockChange.Remove(func);
        mCallbackLock.ExitWriteLock();
    }

    static public bool IsPosInMap(Vector3Int pos){
        GridMap.EnterReadLock();
        bool result = mBlocks.ContainsKey(pos);
        mLock.ExitReadLock();
        return result;
    }

    static public bool IsBlockFree(Vector3Int pos){
        Block block;
        TryGetBlock(pos, out block);
        if(block != null){
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
        bool result = mBlocks.TryGetValue(pos, out block);
        mLock.ExitReadLock();
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
        mSize = size;
        mLock.ExitWriteLock();
    }

    static public Vector3Int GetSize()
    {
        GridMap.EnterReadLock();
        Vector3Int size = mSize;
        mLock.ExitReadLock();
        return size;
    }

    static bool InMap(Vector3Int pos)
    {
        GridMap.EnterReadLock();
        bool inMap = mBlocks.ContainsKey(pos);
        mLock.ExitReadLock();
        return inMap;
    }

    static private void RunCallbacks(Vector3Int pos)
    {
        if (!mCallbackLock.TryEnterReadLock(mLockTimeout)) throw new Exception("Readlock timeout");
        try
        {
            foreach (Action<Vector3Int> a in mRunOnBlockChange)
            {
                a(pos);
            }
        }
        finally
        {
            mCallbackLock.ExitReadLock();
        }
    }

    static void EnterWriteLock()
    {
        if (!mLock.TryEnterWriteLock(mLockTimeout)) throw new Exception("Writelock timeout");
    }

    static void EnterReadLock()
    {
        if (!mLock.TryEnterReadLock(mLockTimeout)) throw new Exception("Readlock timeout");
    }

    static public void SetBlock(Vector3Int pos, Block block)
    {
        EnterWriteLock();
        mBlocks[pos] = block;
        mLock.ExitWriteLock();
        if (IsGenerationDone())
        {
            RunCallbacks(pos);
        }
    }

}
