using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;

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
    private MeshCollider meshCollider;

    private Mesh mesh;

    public Vector3Int ChunkOrigin { get => chunkOrigin; set => chunkOrigin = value; }
    public int ChunkSize { get => chunkSize; set => chunkSize = value; }
    internal IHasBlocks BlockOwner { set => blockOwner = value; }
    public static int? MaxY { set => maxY = value; get => maxY; }

    private Vector3Int chunkOrigin;

    private bool meshUpdateQueued = false;

    private static int? maxY = null;

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
                    generationTask = WorkStealingTaskScheduler.Run(() => GenerateMeshInternal());
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
            for (int y = chunkOrigin.y; y < chunkOrigin.y + chunkSize && y <= maxY; y++)
            {
                for (int z = chunkOrigin.z; z < chunkOrigin.z + chunkSize; z++)
                {
                    //This code will run for every block in the chunk
                    Vector3Int currPos = new Vector3Int(x, y, z);
                    Block block = GetBlock(currPos);
                    if (block != null && !(block is AirBlock))
                    {
                        PartMeshInfo meshInfo = block.GetMesh(currPos, maxY.Value, blockOwner);
                        meshInfo.Validate();
                        newSpriteVertices.AddRange(meshInfo.Vertices);
                        int[] triangles = meshInfo.Triangles.ToArray();
                        for (int i = 0; i < triangles.Length; i++)
                        {
                            triangles[i] += faceCount;
                        }
                        faceCount += meshInfo.Vertices.Count;
                        newSpriteTriangles.AddRange(triangles);
                        newSpriteUV.AddRange(meshInfo.BaseUuv);
                        effectSpriteUV.AddRange(meshInfo.EffectUuv);
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
        if (pos.y > maxY) return new AirBlock();
        blockOwner.TryGetBlock(pos, out block);
        return block;
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
