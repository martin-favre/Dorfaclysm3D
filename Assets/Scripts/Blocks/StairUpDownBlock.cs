
using System;
using Items;
using Logging;
using UnityEngine;

[System.Serializable]
public class StairUpDownBlock : Block
{
    static readonly Vector2 texturePos = TexturePositions.Get(TexturePositions.Name.StairUpDownBlock);


    public StairUpDownBlock() : base(BlockType.stairUpDownBlock)
    {

    }

    public override object Clone()
    {
        return new StairUpDownBlock();
    }

    public override Item GetItem()
    {
        return null;
    }

    public override PartMeshInfo GetMesh(Vector3Int thisBlockPos, int maxY, IHasBlocks blockOwner)
    {
        return renderStair(thisBlockPos, maxY, blockOwner);
    }


    private PartMeshInfo renderStair(Vector3Int currPos, int maxY, IHasBlocks blockOwner)
    {
        //This code will run for every block in the chunk
        int x = currPos.x;
        int y = currPos.y;
        int z = currPos.z;
        Block neighbourBlock = GetBlock(new Vector3Int(x, y + 1, z), maxY, blockOwner);
        Vector2 blockEffect = BlockEffectMap.GetBlockEffect(currPos);
        PartMeshInfo meshInfo = new PartMeshInfo();
        
        RenderRefMesh(BlockMeshes.FullStair, currPos, GetTexturePos(), blockEffect, meshInfo);
        return meshInfo;
    }

    public override string GetName()
    {
        return "Up/Down Stair";
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
        return true;
    }

    public override bool supportsWalkingOnTop()
    {
        return true;
    }

    public override bool supportsWalkingThrough()
    {
        return true;
    }

}