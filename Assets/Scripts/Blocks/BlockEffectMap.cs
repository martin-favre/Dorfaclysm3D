using System;
using System.Collections.Generic;
using System.Threading;
using Logging;
using UnityEngine;

public class BlockEffectMap
{
    static Dictionary<Vector3Int, List<Vector2>> blockEffects = new Dictionary<Vector3Int, List<Vector2>>();
    const int lockTimeout = 10000; // ms
    static readonly ReaderWriterLockSlim blockLock = new ReaderWriterLockSlim();
    readonly static object callbackLock = new object();

    static List<Action<Vector3Int>> runOnBlockChange = new List<Action<Vector3Int>>();

    static readonly LilLogger logger = new LilLogger("BlockEffectMap");

    static void GetReadLockBlockLock()
    {
        if (!blockLock.TryEnterReadLock(lockTimeout)) throw new Exception("Blocklock timeout");
    }
    static void GetWriteLockBlockLock()
    {
        if (!blockLock.TryEnterWriteLock(lockTimeout)) throw new Exception("Blocklock timeout");
    }

    public static void SetEffect(Vector3Int pos, Vector2 effect)
    {
        GetWriteLockBlockLock();
        try
        {

            List<Vector2> list;
            if (blockEffects.TryGetValue(pos, out list))
            {
                list.Add(effect);
            }
            else
            {
                list = new List<Vector2>();
                list.Add(effect);
                blockEffects[pos] = list;
            }
        }
        finally
        {
            blockLock.ExitWriteLock();
        }
        RunOnBlockChanges(pos);
    }

    public static Vector2 GetBlockEffect(Vector3Int pos)
    {
        GetReadLockBlockLock();
        try
        {
            List<Vector2> effects;
            bool success = blockEffects.TryGetValue(pos, out effects);
            if (success && effects.Count > 0)
            {
                // just return the first effect for now.
                // Maybe a priotization will be needed here
                return effects[0];
            }
        }
        finally
        {
            blockLock.ExitReadLock();
        }
        return BlockEffects.NoEffect;
    }

    static void RunOnBlockChanges(Vector3Int pos)
    {
        lock (callbackLock)
        {
            foreach (Action<Vector3Int> action in runOnBlockChange)
            {
                action(pos);
            }
        }
    }

    public static void RemoveEffect(Vector3Int pos, Vector2 effect)
    {
        GetReadLockBlockLock();
        try
        {
            bool success = blockEffects[pos].Remove(effect);
            if (!success) logger.Log("Tried to remove nonexistant effect", LogLevel.Warning);
        }
        finally
        {
            blockLock.ExitReadLock();
        }
        RunOnBlockChanges(pos);
    }

    public static void RegisterOnEffectAddedCallback(Action<Vector3Int> callback)
    {
        lock (callbackLock)
        {
            runOnBlockChange.Add(callback);
        }
    }

    public static void UnregisterOnEffectAddedCallback(Action<Vector3Int> callback)
    {
        lock (callbackLock)
        {
            bool success = runOnBlockChange.Remove(callback);
            if (!success) logger.Log("Tried to remove unregistered callback", LogLevel.Warning);
        }
    }

}