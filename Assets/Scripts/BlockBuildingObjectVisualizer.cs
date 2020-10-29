using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockBuildingObjectVisualizer : MonoBehaviour
{
    private class MyBlock : IHasBlocks
    {
        readonly Vector3Int origin;
        public MyBlock(Vector3Int origin)
        {
            this.origin = origin;
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
                block = new RockBlock();
                return true;
            } else {
                block = new AirBlock();
                return false;
            }
        }
    }
    BlockBuildingSite site;
    ChunkMeshGenerator meshGenerator;
    Block expectedBlock;

    MyBlock blockOwner;

    void Start()
    {
        site = GetComponent<BlockBuildingSite>();
        if (site == null) return;
        meshGenerator = GetComponent<ChunkMeshGenerator>();
        if (meshGenerator == null) return;
        expectedBlock = site.GetBlock();
        Vector3Int origin = site.GetComponent<GridActor>().GetPos();
        
        blockOwner = new MyBlock(Vector3Int.zero);
        meshGenerator.ChunkOrigin = Vector3Int.zero;
        meshGenerator.ChunkSize = 1;
        meshGenerator.BlockOwner = blockOwner;
        meshGenerator.GenerateMesh();
    }

    // Update is called once per frame
    void Update()
    {
        if (site == null || meshGenerator == null) return;



    }
}
