
using System;
using UnityEngine;

public abstract class DestroyableObject
{

    public void Destroy(GameObject gObj)
    {
        OnDestroyed();
        GameObject.Destroy(gObj);
    }

    protected abstract void OnDestroyed();
}