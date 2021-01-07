namespace System
{
    public interface IKeyObservable<out T, Key>
    {
        IDisposable Subscribe(IObserver<T> observer, Key key);
    }
}