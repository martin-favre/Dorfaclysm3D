
using System.Collections.Generic;
using Items;
using Logging;
using UnityEngine;

[System.Serializable]
public abstract class Block : System.ICloneable, System.IEquatable<Block>
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
    private const float textureUnit = 0.25f;
    private Vector3 rotation = Vector3.zero;

    public Vector3 Rotation { get => rotation; set => rotation = value; }

    public Block()
    {
    }
    public Block(Vector3 rotation)
    {
        this.rotation = rotation;
    }


    // Position in the spritesheet
    public abstract Vector2 GetTexturePos();

    // Can you walk through this block?
    public abstract bool SupportsWalkingThrough();

    // Can you ascend from this block?
    public abstract bool SupportsClimbing();

    //Can you walk on top of this block?
    public abstract bool SupportsWalkingOnTop();
    public abstract string GetName();
    public abstract bool IsVisible();

    public abstract bool NeighbourShouldBeRendered();

    public abstract Item GetItem();

    public abstract object Clone();

    public abstract PartMeshInfo GetMesh(Vector3Int thisBlockPos, int maxY, IHasBlocks blockOwner);
    protected bool FaceShouldBeRendered(Block neighbour)
    {
        return neighbour == null || neighbour.NeighbourShouldBeRendered();
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
        Vector2 texturePos = GetTexturePos();
        Vector2 effectPos = BlockEffectMap.Instance.GetBlockEffect(thisBlockPos);
        if (FaceShouldBeRendered(neighbourBlock))
        {
            //Block above is air
            CubeTop(thisBlockPos, meshInfo, texturePos, effectPos);
        }
        neighbourBlock = GetBlock(new Vector3Int(x, y - 1, z), maxY, blockOwner);
        if (FaceShouldBeRendered(neighbourBlock))
        {
            //Block below is air
            CubeBot(thisBlockPos, meshInfo, texturePos, effectPos);

        }

        neighbourBlock = GetBlock(new Vector3Int(x + 1, y, z), maxY, blockOwner);
        if (FaceShouldBeRendered(neighbourBlock))
        {
            //Block east is air
            CubeEast(thisBlockPos, meshInfo, texturePos, effectPos);

        }

        neighbourBlock = GetBlock(new Vector3Int(x - 1, y, z), maxY, blockOwner);
        if (FaceShouldBeRendered(neighbourBlock))
        {
            //Block west is air
            CubeWest(thisBlockPos, meshInfo, texturePos, effectPos);

        }

        neighbourBlock = GetBlock(new Vector3Int(x, y, z + 1), maxY, blockOwner);
        if (FaceShouldBeRendered(neighbourBlock))
        {
            //Block north is air
            CubeNorth(thisBlockPos, meshInfo, texturePos, effectPos);

        }
        neighbourBlock = GetBlock(new Vector3Int(x, y, z - 1), maxY, blockOwner);
        if (FaceShouldBeRendered(neighbourBlock))
        {
            //Block south is air
            CubeSouth(thisBlockPos, meshInfo, texturePos, effectPos);
        }

        return meshInfo;

    }
    public static void CubeTop(Vector3Int pos, PartMeshInfo meshInfo, Vector2 texturePos, Vector2 effectPos)
    {
        Face face = new Face(
            new Vector3(pos.x, pos.y, pos.z + 1),
            new Vector3(pos.x + 1, pos.y, pos.z + 1),
            new Vector3(pos.x + 1, pos.y, pos.z),
            pos);
        MakeFace(face, meshInfo);
        SetUvs(texturePos, effectPos, meshInfo);

    }

    public static void CubeNorth(Vector3Int pos, PartMeshInfo meshInfo, Vector2 texturePos, Vector2 effectPos)
    {
        Face face = new Face(
            new Vector3(pos.x + 1, pos.y - 1, pos.z + 1),
            new Vector3(pos.x + 1, pos.y, pos.z + 1),
            new Vector3(pos.x, pos.y, pos.z + 1),
            new Vector3(pos.x, pos.y - 1, pos.z + 1));

        MakeFace(face, meshInfo);
        SetUvs(texturePos, effectPos, meshInfo);

    }

    public static void CubeEast(Vector3Int pos, PartMeshInfo meshInfo, Vector2 texturePos, Vector2 effectPos)
    {

        Face face = new Face(
            new Vector3(pos.x + 1, pos.y - 1, pos.z),
            new Vector3(pos.x + 1, pos.y, pos.z),
            new Vector3(pos.x + 1, pos.y, pos.z + 1),
            new Vector3(pos.x + 1, pos.y - 1, pos.z + 1));

        MakeFace(face, meshInfo);
        SetUvs(texturePos, effectPos, meshInfo);
    }

    public static void CubeSouth(Vector3Int pos, PartMeshInfo meshInfo, Vector2 texturePos, Vector2 effectPos)
    {
        Face face = new Face(
            new Vector3(pos.x, pos.y - 1, pos.z),
            pos,
            new Vector3(pos.x + 1, pos.y, pos.z),
            new Vector3(pos.x + 1, pos.y - 1, pos.z));
        MakeFace(face, meshInfo);
        SetUvs(texturePos, effectPos, meshInfo);

    }

    public static void CubeWest(Vector3Int pos, PartMeshInfo meshInfo, Vector2 texturePos, Vector2 effectPos)
    {
        Face face = new Face(
            new Vector3(pos.x, pos.y - 1, pos.z + 1),
            new Vector3(pos.x, pos.y, pos.z + 1),
            pos,
            new Vector3(pos.x, pos.y - 1, pos.z));
        MakeFace(face, meshInfo);
        SetUvs(texturePos, effectPos, meshInfo);

    }

    public static void CubeBot(Vector3Int pos, PartMeshInfo meshInfo, Vector2 texturePos, Vector2 effectPos)
    {
        Face face = new Face(
            new Vector3(pos.x, pos.y - 1, pos.z),
            new Vector3(pos.x + 1, pos.y - 1, pos.z),
            new Vector3(pos.x + 1, pos.y - 1, pos.z + 1),
            new Vector3(pos.x, pos.y - 1, pos.z + 1));
        MakeFace(face, meshInfo);
        SetUvs(texturePos, effectPos, meshInfo);
    }

    protected static void MakeFace(Face face, PartMeshInfo meshInfo)
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
    static void SetUvs(Vector2 texturePos, Vector2 effectPos, PartMeshInfo meshInfo)
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
        return other.rotation == rotation;
    }

    public override int GetHashCode()
    {
        int hashCode = -2012710218;
        hashCode = hashCode * -1521134295 + rotation.GetHashCode();
        return hashCode;
    }
}

