using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ChunkMeshGenerator : MonoBehaviour
{
    
    private List<Vector3> mNewSpriteVertices = new List<Vector3>();
    private List<int> mNewSpriteTriangles = new List<int>();
    private List<Vector2> mNewSpriteUV = new List<Vector2>();

    private List<Vector3> mNewCollVertices = new List<Vector3>();
    private List<int> mNewCollTriangles = new List<int>();

    public const int ChunkSize = 8;
    private int mFaceCount;
    private const float mUnit = 0.25f;

    private MeshCollider mCollider;

    private Mesh mMesh;

    private static float Unit => mUnit;

    public Vector3Int ChunkOrigin { set => mChunkOrigin = value; }

    private Vector3Int mChunkOrigin;

    

    private void Awake()
    {
        mMesh = GetComponent<MeshFilter>().mesh;
        mCollider = GetComponent<MeshCollider>();
    }

    void Start()
    {
        GenerateMesh();
    }

    void Update()
    {
        
    }

    public void GenerateMesh()
    {

        for (int x = mChunkOrigin.x; x < mChunkOrigin.x + ChunkSize; x++)
        {
            for (int y = mChunkOrigin.y; y < mChunkOrigin.y + ChunkSize; y++)
            {
                for (int z = mChunkOrigin.z; z < mChunkOrigin.z + ChunkSize; z++)
                {
                    //This code will run for every block in the chunk
                    Vector3Int currPos = new Vector3Int(x,y,z);
                    Block block = GetBlock(currPos);
                    if (block == null || block.Type != Block.BlockType.airBlock)
                    {
                        block = GetBlock(new Vector3Int(x, y + 1, z));
                        if (block == null || block.Type == Block.BlockType.airBlock)
                        {
                            //Block above is air
                            CubeTop(currPos, GetBlock(currPos));
                        }
                        block = GetBlock(new Vector3Int(x, y - 1, z));
                        if (block == null || block.Type == Block.BlockType.airBlock)
                        {
                            //Block below is air
                            CubeBot(currPos, GetBlock(currPos));

                        }

                        block = GetBlock(new Vector3Int(x + 1, y, z));
                        if (block == null || block.Type == Block.BlockType.airBlock)
                        {
                            //Block east is air
                            CubeEast(currPos, GetBlock(currPos));

                        }

                        block = GetBlock(new Vector3Int(x - 1, y, z));
                        if (block == null || block.Type == Block.BlockType.airBlock)
                        {
                            //Block west is air
                            CubeWest(currPos, GetBlock(currPos));

                        }

                        block = GetBlock(new Vector3Int(x, y, z + 1));
                        if (block == null || block.Type == Block.BlockType.airBlock)
                        {
                            //Block north is air
                            CubeNorth(currPos, GetBlock(currPos));

                        }
                        block = GetBlock(new Vector3Int(x, y, z - 1));
                        if (block == null || block.Type == Block.BlockType.airBlock)
                        {
                            //Block south is air
                            CubeSouth(currPos, GetBlock(currPos));
                        }

                    }

                }
            }
        }

        UpdateMesh();
    }

    Block GetBlock(Vector3Int pos)
    {
        Block block = null;
        GridMap.TryGetBlock(pos, out block);
        return block;
    }

    void CubeTop(Vector3Int pos, Block block)
    {

        mNewSpriteVertices.Add(new Vector3(pos.x, pos.y, pos.z + 1));
        mNewSpriteVertices.Add(new Vector3(pos.x + 1, pos.y, pos.z + 1));
        mNewSpriteVertices.Add(new Vector3(pos.x + 1, pos.y, pos.z));
        mNewSpriteVertices.Add(pos);

        Vector2 texturePos = new Vector2(0, 0);

        //if (GetBlock(x, y, z) == 1)
        //{
        //    texturePos = tStone;
        //}
        //else if (GetBlock(x, y, z) == 2)
        //{
        //    texturePos = tGrassTop;
        //}

        Cube(texturePos);

    }

    void CubeNorth(Vector3Int pos, Block block)
    {

        mNewSpriteVertices.Add(new Vector3(pos.x + 1, pos.y - 1, pos.z + 1));
        mNewSpriteVertices.Add(new Vector3(pos.x + 1, pos.y, pos.z + 1));
        mNewSpriteVertices.Add(new Vector3(pos.x, pos.y, pos.z + 1));
        mNewSpriteVertices.Add(new Vector3(pos.x, pos.y - 1, pos.z + 1));

        Vector2 texturePos = new Vector2(0, 0);

        //if (GetBlock(x, y, z) == 1)
        //{
        //    texturePos = tStone;
        //}
        //else if (GetBlock(x, y, z) == 2)
        //{
        //    texturePos = tGrass;
        //}

        Cube(texturePos);

    }

    void CubeEast(Vector3Int pos, Block block)
    {

        mNewSpriteVertices.Add(new Vector3(pos.x + 1, pos.y - 1, pos.z));
        mNewSpriteVertices.Add(new Vector3(pos.x + 1, pos.y, pos.z));
        mNewSpriteVertices.Add(new Vector3(pos.x + 1, pos.y, pos.z + 1));
        mNewSpriteVertices.Add(new Vector3(pos.x + 1, pos.y - 1, pos.z + 1));

        Vector2 texturePos = new Vector2(0, 0);

        //if (Block(x, y, z) == 1)
        //{
        //    texturePos = tStone;
        //}
        //else if (Block(x, y, z) == 2)
        //{
        //    texturePos = tGrass;
        //}

        Cube(texturePos);

    }

    void CubeSouth(Vector3Int pos, Block block)
    {

        mNewSpriteVertices.Add(new Vector3(pos.x, pos.y - 1, pos.z));
        mNewSpriteVertices.Add(pos);
        mNewSpriteVertices.Add(new Vector3(pos.x + 1, pos.y, pos.z));
        mNewSpriteVertices.Add(new Vector3(pos.x + 1, pos.y - 1, pos.z));

        Vector2 texturePos = new Vector2(0, 0);

        //if (Block(x, y, z) == 1)
        //{
        //    texturePos = tStone;
        //}
        //else if (Block(x, y, z) == 2)
        //{
        //    texturePos = tGrass;
        //}

        Cube(texturePos);

    }

    void CubeWest(Vector3Int pos, Block block)
    {

        mNewSpriteVertices.Add(new Vector3(pos.x, pos.y - 1, pos.z + 1));
        mNewSpriteVertices.Add(new Vector3(pos.x, pos.y, pos.z + 1));
        mNewSpriteVertices.Add(pos);
        mNewSpriteVertices.Add(new Vector3(pos.x, pos.y - 1, pos.z));

        Vector2 texturePos = new Vector2(0, 0);

        //if (Block(x, y, z) == 1)
        //{
        //    texturePos = tStone;
        //}
        //else if (Block(x, y, z) == 2)
        //{
        //    texturePos = tGrass;
        //}

        Cube(texturePos);

    }

    void CubeBot(Vector3Int pos, Block block)
    {

        mNewSpriteVertices.Add(new Vector3(pos.x, pos.y - 1, pos.z));
        mNewSpriteVertices.Add(new Vector3(pos.x + 1, pos.y - 1, pos.z));
        mNewSpriteVertices.Add(new Vector3(pos.x + 1, pos.y - 1, pos.z + 1));
        mNewSpriteVertices.Add(new Vector3(pos.x, pos.y - 1, pos.z + 1));

        Vector2 texturePos = new Vector2(0, 0);

        //if (Block(x, y, z) == 1)
        //{
        //    texturePos = tStone;
        //}
        //else if (Block(x, y, z) == 2)
        //{
        //    texturePos = tGrass;
        //}

        Cube(texturePos);

    }

    void Cube(Vector2 texturePos)
    {

        mNewSpriteTriangles.Add(mFaceCount * 4); //1
        mNewSpriteTriangles.Add(mFaceCount * 4 + 1); //2
        mNewSpriteTriangles.Add(mFaceCount * 4 + 2); //3
        mNewSpriteTriangles.Add(mFaceCount * 4); //1
        mNewSpriteTriangles.Add(mFaceCount * 4 + 2); //3
        mNewSpriteTriangles.Add(mFaceCount * 4 + 3); //4

        mNewSpriteUV.Add(new Vector2(mUnit * texturePos.x + mUnit, mUnit * texturePos.y));
        mNewSpriteUV.Add(new Vector2(mUnit * texturePos.x + mUnit, mUnit * texturePos.y + mUnit));
        mNewSpriteUV.Add(new Vector2(mUnit * texturePos.x, mUnit * texturePos.y + mUnit));
        mNewSpriteUV.Add(new Vector2(mUnit * texturePos.x, mUnit * texturePos.y));

        mFaceCount++; // Add this line
    }
    void UpdateMesh()
    {

        mMesh.Clear();
        mMesh.vertices = mNewSpriteVertices.ToArray();
        mMesh.uv = mNewSpriteUV.ToArray();
        mMesh.triangles = mNewSpriteTriangles.ToArray();
        mMesh.Optimize();
        mMesh.RecalculateNormals();

        mCollider.sharedMesh = null;
        mCollider.sharedMesh = mMesh;

        mNewSpriteVertices.Clear();
        mNewSpriteUV.Clear();
        mNewSpriteTriangles.Clear();
        mFaceCount = 0;

    }

}
