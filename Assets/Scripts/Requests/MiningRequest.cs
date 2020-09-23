
using UnityEngine;

[System.Serializable]
public class MiningRequest : PlayerRequest
{
    readonly Block.BlockType blockType;
    readonly Vector3Int position;

    public Vector3Int Position => position;

    public Block.BlockType BlockType => blockType;

    public MiningRequest(Vector3Int position, Block.BlockType blockType){
        this.position = position;
        this.blockType = blockType;
    }

    public override bool Equals(object other)
    {
        return Position == ((MiningRequest)other).Position;
    }

    public override void Finish()
    {
        Cancel();
    }

    public override int GetHashCode()
    {
        throw new System.NotImplementedException();
    }
}