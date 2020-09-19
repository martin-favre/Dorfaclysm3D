using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System;

public class GridActorMap
{
    const int mLockTimeout = 10000; // ms
    static ReaderWriterLockSlim mGridActorLock = new ReaderWriterLockSlim();
    static Dictionary<Vector3Int, List<GridActor>> mGridActors = new Dictionary<Vector3Int, List<GridActor>>();
    static public GridActor[] GetGridActors(Vector3Int pos)
    {
        if (!mGridActorLock.TryEnterReadLock(mLockTimeout)) throw new Exception("Readlock timeout");
        GridActor[] actors = mGridActors[pos].ToArray();
        mGridActorLock.ExitReadLock();
        return actors;
    }

    static public void RegisterGridActor(GridActor actor, Vector3Int pos)
    {
        if (!mGridActorLock.TryEnterWriteLock(mLockTimeout)) throw new Exception("Writelock timeout");
        if (!mGridActors.ContainsKey(pos))
        {
            mGridActors[pos] = new List<GridActor>();
        }
        mGridActors[pos].Add(actor);
        mGridActorLock.ExitWriteLock();
    }
    static public void UnregisterGridActor(GridActor actor, Vector3Int pos)
    {
        if (!mGridActorLock.TryEnterWriteLock(mLockTimeout)) throw new Exception("Writelock timeout");
        mGridActors[pos].Remove(actor);
        mGridActorLock.ExitWriteLock();
    }

    static public bool IsPosFree(Vector3Int pos)
    {
        if (!mGridActorLock.TryEnterReadLock(mLockTimeout)) throw new Exception("Readlock timeout");
        List<GridActor> actors;
        bool success = mGridActors.TryGetValue(pos, out actors);
        bool retVal = true;
        if (success)
        {
            foreach (GridActor actor in actors)
            {
                if (actor.IsBlocking())
                {
                    retVal = false;
                    break;
                }
            }
        }
        mGridActorLock.ExitReadLock();
        return retVal;
    }


}