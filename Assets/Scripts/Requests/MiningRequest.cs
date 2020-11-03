
using UnityEngine;

[System.Serializable]
public class MiningRequest : PlayerRequest
{
    readonly Block.BlockType blockType;
    readonly Vector3Int position;

    public Vector3Int Position => position;

    public Block.BlockType BlockType => blockType;

    public MiningRequest(Vector3Int position, Block.BlockType blockType){
        Debug.Assert(position != null);
        this.position = position;
        this.blockType = blockType;
    }

    public override bool Equals(object other)
    {
        // explicitly ignore blocktype, we can't have two
        // mining requests on the same block.
        return Position == ((MiningRequest)other).Position;
    }

    public override void Finish()
    {
        Cancel();
    }

    public override int GetHashCode()
    {
        return position.GetHashCode();
    }

    public override string ToString()
    {
        return "MiningRequest pos: " + position + " block: " + blockType;
    }

}