
using UnityEngine;

public class GridActor : MonoBehaviour, ISaveableComponent
{
    [System.Serializable]
    public class SaveData : GenericSaveData<GridActor>
    {
        public SerializeableVector3Int position;

    }

    GameObject owner;
    Vector3Int gridPosition;
    bool registered = false;

    void Start()
    {
        RegisterMe();
    }

    void OnDestroy()
    {
        UnregisterMe();
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
    public GameObject GetOwner()
    {
        return owner;
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
        save.position = new SerializeableVector3Int(gridPosition);
        return save;
    }

    public void Load(IGenericSaveData data)
    {
        SaveData savedata = (SaveData)data;
        Move(savedata.position.Get());
    }
}