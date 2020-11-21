
using Items;
using UnityEngine;

[System.Serializable]
public class RockBlock : Block
{
    static readonly Vector2 texturePos = TexturePositions.Get(TexturePositions.Name.RockBlock);
    public RockBlock() : base(BlockType.rockBlock)
    {

    }

    public override object Clone()
    {
        return new RockBlock();
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
        return "Rock Block";
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