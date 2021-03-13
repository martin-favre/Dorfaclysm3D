using UnityEngine;

public class MapGenerationProgress : MonoBehaviour
{
    SimpleValueDisplayer.ValueHook hook;
    const string prefabName = "Prefabs/MapGenerationProgress";

    IMapGenerator generator;
    public static MapGenerationProgress InstantiateNew(IMapGenerator newGenerator)
    {
        GameObject prefabObj = PrefabLoader.GetPrefab<GameObject>(prefabName);
        GameObject obj = Instantiate(prefabObj) as GameObject;
        if (!obj) throw new System.Exception("Could not instantiate prefab " + prefabName);
        MapGenerationProgress disp = obj.GetComponent<MapGenerationProgress>();
        if (!disp) throw new System.Exception("No MapGenerationProgress Component on " + prefabName);
        disp.generator = newGenerator;
        return disp;
    }


    void Start()
    {
        hook = SimpleValueDisplayer.Instance.RegisterValue();
    }
    void Update()
    {
        if (generator.IsComplete())
        {
            GameObject.Destroy(gameObject);
            hook.Dispose();
        }
        else
        {
            hook.UpdateValue("Mapgeneration: " + generator.GetProgress()*100 + "%");
        }
    }

}