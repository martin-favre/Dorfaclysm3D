using System;
using System.Collections.Generic;

public class GenericUnsubscriber<T> : IDisposable
{
    List<IObserver<T>> allObserversRef;
    IObserver<T> myObserver;
    public GenericUnsubscriber(List<IObserver<T>> allObserversRef, IObserver<T> myObserver)
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
