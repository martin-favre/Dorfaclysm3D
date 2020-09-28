
using Items;
using UnityEngine;

[System.Serializable]
public class AirBlock : Block
{
    public AirBlock() : base(BlockType.airBlock) { }

    public override object Clone()
    {
        return new AirBlock();
    }

    public override IItem GetItem()
    {
        return null;
    }

    public override string GetName()
    {
        return "Air Block";
    }

    public override Vector2 GetTexturePos()
    {
        return Vector2.zero; // Has no texture
    }

    public override bool isVisible()
    {
        return false;
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