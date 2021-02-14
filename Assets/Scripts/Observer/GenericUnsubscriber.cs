using System;
using System.Collections.Generic;

public class GenericUnsubscriber<T> : IDisposable
{
    ICollection<IObserver<T>> allObserversRef;
    IObserver<T> myObserver;
    public GenericUnsubscriber(ICollection<IObserver<T>> allObserversRef, IObserver<T> myObserver)
    {
        this.allObserversRef = allObserversRef;
        this.myObserver = myObserver;
        allObserversRef.Add(myObserver);
    }
    public void Dispose()
    {
        if (allObserversRef.Contains(myObserver))
        {
            allObserversRef.Remove(myObserver);
        }
    }
}
