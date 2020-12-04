
using System;

[System.Serializable]
public abstract class PlayerRequest : IHasGuid
{
    private bool cancelled = false;
    protected readonly Guid guid = Guid.NewGuid();

    public Guid Guid => guid;
    public void Cancel()
    {
        cancelled = true;
    }

    public bool IsCancelled()
    {
        return cancelled;
    }

    public abstract void Finish();

    public abstract override int GetHashCode();

}
