using System.Collections.Generic;
using Logging;
using UnityEngine;

public static class ModelLoader
{
    private static LilLogger logger = new LilLogger("Models"); 
    private static MeshFilter LoadMesh(string path)
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
        return filter;
    }

    private static readonly Dictionary<string, MeshFilter> meshes = new Dictionary<string, MeshFilter>();

    public static MeshFilter GetMesh(string path) 
    {
        MeshFilter mesh;
        meshes.TryGetValue(path, out mesh);
        if(mesh == null) {
            mesh = LoadMesh(path);
            meshes[path] = mesh;
        }
        return mesh;
    }
}