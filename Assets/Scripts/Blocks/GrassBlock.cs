
using Items;
using UnityEngine;

[System.Serializable]
public class GrassBlock : Block
{
    static readonly Vector2 texturePos = TexturePositions.Get(TexturePositions.Name.GrassBlock);
    public GrassBlock() : base(BlockType.grassBlock) { }

    public override object Clone()
    {
        return new GrassBlock();
    }

    public override Item GetItem()
    {
        return null;
    }

    public override string GetName()
    {
        return "Grass Block";
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