
using Items;
using UnityEngine;

[System.Serializable]
public class AirBlock : Block
{
    public AirBlock() : base(BlockType.airBlock) { }
    public AirBlock(Vector3 rotation) : base(BlockType.airBlock, rotation) { }

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

    public override bool isVisible()
    {
        return false;
    }

    public override bool supportsClimbing()
    {
        return false;
    }

    public override bool supportsWalkingOnTop()
    {
        return false;
    }

    public override bool supportsWalkingThrough()
    {
        return true;
    }
}