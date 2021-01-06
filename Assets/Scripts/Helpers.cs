using System.Collections;
using System.Collections.Generic;
using Logging;
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

    public static T GetComponent<T>(GameObject g, LilLogger logger) {
        T comp = g.GetComponent<T>();
        if(comp == null) {
            logger.Log("Component " + typeof(T).ToString() + "not found on gameobject " + g.name, LogLevel.Warning);
        }
        return comp;
    }
}
