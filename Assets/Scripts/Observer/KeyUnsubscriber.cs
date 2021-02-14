using System;
using System.Collections.Generic;

public class KeyUnsubscriber<T, Key> : IDisposable
{
    readonly Dictionary<Key, IObserver<T>> allObserversRef;
    readonly IObserver<T> myObserver;
    readonly Key key;
    public KeyUnsubscriber(Dictionary<Key, IObserver<T>> allObserversRef, Key key, IObserver<T> myObserver)
    {
        this.allObserversRef = allObserversRef;
        this.myObserver = myObserver;
        this.key = key;
        allObserversRef[key] = myObserver;
    }
    public void Dispose()
    {
        allObserversRef.Remove(key);
    }
}
