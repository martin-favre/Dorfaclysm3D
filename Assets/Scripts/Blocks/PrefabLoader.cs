using System.Collections.Generic;
using Logging;
using UnityEngine;

class PrefabLoader
{
    private readonly static LilLogger logger = new LilLogger("PrefabLoader");
    private static Object LoadPrefab(string path)
    {
        Object obj = Resources.Load(path) as Object;
        if (obj == null)
        {
            logger.Log("Could not load resource " + path, LogLevel.Error);
        }
        return obj;
    }

    readonly static Dictionary<string, Object> prefabs = new Dictionary<string, Object>();

    public static T GetPrefab<T>(string name) where T : class
    {
        Object gameObject;
        prefabs.TryGetValue(name, out gameObject);
        if (gameObject == null)
        {
            gameObject = LoadPrefab(name);
            prefabs[name] = gameObject;
        }
        return gameObject as T;
    }
}