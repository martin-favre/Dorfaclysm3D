
using Items;
using UnityEngine;

[System.Serializable]
public class WaterBlock : Block
{
    static readonly Vector2 texturePos = TexturePositions.Get(TexturePositions.Name.WaterBlock);
    public WaterBlock() : base(BlockType.waterBlock) { }

    public override object Clone()
    {
        return new WaterBlock();
    }

    public override Item GetItem()
    {
        return new RockBlockItem();
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

    public override bool isVisible()
    {
        return true;
    }

    public override bool supportsClimbing()
    {
        return true;
    }

    public override bool supportsWalkingOnTop()
    {
        return false;
    }

    public override bool supportsWalkingThrough()
    {
        return false;
    }
}