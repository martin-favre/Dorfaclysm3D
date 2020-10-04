using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System;

public class GridActorMap
{
    const int lockTimeout = 10000; // ms
    static ReaderWriterLockSlim gridActorLock = new ReaderWriterLockSlim();
    static Dictionary<Vector3Int, List<GridActor>> gridActors = new Dictionary<Vector3Int, List<GridActor>>();
    static public GridActor[] GetGridActors(Vector3Int pos)
    {
        if (!gridActorLock.TryEnterReadLock(lockTimeout)) throw new Exception("Readlock timeout");
        GridActor[] actors;
        try
        {
            if (gridActors.ContainsKey(pos))
            {

                actors = gridActors[pos].ToArray();
            }
            else
            {
                actors = new GridActor[0];
            }
        }
        finally
        {
            gridActorLock.ExitReadLock();
        }

        return actors;
    }

    static public void RegisterGridActor(GridActor actor, Vector3Int pos)
    {
        if (!gridActorLock.TryEnterWriteLock(lockTimeout)) throw new Exception("Writelock timeout");
        if (!gridActors.ContainsKey(pos))
        {
            gridActors[pos] = new List<GridActor>();
        }
        gridActors[pos].Add(actor);
        gridActorLock.ExitWriteLock();
    }
    static public void UnregisterGridActor(GridActor actor, Vector3Int pos)
    {
        if (!gridActorLock.TryEnterWriteLock(lockTimeout)) throw new Exception("Writelock timeout");
        try
        {
            gridActors[pos].Remove(actor);
        }
        finally
        {
            gridActorLock.ExitWriteLock();
        }

    }

    static public bool IsPosFree(Vector3Int pos)
    {
        if (!gridActorLock.TryEnterReadLock(lockTimeout)) throw new Exception("Readlock timeout");
        List<GridActor> actors;
        bool success = gridActors.TryGetValue(pos, out actors);
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
        gridActorLock.ExitReadLock();
        return retVal;
    }


}