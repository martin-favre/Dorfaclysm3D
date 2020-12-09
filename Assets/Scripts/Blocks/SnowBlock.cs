
using Items;
using UnityEngine;

[System.Serializable]
public class SnowBlock : SolidBlock
{
    static readonly Vector2 texturePos = TexturePositions.Get(TexturePositions.Name.SnowBlock);
    public SnowBlock() : base(BlockType.snowBlock) { }

    public override object Clone()
    {
        return new SnowBlock();
    }

    public override Item GetItem()
    {
        return new RockBlockItem();
    }


    public override string GetName()
    {
        return "Snow Block";
    }

    public override Vector2 GetTexturePos()
    {
        return texturePos;
    }
}