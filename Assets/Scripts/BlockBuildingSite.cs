using UnityEngine;
using Items;
using System;

public class BlockBuildingSite : MonoBehaviour, ISaveableComponent
{
    [System.Serializable]
    class SaveData : GenericSaveData<BlockBuildingSite>
    {
        public bool hasSpawnedRequest = false;
        public Guid requestGuid;
    }

    GridActor actor;

    InventoryComponent inventory;
    SaveData data = new SaveData();
    const string prefabName = "Prefabs/BlockBuildingObject";
    static GameObject prefabObj;

    Block targetBlock;

    Guid requestGuid;

    public static BlockBuildingSite InstantiateNew(Vector3Int position, Block blockToBuild)
    {

        if (prefabObj == null)
        {
            prefabObj = Resources.Load(prefabName) as GameObject;
            if (!prefabObj) throw new System.Exception("Could not load prefab " + prefabName);
        }
        GameObject obj = Instantiate(prefabObj) as GameObject;
        if (!obj) throw new System.Exception("Could not instantiate prefab " + prefabName);
        GridActor actor = obj.GetComponent<GridActor>();
        if (!actor) throw new System.Exception("No GridActor on prefab " + prefabName);
        actor.Move(position);
        BlockBuildingSite bbs = obj.GetComponent<BlockBuildingSite>();
        if (!bbs) throw new System.Exception("No BlockBuildingSite on prefab " + prefabName);
        bbs.targetBlock = blockToBuild;
        return bbs;
    }
    void Start()
    {
        inventory = GetComponent<InventoryComponent>();
        if (inventory)
        {
            inventory.RegisterOnItemAddedCallback(OnItemAdded);
        }
        actor = GetComponent<GridActor>();
        if (actor)
        {
            transform.position = actor.GetPos();
        }
        if (!data.hasSpawnedRequest)
        {
            MoveItemRequest req = new MoveItemRequest(Items.ItemType.RockBlock, actor.GetPos());
            MoveItemRequestPool.Instance.PostRequest(req);
            data.hasSpawnedRequest = true;
            data.requestGuid = req.Guid;
        }

        MoveItemRequestPool.Instance.RegisterOnCancelledCallback(data.requestGuid, OnRequestCancelled);

        BlockVisualizer visualizer = GetComponent<BlockVisualizer>();
        if (visualizer)
        {
            visualizer.RenderBlock(GetBlock());
        }
    }

    public Block GetBlock()
    {
        return targetBlock;
    }

    void OnRequestCancelled(MoveItemRequest req)
    {
        GameObject.Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (inventory)
        {
            inventory.UnregisterOnItemAddedCallback(OnItemAdded);
        }
        
        MoveItemRequestPool.Instance.UnregisterOnCancelledCallback(data.requestGuid);
    }

    void OnItemAdded()
    {
        if (actor)
        {
            GridMap.Instance.SetBlock(actor.GetPos(), GetBlock());
            GameObject.Destroy(gameObject);
        }
    }

    public IGenericSaveData Save()
    {
        return data;
    }

    public void Load(IGenericSaveData data)
    {
        data = (SaveData)data;
    }
}