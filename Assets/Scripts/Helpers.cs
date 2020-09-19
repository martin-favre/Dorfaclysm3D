using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helpers
{

    public static Vector3Int GetRandom(Vector3Int min, Vector3Int max)
    {
        int x = Random.Range(min.x, max.x);
        int y = Random.Range(min.y, max.y);
        int z = Random.Range(min.z, max.z);
        return new Vector3Int(x,y,z);
    }
}
