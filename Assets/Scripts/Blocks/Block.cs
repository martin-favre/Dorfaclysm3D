
using Items;
using UnityEngine;

[System.Serializable]
public abstract partial class Block : System.ICloneable, System.IEquatable<Block>
{
    private readonly BlockType type = BlockType.invalid;

    public BlockType Type { get => type; }

    public Block(BlockType type)
    {
        this.type = type;
    }

    // Position in the spritesheet
    public abstract Vector2 GetTexturePos();

    // Can you walk through this block?
    public abstract bool supportsWalkingThrough();

    // Can you ascend from this block?
    public abstract bool supportsClimbing();

    //Can you walk on top of this block?
    public abstract bool supportsWalkingOnTop();
    public abstract string GetName();
    public abstract bool isVisible();

    public abstract IItem GetItem();

    public abstract object Clone();

    public bool Equals(Block other)
    {
        return other.type == type;
    }

    public override bool Equals(object obj)
    {
        Block b = (Block)obj;
        return b.type == type;
    }
    
    public override int GetHashCode()
    {
        return type.GetHashCode();
    }
}

