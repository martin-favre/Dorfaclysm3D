using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Logging;
using UnityEngine;

public class BlockEffectMap : IObservable<BlockEffectMap.BlockEffectUpdate>
{

    public class BlockEffectUpdate
    {
        public enum ChangeType
        {
            Added,
            Removed
        };
        readonly Vector3Int position;
        readonly Vector2 effect;
        readonly ChangeType changeType;

        public Vector3Int Position => position;

        public Vector2 Effect => effect;

        public ChangeType ChangeType1 => changeType;

        public BlockEffectUpdate(Vector3Int position, Vector2 effect, ChangeType changeType)
        {
            this.position = position;
            this.effect = effect;
            this.changeType = changeType;
        }
    }

    static BlockEffectMap instance;
    static Dictionary<Vector3Int, List<Vector2>> blockEffects = new Dictionary<Vector3Int, List<Vector2>>();
    const int lockTimeout = 10000; // ms
    static readonly ReaderWriterLockSlim blockLock = new ReaderWriterLockSlim();

    static ConcurrentDictionary<IObserver<BlockEffectUpdate>, IObserver<BlockEffectUpdate>> observers = new ConcurrentDictionary<IObserver<BlockEffectUpdate>, IObserver<BlockEffectUpdate>>();

    static readonly LilLogger logger = new LilLogger("BlockEffectMap");

    public static BlockEffectMap Instance { get => instance; }

    static BlockEffectMap() {
        instance = new BlockEffectMap();
    }

    void GetReadLockBlockLock()
    {
        if (!blockLock.TryEnterReadLock(lockTimeout)) throw new Exception("Blocklock timeout");
    }
    void GetWriteLockBlockLock()
    {
        if (!blockLock.TryEnterWriteLock(lockTimeout)) throw new Exception("Blocklock timeout");
    }

    public void SetEffect(Vector3Int pos, Vector2 effect)
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
        RunOnBlockChanges(new BlockEffectUpdate(pos, effect, BlockEffectUpdate.ChangeType.Added));
    }

    public Vector2 GetBlockEffect(Vector3Int pos)
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

    void RunOnBlockChanges(BlockEffectUpdate update)
    {
        foreach (var obs in observers)
        {
            obs.Value.OnNext(update);
        }
    }

    public void RemoveEffect(Vector3Int pos, Vector2 effect)
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
        RunOnBlockChanges(new BlockEffectUpdate(pos, effect, BlockEffectUpdate.ChangeType.Removed));
    }

    public IDisposable Subscribe(IObserver<BlockEffectUpdate> observer)
    {
        return new ConcurrentUnsubscriber<BlockEffectUpdate>(observers, observer);
    }
}