
using System.Collections.Generic;
using Items;
using Logging;
using UnityEngine;

[System.Serializable]
public abstract partial class Block : System.ICloneable, System.IEquatable<Block>
{
    protected class Face
    {
        public readonly Vector3 topLeft;
        public readonly Vector3 topRight;
        public readonly Vector3 bottomRight;
        public readonly Vector3 bottomLeft;

        public Face(Vector3 topRight, Vector3 bottomRight, Vector3 bottomLeft, Vector3 topLeft)
        {
            this.bottomLeft = bottomLeft;
            this.topRight = topRight;
            this.bottomRight = bottomRight;
            this.topLeft = topLeft;
        }
    }
    protected static readonly LilLogger logger = new LilLogger("Blocks");
    private const float textureUnit = 0.5f;
    private readonly BlockType type = BlockType.invalid;

    private readonly Vector3 rotation = Vector3.zero;

    public BlockType Type => type;

    public Vector3 Rotation => rotation;

    public Block(BlockType type)
    {
        this.type = type;
    }
    public Block(BlockType type, Vector3 rotation)
    {
        this.type = type;
        this.rotation = rotation;
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

    public abstract Item GetItem();

    public abstract object Clone();

    public abstract PartMeshInfo GetMesh(Vector3Int thisBlockPos, int maxY, IHasBlocks blockOwner);
    protected bool FaceShouldBeRendered(Block neighbour)
    {
        return neighbour == null || neighbour.Type == Block.BlockType.airBlock || neighbour.Type == Block.BlockType.stairUpDownBlock;
    }

    protected Block GetBlock(Vector3Int pos, int maxY, IHasBlocks blockOwner)
    {
        Block block = null;
        if (pos.y > maxY) return new AirBlock();
        blockOwner.TryGetBlock(pos, out block);
        return block;
    }



    protected PartMeshInfo GenerateMesh(Vector3Int thisBlockPos, int maxY, IHasBlocks blockOwner)
    {
        int x = thisBlockPos.x;
        int y = thisBlockPos.y;
        int z = thisBlockPos.z;
        PartMeshInfo meshInfo = new PartMeshInfo();
        Block neighbourBlock = GetBlock(new Vector3Int(x, y + 1, z), maxY, blockOwner);
        if (FaceShouldBeRendered(neighbourBlock))
        {
            //Block above is air
            CubeTop(thisBlockPos, meshInfo);
        }
        neighbourBlock = GetBlock(new Vector3Int(x, y - 1, z), maxY, blockOwner);
        if (FaceShouldBeRendered(neighbourBlock))
        {
            //Block below is air
            CubeBot(thisBlockPos, meshInfo);

        }

        neighbourBlock = GetBlock(new Vector3Int(x + 1, y, z), maxY, blockOwner);
        if (FaceShouldBeRendered(neighbourBlock))
        {
            //Block east is air
            CubeEast(thisBlockPos, meshInfo);

        }

        neighbourBlock = GetBlock(new Vector3Int(x - 1, y, z), maxY, blockOwner);
        if (FaceShouldBeRendered(neighbourBlock))
        {
            //Block west is air
            CubeWest(thisBlockPos, meshInfo);

        }

        neighbourBlock = GetBlock(new Vector3Int(x, y, z + 1), maxY, blockOwner);
        if (FaceShouldBeRendered(neighbourBlock))
        {
            //Block north is air
            CubeNorth(thisBlockPos, meshInfo);

        }
        neighbourBlock = GetBlock(new Vector3Int(x, y, z - 1), maxY, blockOwner);
        if (FaceShouldBeRendered(neighbourBlock))
        {
            //Block south is air
            CubeSouth(thisBlockPos, meshInfo);
        }

        return meshInfo;

    }
    protected void CubeTop(Vector3Int pos, PartMeshInfo meshInfo)
    {
        Face face = new Face(
            new Vector3(pos.x, pos.y, pos.z + 1),
            new Vector3(pos.x + 1, pos.y, pos.z + 1),
            new Vector3(pos.x + 1, pos.y, pos.z),
            pos);
        MakeFace(face, meshInfo);
        SetUvs(GetTexturePos(), BlockEffectMap.GetBlockEffect(pos), meshInfo);

    }

    protected void CubeNorth(Vector3Int pos, PartMeshInfo meshInfo)
    {
        Face face = new Face(
            new Vector3(pos.x + 1, pos.y - 1, pos.z + 1),
            new Vector3(pos.x + 1, pos.y, pos.z + 1),
            new Vector3(pos.x, pos.y, pos.z + 1),
            new Vector3(pos.x, pos.y - 1, pos.z + 1));

        MakeFace(face, meshInfo);
        SetUvs(GetTexturePos(), BlockEffectMap.GetBlockEffect(pos), meshInfo);

    }

    protected void CubeEast(Vector3Int pos, PartMeshInfo meshInfo)
    {

        Face face = new Face(
            new Vector3(pos.x + 1, pos.y - 1, pos.z),
            new Vector3(pos.x + 1, pos.y, pos.z),
            new Vector3(pos.x + 1, pos.y, pos.z + 1),
            new Vector3(pos.x + 1, pos.y - 1, pos.z + 1));

        MakeFace(face, meshInfo);
        SetUvs(GetTexturePos(), BlockEffectMap.GetBlockEffect(pos), meshInfo);
    }

    protected void CubeSouth(Vector3Int pos, PartMeshInfo meshInfo)
    {
        Face face = new Face(
            new Vector3(pos.x, pos.y - 1, pos.z),
            pos,
            new Vector3(pos.x + 1, pos.y, pos.z),
            new Vector3(pos.x + 1, pos.y - 1, pos.z));
        MakeFace(face, meshInfo);
        SetUvs(GetTexturePos(), BlockEffectMap.GetBlockEffect(pos), meshInfo);

    }

    protected void CubeWest(Vector3Int pos, PartMeshInfo meshInfo)
    {
        Face face = new Face(
            new Vector3(pos.x, pos.y - 1, pos.z + 1),
            new Vector3(pos.x, pos.y, pos.z + 1),
            pos,
            new Vector3(pos.x, pos.y - 1, pos.z));
        MakeFace(face, meshInfo);
        SetUvs(GetTexturePos(), BlockEffectMap.GetBlockEffect(pos), meshInfo);

    }

    protected void CubeBot(Vector3Int pos, PartMeshInfo meshInfo)
    {
        Face face = new Face(
            new Vector3(pos.x, pos.y - 1, pos.z),
            new Vector3(pos.x + 1, pos.y - 1, pos.z),
            new Vector3(pos.x + 1, pos.y - 1, pos.z + 1),
            new Vector3(pos.x, pos.y - 1, pos.z + 1));
        MakeFace(face, meshInfo);
        SetUvs(GetTexturePos(), BlockEffectMap.GetBlockEffect(pos), meshInfo);
    }

    protected void MakeFace(Face face, PartMeshInfo meshInfo)
    {
        meshInfo.Vertices.Add(face.topLeft);
        meshInfo.Vertices.Add(face.topRight);
        meshInfo.Vertices.Add(face.bottomRight);
        meshInfo.Vertices.Add(face.bottomLeft);

        meshInfo.Triangles.Add(meshInfo.TriangleCount); //1
        meshInfo.Triangles.Add(meshInfo.TriangleCount + 1); //2
        meshInfo.Triangles.Add(meshInfo.TriangleCount + 2); //3
        meshInfo.Triangles.Add(meshInfo.TriangleCount); //1
        meshInfo.Triangles.Add(meshInfo.TriangleCount + 2); //3
        meshInfo.Triangles.Add(meshInfo.TriangleCount + 3); //4
        meshInfo.IncrementTriangleCount(4);
    }
    void SetUvs(Vector2 texturePos, Vector2 effectPos, PartMeshInfo meshInfo)
    {
        meshInfo.BaseUuv.Add(new Vector2(textureUnit * texturePos.x + textureUnit, textureUnit * texturePos.y));
        meshInfo.BaseUuv.Add(new Vector2(textureUnit * texturePos.x + textureUnit, textureUnit * texturePos.y + textureUnit));
        meshInfo.BaseUuv.Add(new Vector2(textureUnit * texturePos.x, textureUnit * texturePos.y + textureUnit));
        meshInfo.BaseUuv.Add(new Vector2(textureUnit * texturePos.x, textureUnit * texturePos.y));

        meshInfo.EffectUuv.Add(new Vector2(textureUnit * effectPos.x + textureUnit, textureUnit * effectPos.y));
        meshInfo.EffectUuv.Add(new Vector2(textureUnit * effectPos.x + textureUnit, textureUnit * effectPos.y + textureUnit));
        meshInfo.EffectUuv.Add(new Vector2(textureUnit * effectPos.x, textureUnit * effectPos.y + textureUnit));
        meshInfo.EffectUuv.Add(new Vector2(textureUnit * effectPos.x, textureUnit * effectPos.y));
    }

    protected void RenderRefMesh(BlockMeshes.MeshInfo refMesh, Vector3 currPos, Vector3 rotation, Vector2 baseTexturePos, Vector2 effectTexturePos, PartMeshInfo meshInfo)
    {
        if (refMesh == null) return;

        foreach (Vector3 v in refMesh.Vertices)
        {
            Vector3 newV = Quaternion.Euler(rotation)*v;
            newV += new Vector3(0.5f, -0.5f, 0.5f) + currPos;

            meshInfo.Vertices.Add(newV);
        }
        foreach (int t in refMesh.Triangles)
        {
            meshInfo.Triangles.Add(t + meshInfo.TriangleCount);
        }

        meshInfo.IncrementTriangleCount(refMesh.Vertices.Length);

        foreach (Vector2 v in refMesh.Uuv)
        {
            Vector2 baseV = v + textureUnit * baseTexturePos;
            meshInfo.BaseUuv.Add(baseV);
            Vector2 effectV = v + textureUnit * effectTexturePos;
            meshInfo.EffectUuv.Add(effectV);
        }
    }


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

