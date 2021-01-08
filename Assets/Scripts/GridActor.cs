
using System;
using System.Collections.Generic;
using UnityEngine;

public class GridActor : MonoBehaviour, ISaveableComponent, IHasGuid, IObservable<Vector3Int>
{
    [System.Serializable]
    public class SaveData : GenericSaveData<GridActor>
    {
        public Vector3Int position;
        public Guid guid;
    }

    // public to be viewed in inspector
    bool registered = false;

    SaveData data = new SaveData();

    public Vector3Int Position { get => data.position; }

    List<IObserver<Vector3Int>> observers = new List<IObserver<Vector3Int>>();

    public Guid Guid
    {
        get
        {
            if (data.guid == Guid.Empty) data.guid = Guid.NewGuid();
            return data.guid;
        }
    }

    void Start()
    {
        if (!registered)
        {
            RegisterMe();
        }
    }

    void OnDestroy()
    {
        UnregisterMe();
    }

    public void Move(Vector3Int newPos)
    {
        if (data.position != newPos)
        {
            UnregisterMe();
            data.position = newPos;
            RegisterMe();
            UpdateObservers();
        }
    }

    public bool IsBlocking()
    {
        return false;
    }

    void RegisterMe()
    {
        GridActorMap.RegisterGridActor(this, data.position);
        registered = true;
    }
    void UnregisterMe()
    {
        if (registered)
        {
            GridActorMap.UnregisterGridActor(this, data.position);
        }
    }

    void UpdateObservers()
    {
        foreach(var observer in observers) {
            observer.OnNext(data.position);
        }
    }

    public IGenericSaveData Save()
    {
        return data;
    }

    public void Load(IGenericSaveData data)
    {
        this.data = (SaveData)data;
    }

    public IDisposable Subscribe(IObserver<Vector3Int> observer)
    {
        return new GenericUnsubscriber<Vector3Int>(observers, observer);
    }
}