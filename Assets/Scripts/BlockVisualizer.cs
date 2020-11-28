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
}
