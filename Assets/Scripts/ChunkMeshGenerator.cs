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

    private class Face
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

    public Mesh refMesh;
    public Mesh stepMesh;

    public Mesh stairSide;
    public Mesh stairSide2;

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
                    GenerateMeshInternal();
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

    private void renderStair(Block block, Vector3Int currPos)
    {
        //This code will run for every block in the chunk
        int x = currPos.x;
        int y = currPos.y;
        int z = currPos.z;
        Block neighbourBlock = GetBlock(new Vector3Int(x, y + 1, z));
        bool stepsRendered = false; // Steps are visible from multiple directions, but only need to be rendered once
        if (FaceShouldBeRendered(neighbourBlock))
        {
            //Block above is air
            RenderRefMesh(stepMesh, currPos, block.GetTexturePos(), BlockEffectMap.GetBlockEffect(currPos));
            stepsRendered = true;
        }
        neighbourBlock = GetBlock(new Vector3Int(x, y - 1, z));
        if (FaceShouldBeRendered(neighbourBlock))
        {
            //Block below is air
            CubeBot(currPos, GetBlock(currPos));

        }

        neighbourBlock = GetBlock(new Vector3Int(x + 1, y, z));
        if (FaceShouldBeRendered(neighbourBlock))
        {
            //Block east is air
            CubeEast(currPos, GetBlock(currPos));
        }

        neighbourBlock = GetBlock(new Vector3Int(x, y, z - 1));
        if (FaceShouldBeRendered(neighbourBlock))
        {
            //Block north is air
            RenderRefMesh(stairSide, currPos, block.GetTexturePos(), BlockEffectMap.GetBlockEffect(currPos));
            if (!stepsRendered)
            {
                RenderRefMesh(stepMesh, currPos, block.GetTexturePos(), BlockEffectMap.GetBlockEffect(currPos));
                stepsRendered = true;
            }
        }
        neighbourBlock = GetBlock(new Vector3Int(x, y, z + 1));
        if (FaceShouldBeRendered(neighbourBlock))
        {
            RenderRefMesh(stairSide2, currPos, block.GetTexturePos(), BlockEffectMap.GetBlockEffect(currPos));
            if (!stepsRendered)
            {
                RenderRefMesh(stepMesh, currPos, block.GetTexturePos(), BlockEffectMap.GetBlockEffect(currPos));
                stepsRendered = true;
            }

        }
    }

    private void RenderRefMesh(Mesh refMesh, Vector3 currPos, Vector2 baseTexturePos, Vector2 effectTexturePos)
    {
        List<Vector3> vertices = new List<Vector3>();
        refMesh.GetVertices(vertices);
        foreach (Vector3 v in vertices)
        {
            Vector3 newV = Quaternion.AngleAxis(90, Vector3.up) * v;
            newV = (newV / 2 + new Vector3(0.5f, -0.5f, 0.5f)) + currPos;

            newSpriteVertices.Add(newV);
        }
        int[] triangles = refMesh.GetTriangles(0);
        foreach (int t in triangles)
        {
            newSpriteTriangles.Add(t + faceCount);
        }

        faceCount += vertices.Count;

        List<Vector2> uuvs = new List<Vector2>();
        refMesh.GetUVs(0, uuvs);
        foreach (Vector2 v in uuvs)
        {
            Vector2 baseV = v + unit * baseTexturePos;
            newSpriteUV.Add(baseV);
            Vector2 effectV = v + unit * effectTexturePos;
            effectSpriteUV.Add(effectV);
        }


    }

    private bool FaceShouldBeRendered(Block neighbour)
    {
        return neighbour == null || neighbour.Type == Block.BlockType.airBlock || neighbour.Type == Block.BlockType.stairUpDownBlock;
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
                    if (block != null && block.Type != Block.BlockType.airBlock)
                    {
                        if (block.Type == Block.BlockType.stairUpDownBlock)
                        {
                            renderStair(block, currPos);

                        }
                        else
                        {
                            block = GetBlock(new Vector3Int(x, y + 1, z));
                            if (FaceShouldBeRendered(block))
                            {
                                //Block above is air
                                CubeTop(currPos, GetBlock(currPos));
                            }
                            block = GetBlock(new Vector3Int(x, y - 1, z));
                            if (FaceShouldBeRendered(block))
                            {
                                //Block below is air
                                CubeBot(currPos, GetBlock(currPos));

                            }

                            block = GetBlock(new Vector3Int(x + 1, y, z));
                            if (FaceShouldBeRendered(block))
                            {
                                //Block east is air
                                CubeEast(currPos, GetBlock(currPos));

                            }

                            block = GetBlock(new Vector3Int(x - 1, y, z));
                            if (FaceShouldBeRendered(block))
                            {
                                //Block west is air
                                CubeWest(currPos, GetBlock(currPos));

                            }

                            block = GetBlock(new Vector3Int(x, y, z + 1));
                            if (FaceShouldBeRendered(block))
                            {
                                //Block north is air
                                CubeNorth(currPos, GetBlock(currPos));

                            }
                            block = GetBlock(new Vector3Int(x, y, z - 1));
                            if (FaceShouldBeRendered(block))
                            {
                                //Block south is air
                                CubeSouth(currPos, GetBlock(currPos));
                            }

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
        if (pos.y > maxY) return new AirBlock();
        blockOwner.TryGetBlock(pos, out block);
        return block;
    }

    void CubeTop(Vector3Int pos, Block block)
    {
        Face face = new Face(
            new Vector3(pos.x, pos.y, pos.z + 1),
            new Vector3(pos.x + 1, pos.y, pos.z + 1),
            new Vector3(pos.x + 1, pos.y, pos.z),
            pos);
        MakeFace(face);
        SetUvs(block.GetTexturePos(), BlockEffectMap.GetBlockEffect(pos));

    }

    void CubeNorth(Vector3Int pos, Block block)
    {
        Face face = new Face(
            new Vector3(pos.x + 1, pos.y - 1, pos.z + 1),
            new Vector3(pos.x + 1, pos.y, pos.z + 1),
            new Vector3(pos.x, pos.y, pos.z + 1),
            new Vector3(pos.x, pos.y - 1, pos.z + 1));

        MakeFace(face);
        SetUvs(block.GetTexturePos(), BlockEffectMap.GetBlockEffect(pos));

    }

    void CubeEast(Vector3Int pos, Block block)
    {

        Face face = new Face(
            new Vector3(pos.x + 1, pos.y - 1, pos.z),
            new Vector3(pos.x + 1, pos.y, pos.z),
            new Vector3(pos.x + 1, pos.y, pos.z + 1),
            new Vector3(pos.x + 1, pos.y - 1, pos.z + 1));

        MakeFace(face);
        SetUvs(block.GetTexturePos(), BlockEffectMap.GetBlockEffect(pos));
    }

    void CubeSouth(Vector3Int pos, Block block)
    {
        Face face = new Face(
            new Vector3(pos.x, pos.y - 1, pos.z),
            pos,
            new Vector3(pos.x + 1, pos.y, pos.z),
            new Vector3(pos.x + 1, pos.y - 1, pos.z));
        MakeFace(face);
        SetUvs(block.GetTexturePos(), BlockEffectMap.GetBlockEffect(pos));

    }

    void CubeWest(Vector3Int pos, Block block)
    {
        Face face = new Face(
            new Vector3(pos.x, pos.y - 1, pos.z + 1),
            new Vector3(pos.x, pos.y, pos.z + 1),
            pos,
            new Vector3(pos.x, pos.y - 1, pos.z));
        MakeFace(face);
        SetUvs(block.GetTexturePos(), BlockEffectMap.GetBlockEffect(pos));

    }

    void CubeBot(Vector3Int pos, Block block)
    {
        Face face = new Face(
            new Vector3(pos.x, pos.y - 1, pos.z),
            new Vector3(pos.x + 1, pos.y - 1, pos.z),
            new Vector3(pos.x + 1, pos.y - 1, pos.z + 1),
            new Vector3(pos.x, pos.y - 1, pos.z + 1));
        MakeFace(face);
        SetUvs(block.GetTexturePos(), BlockEffectMap.GetBlockEffect(pos));
    }

    void MakeFace(Face face)
    {
        newSpriteVertices.Add(face.topLeft);
        newSpriteVertices.Add(face.topRight);
        newSpriteVertices.Add(face.bottomRight);
        newSpriteVertices.Add(face.bottomLeft);

        newSpriteTriangles.Add(faceCount); //1
        newSpriteTriangles.Add(faceCount + 1); //2
        newSpriteTriangles.Add(faceCount + 2); //3
        newSpriteTriangles.Add(faceCount); //1
        newSpriteTriangles.Add(faceCount + 2); //3
        newSpriteTriangles.Add(faceCount + 3); //4
        faceCount += 4;
    }
    void SetUvs(Vector2 texturePos, Vector2 effectPos)
    {
        newSpriteUV.Add(new Vector2(unit * texturePos.x + unit, unit * texturePos.y));
        newSpriteUV.Add(new Vector2(unit * texturePos.x + unit, unit * texturePos.y + unit));
        newSpriteUV.Add(new Vector2(unit * texturePos.x, unit * texturePos.y + unit));
        newSpriteUV.Add(new Vector2(unit * texturePos.x, unit * texturePos.y));

        effectSpriteUV.Add(new Vector2(unit * effectPos.x + unit, unit * effectPos.y));
        effectSpriteUV.Add(new Vector2(unit * effectPos.x + unit, unit * effectPos.y + unit));
        effectSpriteUV.Add(new Vector2(unit * effectPos.x, unit * effectPos.y + unit));
        effectSpriteUV.Add(new Vector2(unit * effectPos.x, unit * effectPos.y));
    }

    // void SetEffectUUv(Vector2 effectPos){
    //     effectSpriteUV.Add(new Vector2(unit * effectPos.x + unit, unit * effectPos.y));
    //     effectSpriteUV.Add(new Vector2(unit * effectPos.x + unit, unit * effectPos.y + unit));
    //     effectSpriteUV.Add(new Vector2(unit * effectPos.x, unit * effectPos.y + unit));
    //     effectSpriteUV.Add(new Vector2(unit * effectPos.x, unit * effectPos.y));

    // }
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
