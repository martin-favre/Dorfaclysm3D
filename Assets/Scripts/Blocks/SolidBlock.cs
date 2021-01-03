
using Items;
using UnityEngine;

[System.Serializable]
public abstract class SolidBlock : Block
{
    protected SolidBlock() : base()
    {
    }

    protected SolidBlock( Vector3 rotation) : base( rotation)
    {
    }
    public override PartMeshInfo GetMesh(Vector3Int thisBlockPos, int maxY, IHasBlocks blockOwner)
    {
        return GenerateMesh(thisBlockPos, maxY, blockOwner);
    }

    public override bool IsVisible()
    {
        return true;
    }

    public override bool SupportsClimbing()
    {
        return false;
    }

    public override bool SupportsWalkingOnTop()
    {
        return true;
    }

    public override bool SupportsWalkingThrough()
    {
        return false;
    }
    public override bool NeighbourShouldBeRendered()
    {
        return false;
    }

}