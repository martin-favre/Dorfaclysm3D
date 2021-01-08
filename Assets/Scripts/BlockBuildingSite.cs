using UnityEngine;
using Items;
using System;

public class BlockBuildingSite : MonoBehaviour, ISaveableComponent
{
    [System.Serializable]
    class SaveData : GenericSaveData<BlockBuildingSite>
    {
        public bool hasSpawnedRequest = false;
        public bool requestFinished = false;
        public Guid requestGuid;
        public Block targetBlock;
        public Type itemRequired;
    }

    GridActor actor;

    InventoryComponent inventory;
    SaveData data = new SaveData();
    const string prefabName = "Prefabs/BlockBuildingObject";

    SimpleObserver<InventoryUpdateEvent> inventoryObserver;

    SimpleObserver<RequestPoolUpdateEvent<MoveItemRequest>> myObserver;

    public static BlockBuildingSite InstantiateNew(Vector3Int position, Block blockToBuild, Type itemRequired)
    {
        GameObject prefabObj = PrefabLoader.GetPrefab(prefabName);
        GameObject obj = Instantiate(prefabObj) as GameObject;
        if (!obj) throw new System.Exception("Could not instantiate prefab " + prefabName);
        GridActor actor = obj.GetComponent<GridActor>();
        if (!actor) throw new System.Exception("No GridActor on prefab " + prefabName);
        actor.Move(position);
        BlockBuildingSite bbs = obj.GetComponent<BlockBuildingSite>();
        if (!bbs) throw new System.Exception("No BlockBuildingSite on prefab " + prefabName);
        bbs.data.targetBlock = blockToBuild;
        bbs.data.itemRequired = itemRequired;
        return bbs;
    }
    void Start()
    {
        inventory = GetComponent<InventoryComponent>();
        if (inventory)
        {
            inventoryObserver = new SimpleObserver<InventoryUpdateEvent>(inventory, OnInventoryUpdate);
        }
        actor = GetComponent<GridActor>();
        if (actor)
        {
            transform.position = actor.Position;
        }
        if (!data.hasSpawnedRequest)
        {
            MoveItemRequest req = new MoveItemRequest(data.itemRequired, actor.Position);
            data.requestGuid = req.Guid;
            MoveItemRequestPool.Instance.PostRequest(req);
            data.hasSpawnedRequest = true;
        }
        myObserver = new SimpleObserver<RequestPoolUpdateEvent<MoveItemRequest>>(MoveItemRequestPool.Instance, (updateEvent) =>
        {
            if (updateEvent.Type == RequestPoolUpdateEvent<MoveItemRequest>.EventType.Cancelled && updateEvent.Request.Guid.Equals(data.requestGuid))
            {
                data.requestFinished = true;
                GameObject.Destroy(gameObject);
            }
        });

        BlockVisualizer visualizer = GetComponent<BlockVisualizer>();
        if (visualizer)
        {
            visualizer.RenderBlock(GetBlock());
        }
    }

    public Block GetBlock()
    {
        return data.targetBlock;
    }

    void OnDestroy()
    {

        if (!data.requestFinished && data.hasSpawnedRequest)
        {
            // I.e. if we got destroyed while our request is still out there
            MoveItemRequestPool.Instance.CancelRequest(data.requestGuid);
        }
    }

    void OnInventoryUpdate(InventoryUpdateEvent update)
    {
        if (update.Type != InventoryUpdateEvent.UpdateType.Added) return;
        // Need to add a more thorough check on what we added here
        if (actor)
        {
            GridMap.Instance.SetBlock(actor.Position, GetBlock());
        }
        data.requestFinished = true;
        GameObject.Destroy(gameObject);
    }

    public IGenericSaveData Save()
    {
        return data;
    }

    public void Load(IGenericSaveData saveData)
    {
        data = (SaveData)saveData;
    }
}