
using System;
using Items;
using Logging;
using UnityEngine;

[System.Serializable]
public class StairUpDownBlock : Block
{
    static readonly Vector2 texturePos = TexturePositions.StairUpDownBlock;


    public StairUpDownBlock(Vector3 rotation) : base(rotation)
    {

    }

    public override object Clone()
    {
        return new StairUpDownBlock(Rotation);
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
        Vector2 blockEffect = BlockEffectMap.Instance.GetBlockEffect(currPos);
        PartMeshInfo meshInfo = new PartMeshInfo();
        RenderRefMesh(BlockMeshes.FullStair, currPos, Rotation, GetTexturePos(), blockEffect, meshInfo);
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

    public override bool IsVisible()
    {
        return true;
    }

    public override bool SupportsClimbing()
    {
        return true;
    }

    public override bool SupportsWalkingOnTop()
    {
        return true;
    }

    public override bool SupportsWalkingThrough()
    {
        return true;
    }

    public override bool NeighbourShouldBeRendered()
    {
        return true;
    }
}