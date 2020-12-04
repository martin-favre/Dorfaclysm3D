using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Logging;
using UnityEngine;
using System.Linq;

public abstract class RequestPool<T> where T : PlayerRequest
{
    [System.Serializable]
    protected class RequestOnCooldown : IEquatable<RequestOnCooldown>
    {
        public readonly T request;
        public readonly double startTime;
        const int cooldownTime = 2; // seconds

        public RequestOnCooldown(T request, double startTime)
        {
            this.request = request;
            this.startTime = startTime;
        }

        public bool IsCool()
        {
            return startTime + cooldownTime < Time.time;
        }

        public bool Equals(RequestOnCooldown other)
        {
            return other.request.Equals(request);
        }
    }

    [System.Serializable]
    private class SaveData : GenericSaveData<RequestPool<T>>
    {
        public HashSet<T> requests = new HashSet<T>();
        public HashSet<T> handedOutRequests = new HashSet<T>();
    }
    protected List<RequestOnCooldown> coolingDownRequests = new List<RequestOnCooldown>();

    SaveData data = new SaveData();

    readonly protected object lockObject = new object();

    Dictionary<Guid, Action<T>> onCancelledCallbacks = new Dictionary<Guid, Action<T>>();

    protected LilLogger logger = new LilLogger(typeof(T).Name + "RequestPool");

    protected HashSet<T> Requests { get => data.requests; }
    protected HashSet<T> HandedOutRequests { get => data.handedOutRequests; }
    protected List<RequestOnCooldown> CoolingDownRequests { get => coolingDownRequests; }

    protected void ReturnCooledDownTasks(bool returnAll)
    {
        lock (lockObject)
        {
            while (CoolingDownRequests.Count > 0)
            {
                if (CoolingDownRequests.Last().IsCool() || returnAll)
                {
                    Requests.Add(CoolingDownRequests.Last().request);
                    CoolingDownRequests.RemoveAt(CoolingDownRequests.Count - 1);
                }
                else
                {
                    break;
                }
            }
        }
    }

    public virtual IGenericSaveData GetSave()
    {
        ReturnCooledDownTasks(true);
        lock (lockObject)
        {
            return data;
        }
    }

    public virtual void Load(IGenericSaveData save)
    {
        logger.Log("Loading");
        lock (lockObject)
        {
            data = (SaveData)save;
        }
        foreach (T request in data.requests)
        {
            OnRequestAdded(request);
        }
        foreach (T request in data.handedOutRequests)
        {
            OnRequestAdded(request);
        }
    }

    private bool IsRequestDuplicate(T request)
    {
        return HandedOutRequests.Contains(request) || Requests.Contains(request) || CoolingDownRequests.Contains(new RequestOnCooldown(request, 0));
    }

    public bool PostRequest(T request)
    {
        logger.Log("Attempting posting request " + request + " of type " + typeof(T).ToString());
        bool success = false;
        lock (lockObject)
        {
            if (IsRequestDuplicate(request))
            {
                logger.Log("Request was not posted due to being duplicate");
                return false;
            }
            success = Requests.Add(request);
            if (success)
            {
                logger.Log("Request was posted");
            }
            else
            {
                logger.Log("For unknown reason, did not post request " + request + " of type " + typeof(T).ToString(), LogLevel.Warning);
            }
        }
        if (success)
        {
            OnRequestAdded(request);
        }
        return success;
    }

    public void RegisterOnCancelledCallback(Guid requestGuid, Action<T> action)
    {
        onCancelledCallbacks[requestGuid] = action;
    }
    public void UnregisterOnCancelledCallback(Guid requestGuid)
    {
        bool success = onCancelledCallbacks.Remove(requestGuid);
    }

    protected virtual void OnRequestAdded(T request)
    {
        // May be implemented by children
    }

    protected virtual void OnRequestRemoved(T request)
    {
        // May be implemented by children
    }

    public void ReturnRequest(T request)
    {
        logger.Log("Returned request of type " + typeof(T).ToString());
        lock (lockObject)
        {
            HandedOutRequests.Remove(request);
            CoolingDownRequests.Add(new RequestOnCooldown(request, Time.time));
        }
    }

    protected void PerformForEachRequest(Action<T> action)
    {
        foreach (T req in Requests)
        {
            action(req);
        }
        foreach (T req in HandedOutRequests)
        {
            action(req);
        }
        foreach (RequestOnCooldown req in CoolingDownRequests)
        {
            action(req.request);
        }
    }

    public void CancelRequest(T request)
    {
        logger.Log("Cancelled request " + request + " of type " + typeof(T).ToString());

        Action<T> callback = null;
        lock (lockObject)
        {
            request.Cancel();
            HandedOutRequests.Remove(request);
            Requests.Remove(request);
            CoolingDownRequests.Remove(new RequestOnCooldown(request, 0));
            onCancelledCallbacks.TryGetValue(request.Guid, out callback);
        }
        if (callback != null)
        {
            callback(request);
        }
        OnRequestRemoved(request);
    }
    public void FinishRequest(T request)
    {
        logger.Log("Finished request " + request + " of type " + typeof(T).ToString());
        lock (lockObject)
        {
            HandedOutRequests.Remove(request);
            request.Finish();
        }
        OnRequestRemoved(request);
    }

    public bool HasRequests()
    {
        ReturnCooledDownTasks(false);
        lock (lockObject)
        {
            return Requests.Count != 0;
        }
    }

    protected void HandOutRequest(T request)
    {
        logger.Log("Handed out request " + request + " of type " + typeof(T).ToString());
        lock (lockObject)
        {
            Requests.Remove(request);
            HandedOutRequests.Add(request);
        }
    }

    public abstract T GetRequest(GridActor actor);
}
