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
            triangles = mesh.triangles;
            uuv = new List<Vector2>();
            mesh.GetUVs(0, uuv);
        }


        public List<Vector2> Uuv => uuv;

        public int[] Triangles => triangles;

        public Vector3[] Vertices => vertices;
    }
    private static MeshInfo stairSteps;
    private static MeshInfo stairSide;
    private static MeshInfo stairSide2;

    public static MeshInfo StairSteps { get => stairSteps; }
    public static MeshInfo StairSide { get => stairSide; }
    public static MeshInfo StairSide2 { get => stairSide2; }

    public static LilLogger logger = new LilLogger("Blocks");

    private static MeshInfo LoadMesh(string path)
    {
        GameObject gObj = Resources.Load(path) as GameObject;
        if (gObj == null)
        {
            logger.Log("Could not load resource " + path, LogLevel.Error);
            return null;
        }
        MeshFilter filter = gObj.GetComponent<MeshFilter>();
        if (filter == null)
        {
            logger.Log("Resource " + path + " did not contain MeshFilter", LogLevel.Error);
            return null;
        }

        return new MeshInfo(filter.sharedMesh);
    }

    // Must be called from main thread
    // Can't use Mesh otherwise
    public static void LoadMeshes()
    {
        stairSteps = LoadMesh("Models/steps");
        stairSide = LoadMesh("Models/stairside");
        stairSide2 = LoadMesh("Models/stairside2");
    }

}