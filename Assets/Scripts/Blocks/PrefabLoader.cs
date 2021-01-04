using System.Collections.Generic;
using Logging;
using UnityEngine;

class PrefabLoader
{
    private readonly static LilLogger logger = new LilLogger("PrefabLoader");
    private static GameObject LoadPrefab(string path)
    {
        GameObject obj = Resources.Load(path) as GameObject;
        if (obj == null)
        {
            logger.Log("Could not load resource " + path, LogLevel.Error);
        }
        return obj;
    }

    readonly static Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();

    public static GameObject GetPrefab(string name)
    {
        GameObject gameObject;
        prefabs.TryGetValue(name, out gameObject);
        if (gameObject == null)
        {
            gameObject = LoadPrefab(name);
            prefabs[name] = gameObject;
        }
        return gameObject;
    }
}