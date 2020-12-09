
using Items;
using UnityEngine;

[System.Serializable]
public abstract class SolidBlock : Block
{
    protected SolidBlock(BlockType type) : base(type)
    {
    }

    protected SolidBlock(BlockType type, Vector3 rotation) : base(type, rotation)
    {
    }
    public override PartMeshInfo GetMesh(Vector3Int thisBlockPos, int maxY, IHasBlocks blockOwner)
    {
        return GenerateMesh(thisBlockPos, maxY, blockOwner);
    }

    public override bool isVisible()
    {
        return true;
    }

    public override bool supportsClimbing()
    {
        return false;
    }

    public override bool supportsWalkingOnTop()
    {
        return true;
    }

    public override bool supportsWalkingThrough()
    {
        return false;
    }
}