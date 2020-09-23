using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ChunkMeshGenerator : MonoBehaviour
{

    private List<Vector3> newSpriteVertices = new List<Vector3>();
    private List<int> newSpriteTriangles = new List<int>();
    private List<Vector2> newSpriteUV = new List<Vector2>();

    private List<Vector3> newCollVertices = new List<Vector3>();
    private List<int> newCollTriangles = new List<int>();

    public const int chunkSize = 8;
    private int faceCount;
    private const float unit = 0.5f;

    private MeshCollider meshCollider;

    private Mesh mesh;

    public Vector3Int ChunkOrigin { set => chunkOrigin = value; }

    private Vector3Int chunkOrigin;

    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        meshCollider = GetComponent<MeshCollider>();
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

        for (int x = chunkOrigin.x; x < chunkOrigin.x + chunkSize; x++)
        {
            for (int y = chunkOrigin.y; y < chunkOrigin.y + chunkSize; y++)
            {
                for (int z = chunkOrigin.z; z < chunkOrigin.z + chunkSize; z++)
                {
                    //This code will run for every block in the chunk
                    Vector3Int currPos = new Vector3Int(x, y, z);
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
        newSpriteVertices.Add(new Vector3(pos.x, pos.y, pos.z + 1));
        newSpriteVertices.Add(new Vector3(pos.x + 1, pos.y, pos.z + 1));
        newSpriteVertices.Add(new Vector3(pos.x + 1, pos.y, pos.z));
        newSpriteVertices.Add(pos);
        Cube(block.GetTexturePos());
    }

    void CubeNorth(Vector3Int pos, Block block)
    {
        newSpriteVertices.Add(new Vector3(pos.x + 1, pos.y - 1, pos.z + 1));
        newSpriteVertices.Add(new Vector3(pos.x + 1, pos.y, pos.z + 1));
        newSpriteVertices.Add(new Vector3(pos.x, pos.y, pos.z + 1));
        newSpriteVertices.Add(new Vector3(pos.x, pos.y - 1, pos.z + 1));
        Cube(block.GetTexturePos());
    }

    void CubeEast(Vector3Int pos, Block block)
    {

        newSpriteVertices.Add(new Vector3(pos.x + 1, pos.y - 1, pos.z));
        newSpriteVertices.Add(new Vector3(pos.x + 1, pos.y, pos.z));
        newSpriteVertices.Add(new Vector3(pos.x + 1, pos.y, pos.z + 1));
        newSpriteVertices.Add(new Vector3(pos.x + 1, pos.y - 1, pos.z + 1));

        Cube(block.GetTexturePos());

    }

    void CubeSouth(Vector3Int pos, Block block)
    {

        newSpriteVertices.Add(new Vector3(pos.x, pos.y - 1, pos.z));
        newSpriteVertices.Add(pos);
        newSpriteVertices.Add(new Vector3(pos.x + 1, pos.y, pos.z));
        newSpriteVertices.Add(new Vector3(pos.x + 1, pos.y - 1, pos.z));
        Cube(block.GetTexturePos());


    }

    void CubeWest(Vector3Int pos, Block block)
    {

        newSpriteVertices.Add(new Vector3(pos.x, pos.y - 1, pos.z + 1));
        newSpriteVertices.Add(new Vector3(pos.x, pos.y, pos.z + 1));
        newSpriteVertices.Add(pos);
        newSpriteVertices.Add(new Vector3(pos.x, pos.y - 1, pos.z));
        Cube(block.GetTexturePos());


    }

    void CubeBot(Vector3Int pos, Block block)
    {

        newSpriteVertices.Add(new Vector3(pos.x, pos.y - 1, pos.z));
        newSpriteVertices.Add(new Vector3(pos.x + 1, pos.y - 1, pos.z));
        newSpriteVertices.Add(new Vector3(pos.x + 1, pos.y - 1, pos.z + 1));
        newSpriteVertices.Add(new Vector3(pos.x, pos.y - 1, pos.z + 1));
        Cube(block.GetTexturePos());


    }

    void Cube(Vector2 texturePos)
    {

        newSpriteTriangles.Add(faceCount * 4); //1
        newSpriteTriangles.Add(faceCount * 4 + 1); //2
        newSpriteTriangles.Add(faceCount * 4 + 2); //3
        newSpriteTriangles.Add(faceCount * 4); //1
        newSpriteTriangles.Add(faceCount * 4 + 2); //3
        newSpriteTriangles.Add(faceCount * 4 + 3); //4

        newSpriteUV.Add(new Vector2(unit * texturePos.x + unit, unit * texturePos.y));
        newSpriteUV.Add(new Vector2(unit * texturePos.x + unit, unit * texturePos.y + unit));
        newSpriteUV.Add(new Vector2(unit * texturePos.x, unit * texturePos.y + unit));
        newSpriteUV.Add(new Vector2(unit * texturePos.x, unit * texturePos.y));

        faceCount++; // Add this line
    }
    void UpdateMesh()
    {

        mesh.Clear();
        mesh.vertices = newSpriteVertices.ToArray();
        mesh.uv = newSpriteUV.ToArray();
        mesh.triangles = newSpriteTriangles.ToArray();
        mesh.Optimize();
        mesh.RecalculateNormals();

        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;

        newSpriteVertices.Clear();
        newSpriteUV.Clear();
        newSpriteTriangles.Clear();
        faceCount = 0;

    }

}
