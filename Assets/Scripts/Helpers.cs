using System;
using System.Collections;
using System.Collections.Generic;
using Logging;
using UnityEngine;

public static class Helpers
{

    public static Vector3Int GetRandom(Vector3Int min, Vector3Int max)
    {
        int x = UnityEngine.Random.Range(min.x, max.x);
        int y = UnityEngine.Random.Range(min.y, max.y);
        int z = UnityEngine.Random.Range(min.z, max.z);
        return new Vector3Int(x, y, z);
    }

    public static T GetComponent<T>(GameObject g, LilLogger logger)
    {
        T comp = g.GetComponent<T>();
        if (comp == null)
        {
            logger.Log("Component " + typeof(T).ToString() + "not found on gameobject " + g.name, LogLevel.Warning);
        }
        return comp;
    }

    public static void ForEachPosition(Vector3Int position, Vector3Int size, Action<Vector3Int> action)
    {
        int endX = position.x + size.x;
        int endY = position.y + size.y;
        int endZ = position.z + size.z;
        for (int x = position.x; x < endX; x++)
        {
            for (int y = position.y; y < endY; y++)
            {
                for (int z = position.z; z < endZ; z++)
                {
                    action(new Vector3Int(x, y, z));
                }
            }
        }
    }
}
