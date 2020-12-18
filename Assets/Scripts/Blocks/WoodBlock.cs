
using Items;
using UnityEngine;

[System.Serializable]
public class WoodBlock : SolidBlock
{
    static readonly Vector2 texturePos = TexturePositions.WoodBlock;
    public WoodBlock() : base() { }
    public WoodBlock(Vector3 rotation) : base(rotation) { }

    public override object Clone()
    {
        return new WoodBlock();
    }

    public override Item GetItem()
    {
        return new WoodBlockItem();
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