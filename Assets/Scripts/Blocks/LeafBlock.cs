
using Items;
using UnityEngine;

[System.Serializable]
public class LeafBlock : SolidBlock
{
    static readonly Vector2 texturePos = TexturePositions.LeafBlock;
    public LeafBlock() : base() { }

    public override object Clone()
    {
        return new LeafBlock();
    }

    public override Item GetItem()
    {
        return null;
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