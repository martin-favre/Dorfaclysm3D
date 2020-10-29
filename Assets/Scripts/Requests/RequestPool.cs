using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public abstract class RequestPool<T> where T : PlayerRequest
{
    [System.Serializable]
    protected class RequestOnCooldown
    {
        public readonly T request;
        public readonly double startTime;
        const int cooldownTime = 5; // seconds

        public RequestOnCooldown(T request, double startTime)
        {
            this.request = request;
            this.startTime = startTime;
        }

        public bool IsCool()
        {
            return startTime + cooldownTime < Time.time;
        }
    }

    [System.Serializable]
    private class SaveData : GenericSaveData<RequestPool<T>>
    {
        public HashSet<T> requests = new HashSet<T>();
        public HashSet<T> handedOutRequests = new HashSet<T>();
    }
    protected Queue<RequestOnCooldown> coolingDownRequests = new Queue<RequestOnCooldown>();

    SaveData data = new SaveData();

    readonly protected object lockObject = new object();

    protected HashSet<T> Requests { get => data.requests; }
    protected HashSet<T> HandedOutRequests { get => data.handedOutRequests; }
    protected Queue<RequestOnCooldown> CoolingDownRequests { get => coolingDownRequests; }

    protected void ReturnCooledDownTasks(bool returnAll)
    {
        lock (lockObject)
        {
            while (CoolingDownRequests.Count > 0)
            {
                if (CoolingDownRequests.Peek().IsCool() || returnAll)
                {
                    Requests.Add(CoolingDownRequests.Dequeue().request);
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

    public bool PostRequest(T request)
    {
        OnRequestAdded(request);
        lock (lockObject)
        {
            return Requests.Add(request);
        }
    }

    protected virtual void OnRequestAdded(T request)
    {
    }

    protected virtual void OnRequestRemoved(T request)
    {

    }

    public void ReturnRequest(T request)
    {
        Debug.Log("Returned request of type " + typeof(T).ToString());
        lock (lockObject)
        {
            HandedOutRequests.Remove(request);
            CoolingDownRequests.Enqueue(new RequestOnCooldown(request, Time.time));
        }
    }

    public void CancelRequest(T request)
    {
        Debug.Log("Cancelled request of type " + typeof(T).ToString());

        lock (lockObject)
        {
            request.Cancel();
            HandedOutRequests.Remove(request);
        }
        OnRequestRemoved(request);
    }

    public void FinishRequest(T request)
    {
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
        lock (lockObject)
        {
            Requests.Remove(request);
            HandedOutRequests.Add(request);
        }
    }

    public abstract T GetRequest(GridActor actor);
}
