
using UnityEngine;

public class GridActor
{
    [System.Serializable]
    public class SaveData
    {
        public SerializeableVector3Int position;
    }


    GameObject owner;
    Vector3Int gridPosition;
    public GridActor(GameObject owner, Vector3Int gridPosition)
    {
        this.owner = owner;
        this.gridPosition = gridPosition;
        RegisterMe();
    }

    public GridActor(GameObject owner, SaveData save) {
        SaveData savedata = (SaveData) save;
        gridPosition = savedata.position.Get();
        RegisterMe();
    }   


    public SaveData GetSave()
    {
        SaveData save = new SaveData();
        save.position = new SerializeableVector3Int(gridPosition);
        return save;
    }


    ~GridActor()
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
    }
    void UnregisterMe()
    {
        GridActorMap.UnregisterGridActor(this, gridPosition);
    }


}