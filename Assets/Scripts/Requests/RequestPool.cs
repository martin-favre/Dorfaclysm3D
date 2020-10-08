using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public abstract class RequestPool<T> where T : PlayerRequest
{
    protected HashSet<T> requests = new HashSet<T>();
    protected HashSet<T> handedOutRequests = new HashSet<T>();

    readonly protected object lockObject = new object();
    public bool PostRequest(T request)
    {
        bool success;
        lock (lockObject)
        {
            success = requests.Add(request);
        }
        return success;

    }

    public void ReturnRequest(T request)
    {
        lock (lockObject)
        {
            handedOutRequests.Remove(request);
        }
        Task.Run(
            () =>
            {
                // chill a bit before re-adding it to the pool
                Thread.Sleep(1000);
                PostRequest(request);
            });

    }

    public void CancelRequest(T request)
    {
        lock (lockObject)
        {
            request.Cancel();
            handedOutRequests.Remove(request);
        }
    }

    public void FinishRequest(T request)
    {
        lock (lockObject)
        {
            handedOutRequests.Remove(request);
            request.Finish();
        }
    }

    public bool HasRequests()
    {
        bool hasRequests;
        lock (lockObject)
        {
            hasRequests = requests.Count != 0;
        }
        return hasRequests;
    }

    protected void HandOutRequest(T request)
    {
        lock (lockObject)
        {
            requests.Remove(request);
            handedOutRequests.Add(request);
        }
    }

    public abstract T GetRequest(GridActor actor);
}
