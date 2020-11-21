using System.Collections.Generic;
using UnityEngine;

public class PartMeshInfo
{
    private readonly List<Vector3> vertices = new List<Vector3>();
    private readonly List<int> triangles = new List<int>();
    private readonly List<Vector2> baseUuv = new List<Vector2>();
    private readonly List<Vector2> effectUuv = new List<Vector2>();
    private int triangleCount;
    public List<Vector3> Vertices => vertices;

    public List<int> Triangles => triangles;

    public List<Vector2> BaseUuv => baseUuv;

    public List<Vector2> EffectUuv => effectUuv;

    public int TriangleCount { get => triangleCount; }

    public void IncrementTriangleCount(int amount)
    {
        triangleCount += amount;
    }

    public void Validate()
    {
        if(vertices.Count != baseUuv.Count) throw new System.Exception("vertices baseUuv size difference");
        if(effectUuv.Count != baseUuv.Count) throw new System.Exception("effectUuv baseUuv size difference");
        if(vertices.Count != triangleCount) throw new System.Exception("vertices triangleCount size difference");
    }
}
