using System;
using System.Collections.Generic;
using Items;
using Logging;
using UnityEngine;

class BuildingSite : MonoBehaviour
{
    [System.Serializable]
    class SaveData : GenericSaveData<BlockBuildingSite>
    {
        public BuildingBlueprint blueprint;
    }
    const string prefabPath = "Prefabs/BuildingSite";

    SaveData data = new SaveData();
    InventoryComponent inventory;
    GridActor actor;
    GameObject blueprintPrefab;
    LilLogger logger = new LilLogger("BuildingSites");
    SimpleObserver<RequestPoolUpdateEvent<MoveItemRequest>> requestObserver;
    SimpleObserver<InventoryUpdateEvent> inventoryObserver;



    public static BuildingSite InstantiateNew(BuildingBlueprint blueprint)
    {
        GameObject prefab = PrefabLoader.GetPrefab(prefabPath);
        GameObject gObj = Instantiate(prefab) as GameObject;
        BuildingSite site = gObj.GetComponent<BuildingSite>();
        site.data.blueprint = blueprint;
        return site;
    }

    void Start()
    {
        inventory = Helpers.GetComponent<InventoryComponent>(gameObject, logger);
        if (inventory)
        {
            inventoryObserver = new SimpleObserver<InventoryUpdateEvent>(inventory, OnInventoryUpdated);
        }
        actor = Helpers.GetComponent<GridActor>(gameObject, logger);
        if (actor)
        {
            actor.Move(data.blueprint.Location);
            transform.position = actor.GetPos();
        }
        if (data.blueprint != null)
        {
            blueprintPrefab = PrefabLoader.GetPrefab(data.blueprint.PrefabPath);
            MeshFilter meshFilter = Helpers.GetComponent<MeshFilter>(gameObject, logger);
            if (meshFilter)
            {
                MeshFilter prefabMesh = Helpers.GetComponent<MeshFilter>(blueprintPrefab, logger);
                if (prefabMesh)
                {
                    meshFilter.mesh = prefabMesh.sharedMesh;
                }
            }

            MeshRenderer renderer = Helpers.GetComponent<MeshRenderer>(gameObject, logger);
            if (renderer)
            {
                MeshRenderer prefabRenderer = Helpers.GetComponent<MeshRenderer>(blueprintPrefab, logger);
                if (prefabRenderer)
                {
                    renderer.materials = prefabRenderer.sharedMaterials;
                }
            }
        }
    }

    void OnInventoryUpdated(InventoryUpdateEvent update)
    {

    }

}