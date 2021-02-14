
using System;

class SimpleObserver<T> : IObserver<T>
{
    private readonly IObservable<T> source;
    private readonly Action<T> onNext;
    private readonly IDisposable subscription;
    bool completed = false;

    public SimpleObserver(IObservable<T> source, Action<T> onNext)
    {
        this.source = source;
        this.onNext = onNext;
        this.subscription = this.source.Subscribe(this);
    }

    ~SimpleObserver()
    {
        if (!completed)
        {
            this.subscription.Dispose();
        }
    }

    public void OnCompleted()
    {
        completed = true;
        this.subscription.Dispose();
    }

    public void OnError(Exception error)
    {
        throw error;
    }

    public void OnNext(T value)
    {
        onNext(value);
    }
}