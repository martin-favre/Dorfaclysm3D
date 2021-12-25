using Logging;
using UnityEngine;

public class DorfComponent : MonoBehaviour, ISaveableComponent
{
    const string prefabName = "Prefabs/Dorf";
    LilLogger logger;
    GridActor gridActor;
    DorfController controller;

    private void Start()
    {
        if (logger == null) logger = new LilLogger(gameObject.name);
        gridActor = GetComponent<GridActor>();
        controller = new DorfController(gameObject.name, gridActor, logger);
        logger.Log("I started");
    }

    private void Update()
    {
        controller.Update();
    }

    public static DorfComponent InstantiateDorf(Vector3Int spawnPos)
    {
        GameObject prefabObj = PrefabLoader.GetPrefab<GameObject>(prefabName);
        GameObject obj = Instantiate(prefabObj) as GameObject;
        obj.name = "Dorf_" + obj.GetInstanceID().ToString();
        if (!obj) throw new System.Exception("Could not instantiate prefab " + prefabName);
        DorfComponent dorf = obj.GetComponent<DorfComponent>();
        if (!dorf) throw new System.Exception("No DorfController Component on " + prefabName);
        dorf.gridActor = dorf.GetComponent<GridActor>();
        if (!dorf.gridActor) throw new System.Exception("No GridActor on prefab " + prefabName);
        dorf.gridActor.Move(spawnPos);
        dorf.logger = new LilLogger(obj.name);
        return dorf;
    }

    public IGenericSaveData Save()
    {
        return controller.Save();
    }

    public void Load(IGenericSaveData data)
    {
        controller = new DorfController(data, gridActor, logger);
    }
}