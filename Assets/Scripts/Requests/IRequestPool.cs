using System;

public interface IRequestPool<T> : IObservable<RequestPoolUpdateEvent<T>>, IKeyObservable<RequestPoolUpdateEvent<T>, Guid> where T : PlayerRequest
{
    // Post a new available request
    bool PostRequest(T request);

    // Return an unfinished request to be available again.
    void ReturnRequest(T request);

    // Finish a completed request
    void FinishRequest(T request);

    // If any requests are available
    bool HasRequests();

    // Get a request from the pool. If multiple requests are available the result 
    // depends on the GridActor.
    // Returns null if no request is available
    T GetRequest(GridActor actor);

    IGenericSaveData GetSave();
    void Load(IGenericSaveData save);
}