
using System;
using System.Collections.Generic;
using UnityEngine;

public class GridActor : MonoBehaviour, ISaveableComponent, IHasGuid, IObservable<Vector3Int>
{
    [System.Serializable]

    public class SaveData : GenericSaveData<GridActor>
    {
        public Vector3Int position;
        public Vector3Int size;
        public Guid guid;
    }

    // Size and position extracted from SaveData to be viewable in editor
    [SerializeField]
    private Vector3Int size = new Vector3Int(1, 1, 1);

    [SerializeField]
    private Vector3Int position;

    bool registered = false;

    SaveData data = new SaveData();

    public Vector3Int Position { get => position; }
    public Vector3Int Size { get => size; }

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

    public void SetSize(Vector3Int newSize)
    {
        if (size != newSize)
        {
            UnregisterMe();
            size = newSize;
            RegisterMe();
        }
    }

    public void Move(Vector3Int newPos)
    {
        if (position != newPos)
        {
            UnregisterMe();
            position = newPos;
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
        Helpers.ForEachPosition(this.Position, this.Size, (position) =>
        {
            GridActorMap.RegisterGridActor(this, position);
        });
        registered = true;
    }
    void UnregisterMe()
    {
        if (registered)
        {
            Helpers.ForEachPosition(this.Position, this.Size, (position) =>
            {
                GridActorMap.UnregisterGridActor(this, position);
            });
        }
    }

    void UpdateObservers()
    {
        foreach (var observer in observers)
        {
            observer.OnNext(position);
        }
    }

    public IGenericSaveData Save()
    {
        data.position = Position;
        data.size = Size;
        return data;
    }

    public void Load(IGenericSaveData data)
    {
        this.data = (SaveData)data;
        position = this.data.position;
        size = this.data.size;
    }

    public IDisposable Subscribe(IObserver<Vector3Int> observer)
    {
        return new GenericUnsubscriber<Vector3Int>(observers, observer);
    }
}