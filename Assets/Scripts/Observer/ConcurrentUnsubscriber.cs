using System;
using System.Collections.Concurrent;

public class ConcurrentUnsubscriber<T> : IDisposable
{
    // I actually want a concurrentList, and I can't see any interface that has a thread-safe add and remove.   
    ConcurrentDictionary<IObserver<T>, IObserver<T>> allObserversRef;
    IObserver<T> myObserver;
    public ConcurrentUnsubscriber(ConcurrentDictionary<IObserver<T>, IObserver<T>> allObserversRef, IObserver<T> myObserver)
    {
        this.allObserversRef = allObserversRef;
        this.myObserver = myObserver;
        bool success = allObserversRef.TryAdd(myObserver, myObserver);
        if(!success) throw new Exception("Unable to add item in collection");
    }
    public void Dispose()
    {
        allObserversRef.TryRemove(myObserver, out _);
    }
}
