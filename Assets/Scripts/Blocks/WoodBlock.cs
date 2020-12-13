
using Items;
using UnityEngine;

[System.Serializable]
public class WoodBlock : SolidBlock
{
    static readonly Vector2 texturePos = TexturePositions.Get(TexturePositions.Name.WoodBlock);
    public WoodBlock() : base(BlockType.woodBlock) { }

    public override object Clone()
    {
        return new WoodBlock();
    }

    public override Item GetItem()
    {
        return new RockBlockItem();
    }

    public override string GetName()
    {
        return "Wood Block";
    }

    public override Vector2 GetTexturePos()
    {
        return texturePos;
    }
}