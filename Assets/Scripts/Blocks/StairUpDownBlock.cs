
using Items;
using UnityEngine;

[System.Serializable]
public class StairUpDownBlock : Block
{
    static readonly Vector2 texturePos = TexturePositions.Get(TexturePositions.Name.StairUpDownBlock);
    public StairUpDownBlock() : base(BlockType.stairUpDownBlock)
    {

    }

    public override object Clone()
    {
        return new StairUpDownBlock();
    }

    public override Item GetItem()
    {
        return null;
    }

    public override string GetName()
    {
        return "Up/Down Stair";
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
        return true;
    }

    public override bool supportsWalkingThrough()
    {
        return true;
    }
}