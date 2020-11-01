using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class ChunkMeshGenerator : MonoBehaviour
{

    private enum State
    {
        Idle,
        UpdateReceived,
        GeneratingMesh,
        MeshResultReceived
    }

    private List<Vector3> newSpriteVertices = new List<Vector3>();
    private List<int> newSpriteTriangles = new List<int>();
    private List<Vector2> newSpriteUV = new List<Vector2>();
    private List<Vector2> effectSpriteUV = new List<Vector2>();

    private int chunkSize = 8;
    private int faceCount;
    private const float unit = 0.5f;

    private MeshCollider meshCollider;

    private Mesh mesh;

    public Vector3Int ChunkOrigin { get => chunkOrigin; set => chunkOrigin = value; }
    public int ChunkSize { get => chunkSize; set => chunkSize = value; }
    internal IHasBlocks BlockOwner { set => blockOwner = value; }
    public int? MaxY { set => maxY = value; }

    private Vector3Int chunkOrigin;

    private bool meshUpdateQueued = false;

    private int? maxY = null;

    private State state = State.Idle;

    private Task generationTask;

    private IHasBlocks blockOwner;


    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        meshCollider = GetComponent<MeshCollider>();
    }

    void Start()
    {
    }

    void Update()
    {
        switch (state)
        {
            case State.Idle:
                if (meshUpdateQueued)
                {
                    generationTask = Task.Run(() => GenerateMeshInternal());
                    state = State.GeneratingMesh;
                    meshUpdateQueued = false;
                }
                break;
            case State.GeneratingMesh:
                if (generationTask.IsCompleted)
                {
                    state = State.MeshResultReceived;
                }
                break;
            case State.MeshResultReceived:
                UpdateMesh();
                state = State.Idle;
                break;
        }
    }

    private void GenerateMeshInternal()
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
        state = State.MeshResultReceived;
    }
    public void GenerateMesh()
    {
        meshUpdateQueued = true;
    }



    Block GetBlock(Vector3Int pos)
    {
        Block block = null;
        if(pos.y > maxY) return new AirBlock();
        blockOwner.TryGetBlock(pos, out block);
        return block;
    }

    void CubeTop(Vector3Int pos, Block block)
    {
        newSpriteVertices.Add(new Vector3(pos.x, pos.y, pos.z + 1));
        newSpriteVertices.Add(new Vector3(pos.x + 1, pos.y, pos.z + 1));
        newSpriteVertices.Add(new Vector3(pos.x + 1, pos.y, pos.z));
        newSpriteVertices.Add(pos);
        Cube(block.GetTexturePos(), BlockEffectMap.GetBlockEffect(pos));
    }

    void CubeNorth(Vector3Int pos, Block block)
    {
        newSpriteVertices.Add(new Vector3(pos.x + 1, pos.y - 1, pos.z + 1));
        newSpriteVertices.Add(new Vector3(pos.x + 1, pos.y, pos.z + 1));
        newSpriteVertices.Add(new Vector3(pos.x, pos.y, pos.z + 1));
        newSpriteVertices.Add(new Vector3(pos.x, pos.y - 1, pos.z + 1));
        Cube(block.GetTexturePos(), BlockEffectMap.GetBlockEffect(pos));
    }

    void CubeEast(Vector3Int pos, Block block)
    {

        newSpriteVertices.Add(new Vector3(pos.x + 1, pos.y - 1, pos.z));
        newSpriteVertices.Add(new Vector3(pos.x + 1, pos.y, pos.z));
        newSpriteVertices.Add(new Vector3(pos.x + 1, pos.y, pos.z + 1));
        newSpriteVertices.Add(new Vector3(pos.x + 1, pos.y - 1, pos.z + 1));
        Cube(block.GetTexturePos(), BlockEffectMap.GetBlockEffect(pos));

    }

    void CubeSouth(Vector3Int pos, Block block)
    {

        newSpriteVertices.Add(new Vector3(pos.x, pos.y - 1, pos.z));
        newSpriteVertices.Add(pos);
        newSpriteVertices.Add(new Vector3(pos.x + 1, pos.y, pos.z));
        newSpriteVertices.Add(new Vector3(pos.x + 1, pos.y - 1, pos.z));
        Cube(block.GetTexturePos(), BlockEffectMap.GetBlockEffect(pos));
    }

    void CubeWest(Vector3Int pos, Block block)
    {

        newSpriteVertices.Add(new Vector3(pos.x, pos.y - 1, pos.z + 1));
        newSpriteVertices.Add(new Vector3(pos.x, pos.y, pos.z + 1));
        newSpriteVertices.Add(pos);
        newSpriteVertices.Add(new Vector3(pos.x, pos.y - 1, pos.z));
        Cube(block.GetTexturePos(), BlockEffectMap.GetBlockEffect(pos));
    }

    void CubeBot(Vector3Int pos, Block block)
    {

        newSpriteVertices.Add(new Vector3(pos.x, pos.y - 1, pos.z));
        newSpriteVertices.Add(new Vector3(pos.x + 1, pos.y - 1, pos.z));
        newSpriteVertices.Add(new Vector3(pos.x + 1, pos.y - 1, pos.z + 1));
        newSpriteVertices.Add(new Vector3(pos.x, pos.y - 1, pos.z + 1));
        Cube(block.GetTexturePos(), BlockEffectMap.GetBlockEffect(pos));

    }

    void Cube(Vector2 texturePos, Vector2 effectPos)
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

        effectSpriteUV.Add(new Vector2(unit * effectPos.x + unit, unit * effectPos.y));
        effectSpriteUV.Add(new Vector2(unit * effectPos.x + unit, unit * effectPos.y + unit));
        effectSpriteUV.Add(new Vector2(unit * effectPos.x, unit * effectPos.y + unit));
        effectSpriteUV.Add(new Vector2(unit * effectPos.x, unit * effectPos.y));


        faceCount++; // Add this line
    }
    private void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = newSpriteVertices.ToArray();
        mesh.uv = newSpriteUV.ToArray();
        mesh.uv2 = effectSpriteUV.ToArray();
        mesh.triangles = newSpriteTriangles.ToArray();
        mesh.Optimize();
        mesh.RecalculateNormals();

        if (meshCollider != null)
        {
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
        }

        newSpriteVertices.Clear();
        newSpriteUV.Clear();
        effectSpriteUV.Clear();
        newSpriteTriangles.Clear();
        faceCount = 0;
    }

}
