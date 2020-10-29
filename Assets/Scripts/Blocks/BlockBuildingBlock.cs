
using Items;
using UnityEngine;

[System.Serializable]
public class BlockBuildingBlock : Block
{
    static readonly Vector2 texturePos = TexturePositions.Get(TexturePositions.Name.BlockBuildingBlock);
    public BlockBuildingBlock() : base(BlockType.blockBuildingBlock) { }

    public override object Clone()
    {
        return new BlockBuildingBlock();
    }

    public override Item GetItem()
    {
        return null;
    }

    public override string GetName()
    {
        return "Block Building Block";
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
        return false;
    }

    public override bool supportsWalkingThrough()
    {
        return true;
    }
}