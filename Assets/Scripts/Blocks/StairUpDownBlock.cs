
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
        bool stepsRendered = false; // Steps are visible from multiple directions, but only need to be rendered once
        if (FaceShouldBeRendered(neighbourBlock))
        {
            //Block above is air
            if (BlockMeshes.StairSteps != null)
            {
                RenderRefMesh(BlockMeshes.StairSteps, currPos, GetTexturePos(), blockEffect, meshInfo);
            }
            stepsRendered = true;
        }
        neighbourBlock = GetBlock(new Vector3Int(x, y - 1, z), maxY, blockOwner);
        if (FaceShouldBeRendered(neighbourBlock))
        {
            //Block below is air
            CubeBot(currPos, meshInfo);

        }

        neighbourBlock = GetBlock(new Vector3Int(x + 1, y, z), maxY, blockOwner);
        if (FaceShouldBeRendered(neighbourBlock))
        {
            //Block east is air
            CubeEast(currPos, meshInfo);
        }

        neighbourBlock = GetBlock(new Vector3Int(x, y, z - 1), maxY, blockOwner);
        if (FaceShouldBeRendered(neighbourBlock))
        {
            //Block north is air
            RenderRefMesh(BlockMeshes.StairSide, currPos, GetTexturePos(), blockEffect, meshInfo);
            if (!stepsRendered)
            {
                RenderRefMesh(BlockMeshes.StairSide, currPos, GetTexturePos(), blockEffect, meshInfo);
                stepsRendered = true;
            }
        }
        neighbourBlock = GetBlock(new Vector3Int(x, y, z + 1), maxY, blockOwner);
        if (FaceShouldBeRendered(neighbourBlock))
        {
            RenderRefMesh(BlockMeshes.StairSide2, currPos, GetTexturePos(), blockEffect, meshInfo);
            if (!stepsRendered)
            {
                RenderRefMesh(BlockMeshes.StairSide2, currPos, GetTexturePos(), blockEffect, meshInfo);
                stepsRendered = true;
            }

        }
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