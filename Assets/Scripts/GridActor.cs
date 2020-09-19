
using UnityEngine;

public class GridActor
{

    GameObject mOwner;
    Vector3Int mGridPosition;
    public GridActor(GameObject owner, Vector3Int gridPosition)
    {
        mOwner = owner;
        mGridPosition = gridPosition;
        RegisterMe();
    }

    ~GridActor()
    {
        UnregisterMe();
    }

    public void Move(Vector3Int newPos)
    {
        UnregisterMe();
        mGridPosition = newPos;
        RegisterMe();
    }

    public bool IsBlocking()
    {
        return false;
    }
    public GameObject GetOwner()
    {
        return mOwner;
    }

    public Vector3Int GetPos(){
        return mGridPosition;
    }

    void RegisterMe()
    {
        GridActorMap.RegisterGridActor(this, mGridPosition);
    }
    void UnregisterMe()
    {
        GridActorMap.UnregisterGridActor(this, mGridPosition);
    }


}