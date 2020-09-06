
using UnityEngine;

[System.Serializable]
public abstract class Block : System.ICloneable
{
    public enum BlockType
    {
        invalid,
        rockBlock,
        airBlock,
        grassBlock,
        stairUpDownBlock
    }


    private BlockType mType = BlockType.invalid;

    public BlockType Type { get => mType; }

    public Block(BlockType type)
    {
        mType = type;
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
}

