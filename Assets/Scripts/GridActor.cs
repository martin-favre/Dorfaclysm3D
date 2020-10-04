
using UnityEngine;

public class GridActor : MonoBehaviour, ISaveableComponent
{
    [System.Serializable]
    public class SaveData : GenericSaveData<GridActor>
    {
        public Vector3Int position;
    }

    // public to be viewed in inspector
    public Vector3Int gridPosition;
    bool registered = false;

    void Start()
    {
        RegisterMe();
    }

    void OnDestroy()
    {
        UnregisterMe();
        Debug.Log("GridActor, OnDestroy");
    }

    public void Move(Vector3Int newPos)
    {
        UnregisterMe();
        gridPosition = newPos;
        RegisterMe();

    }

    public bool IsBlocking()
    {
        return false;
    }

    public Vector3Int GetPos()
    {
        return gridPosition;
    }

    void RegisterMe()
    {
        GridActorMap.RegisterGridActor(this, gridPosition);
        registered = true;
    }
    void UnregisterMe()
    {
        if (registered)
        {
            GridActorMap.UnregisterGridActor(this, gridPosition);
        }
    }

    public IGenericSaveData Save()
    {
        SaveData save = new SaveData();
        save.position = (gridPosition);
        return save;
    }

    public void Load(IGenericSaveData data)
    {
        SaveData savedata = (SaveData)data;
        Move(savedata.position);
    }
}