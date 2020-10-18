using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testShaderScript : MonoBehaviour
{
    Mesh mesh;
        private List<Vector3> newSpriteVertices = new List<Vector3>();
    private List<int> newSpriteTriangles = new List<int>();
    private List<Vector2> newSpriteUV = new List<Vector2>();

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
