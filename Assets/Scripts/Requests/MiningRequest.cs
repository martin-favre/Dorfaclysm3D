
using System;
using UnityEngine;

[System.Serializable]
public class MiningRequest : PlayerRequest, IEquatable<MiningRequest>
{
    readonly Block.BlockType blockType;
    readonly Vector3Int position;

    public Vector3Int Position => position;

    public Block.BlockType BlockType => blockType;

    public MiningRequest(Vector3Int position, Block.BlockType blockType)
    {
        Debug.Assert(position != null);
        this.position = position;
        this.blockType = blockType;
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

    public bool Equals(MiningRequest other)
    {
        return position == other.position;
    }
}