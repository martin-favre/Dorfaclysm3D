
using Items;
using UnityEngine;

[System.Serializable]
public class LeafBlock : SolidBlock
{
    static readonly Vector2 texturePos = TexturePositions.Get(TexturePositions.Name.LeafBlock);
    public LeafBlock() : base(BlockType.leafBlock) { }

    public override object Clone()
    {
        return new LeafBlock();
    }

    public override Item GetItem()
    {
        return new RockBlockItem();
    }

    public override string GetName()
    {
        return "Leaf Block";
    }

    public override Vector2 GetTexturePos()
    {
        return texturePos;
    }
}