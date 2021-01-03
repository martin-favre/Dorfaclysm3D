
using Items;
using UnityEngine;

[System.Serializable]
public class WaterBlock : Block
{
    static readonly Vector2 texturePos = TexturePositions.WaterBlock;
    public WaterBlock() : base() { }

    public override object Clone()
    {
        return new WaterBlock();
    }

    public override Item GetItem()
    {
        return null;
    }

    public override PartMeshInfo GetMesh(Vector3Int thisBlockPos, int maxY, IHasBlocks blockOwner)
    {
        return GenerateMesh(thisBlockPos, maxY, blockOwner);
    }

    public override string GetName()
    {
        return "Water Block";
    }

    public override Vector2 GetTexturePos()
    {
        return texturePos;
    }

    public override bool IsVisible()
    {
        return true;
    }

    public override bool NeighbourShouldBeRendered()
    {
        return true;
    }

    public override bool SupportsClimbing()
    {
        return true;
    }

    public override bool SupportsWalkingOnTop()
    {
        return false;
    }

    public override bool SupportsWalkingThrough()
    {
        return false;
    }
}