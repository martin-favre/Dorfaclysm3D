
using System;

[System.Serializable]
public abstract class PlayerRequest
{
    private bool cancelled = false;

    public void Cancel()
    {
        cancelled = true;
    }

    public bool IsCancelled()
    {
        return cancelled;
    }

    public abstract void Finish();

    public abstract override bool Equals(object obj);

    public abstract override int GetHashCode();

}
