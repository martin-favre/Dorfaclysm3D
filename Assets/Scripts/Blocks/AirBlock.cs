
using Items;
using UnityEngine;

[System.Serializable]
public class AirBlock : Block
{
    public AirBlock() : base() { }
    public AirBlock(Vector3 rotation) : base(rotation) { }

    public override object Clone()
    {
        return new AirBlock();
    }

    public override Item GetItem()
    {
        return null;
    }

    public override PartMeshInfo GetMesh(Vector3Int thisBlockPos, int maxY, IHasBlocks blockOwner)
    {
        return new PartMeshInfo(); // Can't render air yo
    }

    public override string GetName()
    {
        return "Air Block";
    }

    public override Vector2 GetTexturePos()
    {
        return Vector2.zero; // Has no texture
    }

    public override bool IsVisible()
    {
        return false;
    }

    public override bool NeighbourShouldBeRendered()
    {
        return true;
    }

    public override bool SupportsClimbing()
    {
        return false;
    }

    public override bool SupportsWalkingOnTop()
    {
        return false;
    }

    public override bool SupportsWalkingThrough()
    {
        return true;
    }
}