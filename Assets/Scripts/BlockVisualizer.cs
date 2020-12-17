using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockVisualizer : MonoBehaviour
{
    private class MyBlock : IHasBlocks
    {
        readonly Vector3Int origin;
        readonly Block expectedBlock;

        public MyBlock(Vector3Int origin, Block expectedBlock)
        {
            this.origin = origin;
            this.expectedBlock = expectedBlock;
        }
        public Block GetBlock(Vector3Int pos)
        {
            Block block;
            TryGetBlock(pos, out block);
            return block;
        }

        public Vector3Int GetSize()
        {
            return new Vector3Int(1, 1, 1);
        }

        public void SetBlock(Vector3Int pos, Block block)
        {
            // not used
        }

        public void SetSize(Vector3Int size)
        {
            throw new System.NotImplementedException();
        }

        public bool TryGetBlock(Vector3Int pos, out Block block)
        {
            if (pos == origin)
            {
                block = expectedBlock;
                return true;
            }
            else
            {
                block = new AirBlock();
                return false;
            }
        }
    }

    Mesh mesh;
    ChunkMeshGenerator meshGenerator;

    public void RenderBlock(Block originBlock)
    {
        if (meshGenerator == null)
        {
            meshGenerator = GetComponent<ChunkMeshGenerator>();
        }
        if (meshGenerator == null) return;
        MyBlock blockOwner = new MyBlock(Vector3Int.zero, originBlock);
        meshGenerator.ChunkOrigin = Vector3Int.zero;
        meshGenerator.ChunkSize = 1;
        meshGenerator.BlockOwner = blockOwner;
        meshGenerator.GenerateMesh();
    }
    PartMeshInfo GenerateMesh(Vector3Int thisBlockPos, int maxY, Vector2 texturePos)
    {
        PartMeshInfo meshInfo = new PartMeshInfo();
        if (thisBlockPos.y <= maxY)
        {
            Block.CubeTop(thisBlockPos, meshInfo, texturePos, BlockEffects.NoEffect);
            Block.CubeBot(thisBlockPos, meshInfo, texturePos, BlockEffects.NoEffect);
            Block.CubeEast(thisBlockPos, meshInfo, texturePos, BlockEffects.NoEffect);
            Block.CubeWest(thisBlockPos, meshInfo, texturePos, BlockEffects.NoEffect);
            Block.CubeNorth(thisBlockPos, meshInfo, texturePos, BlockEffects.NoEffect);
            Block.CubeSouth(thisBlockPos, meshInfo, texturePos, BlockEffects.NoEffect);
        }
        return meshInfo;
    }

    public void RenderBlock(Vector2 texturePos)
    {
        if(mesh == null) mesh = GetComponent<MeshFilter>().mesh;
        
        PartMeshInfo meshinfo = GenerateMesh(Vector3Int.zero, ChunkMeshGenerator.MaxY.Value, texturePos);

        mesh.Clear();
        mesh.vertices = meshinfo.Vertices.ToArray();
        mesh.uv = meshinfo.BaseUuv.ToArray();
        mesh.uv2 = meshinfo.EffectUuv.ToArray();
        mesh.triangles = meshinfo.Triangles.ToArray();
        mesh.Optimize();
        mesh.RecalculateNormals();
    }
}
