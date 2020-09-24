
using UnityEngine;

[System.Serializable]
public abstract class Block : System.ICloneable, System.IEquatable<Block>
{
    public enum BlockType
    {
        invalid,
        rockBlock,
        airBlock,
        grassBlock,
        stairUpDownBlock
    }


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

