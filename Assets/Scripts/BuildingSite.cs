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
        public List<Guid> spawnedRequests;
    }
    const string prefabPath = "Prefabs/BuildingSite";

    SaveData data = new SaveData();
    InventoryComponent inventory;
    GridActor actor;
    GameObject blueprintPrefab;
    LilLogger logger = new LilLogger("BuildingSites");
    SimpleObserver<InventoryUpdateEvent> inventoryObserver;

    List<SimpleKeyObserver<RequestPoolUpdateEvent<MoveItemRequest>, Guid>> requestObservers = new List<SimpleKeyObserver<RequestPoolUpdateEvent<MoveItemRequest>, Guid>>();

    public static BuildingSite InstantiateNew(BuildingBlueprint blueprint)
    {
        GameObject prefab = PrefabLoader.GetPrefab(prefabPath);
        GameObject gObj = Instantiate(prefab) as GameObject;
        BuildingSite site = gObj.GetComponent<BuildingSite>();
        site.data.blueprint = blueprint;
        return site;
    }

    void SetupMesh()
    {
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
                renderer.material.mainTexture = prefabRenderer.sharedMaterial.mainTexture;
            }
        }
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
            transform.position = actor.Position;
        }
        if (data.blueprint != null)
        {
            blueprintPrefab = PrefabLoader.GetPrefab(data.blueprint.PrefabPath);
            SetupMesh();

            SetupGridActor();

            SetupRequests();
        }

    }

    private void SetupGridActor()
    {
        GridActor prefabActor = blueprintPrefab.GetComponent<GridActor>();
        if (prefabActor && actor)
        {
            actor.SetSize(prefabActor.Size);
        }
    }

    private void SetupRequests()
    {
        if (data.spawnedRequests == null)
        {
            data.spawnedRequests = new List<Guid>();
            foreach (var ingredient in data.blueprint.RequiredItems)
            {
                for (int ii = 0; ii < ingredient.Item2; ii++)
                {
                    MoveItemRequest newReq = new MoveItemRequest(ingredient.Item1, actor.Position, actor.Guid);
                    data.spawnedRequests.Add(newReq.Guid);
                    MoveItemRequestPool.Instance.PostRequest(
                        newReq
                    );
                }
            }
        }
        foreach (var requestGuid in data.spawnedRequests)
        {
            requestObservers.Add(new SimpleKeyObserver<RequestPoolUpdateEvent<MoveItemRequest>, Guid>(MoveItemRequestPool.Instance, requestGuid,
                                (reqUpdate) =>
                                {
                                    if (reqUpdate.Type == RequestPoolUpdateEvent<MoveItemRequest>.EventType.Cancelled)
                                    {
                                        GameObject.Destroy(gameObject);
                                    }
                                    if (reqUpdate.Type == RequestPoolUpdateEvent<MoveItemRequest>.EventType.Finished)
                                    {
                                        data.spawnedRequests.Remove(reqUpdate.Request.Guid);
                                    }
                                }
                            ));
        }
    }

    private void OnDestroy()
    {
        Guid[] guidsToRemove = data.spawnedRequests.ToArray();
        foreach (var requestGuid in guidsToRemove)
        {
            MoveItemRequestPool.Instance.CancelRequest(requestGuid);
        }
    }

    private bool AreAllRequiredItemsAvailable()
    {
        foreach (var ingredient in data.blueprint.RequiredItems)
        {
            if (ingredient.Item2 != inventory.GetItemCount(ingredient.Item1))
            {
                return false;
            }
        }
        return true;
    }

    void OnInventoryUpdated(InventoryUpdateEvent update)
    {
        if (update.Type != InventoryUpdateEvent.UpdateType.Added) return;
        if (inventory == null) return;
        if (actor == null) return;

        if (AreAllRequiredItemsAvailable())
        {
            GameObject newObj = Instantiate(blueprintPrefab) as GameObject;
            GridActor newActor = newObj.GetComponent<GridActor>();
            if (actor) newActor.Move(actor.Position);

            GameObject.Destroy(gameObject);
        }
    }

}