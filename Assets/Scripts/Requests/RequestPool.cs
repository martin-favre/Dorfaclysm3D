using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class RequestPool<T> where T : PlayerRequest
{
    protected HashSet<T> requests = new HashSet<T>();
    protected HashSet<T> handedOutRequests = new HashSet<T>();
    public bool PostRequest(T request)
    {
        return requests.Add(request);
    }

    public void ReturnRequest(T request)
    {
        handedOutRequests.Remove(request);
        requests.Add(request);
    }

    public void CancelRequest(T request) {
        request.Cancel();
        handedOutRequests.Remove(request);
    }

    public void FinishRequest(T request) {
        handedOutRequests.Remove(request);
        request.Finish();
    }

    public bool HasRequests() {
        return requests.Count != 0;
    }

    protected void HandOutRequest(T request) {
        requests.Remove(request);
        handedOutRequests.Add(request);
    }

    public abstract T GetRequest(GridActor actor);
}
