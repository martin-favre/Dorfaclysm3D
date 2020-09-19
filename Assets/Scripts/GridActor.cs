
using UnityEngine;

public class GridActor
{

    GameObject owner;
    Vector3Int gridPosition;
    public GridActor(GameObject owner, Vector3Int gridPosition)
    {
        this.owner = owner;
        this.gridPosition = gridPosition;
        RegisterMe();
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

    public Vector3Int GetPos(){
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