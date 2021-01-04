using System.Collections.Generic;
using Logging;
using UnityEngine;

public class BlockMeshes
{
    public class MeshInfo
    {
        readonly Vector3[] vertices;
        readonly int[] triangles;
        readonly List<Vector2> uuv;

        public MeshInfo(Mesh mesh)
        {
            vertices = mesh.vertices;
            for(int i = 0; i < vertices.Length; i++) {
                // all meshes seems to be twice as big as expected, no matter the scaling in blender.
                vertices[i] /= 2;
            }
            triangles = mesh.triangles;
            uuv = new List<Vector2>();
            mesh.GetUVs(0, uuv);
        }


        public List<Vector2> Uuv => uuv;

        public int[] Triangles => triangles;

        public Vector3[] Vertices => vertices;
    }
    private static MeshInfo fullStair;

    public static MeshInfo FullStair { get => fullStair; }

    public static LilLogger logger = new LilLogger("Blocks");

    private static MeshInfo LoadMesh(string path)
    {
    
        return new MeshInfo(ModelLoader.GetMesh(path).sharedMesh);
    }

    // Must be called from main thread
    // Can't use Mesh otherwise
    public static void LoadMeshes()
    {
        fullStair = LoadMesh("Models/fullstair");
    }

}