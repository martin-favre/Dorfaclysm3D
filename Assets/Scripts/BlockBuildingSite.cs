using UnityEngine;
using Items;
using System;

public class BlockBuildingSite : MonoBehaviour, ISaveableComponent
{
    [System.Serializable]
    class SaveData : GenericSaveData<BlockBuildingSite>
    {
        public bool hasSpawnedRequest = false;

    }

    GridActor actor;

    InventoryComponent inventory;
    SaveData data = new SaveData();
    const string prefabName = "Prefabs/BlockBuildingObject";
    static GameObject prefabObj;

    Block targetBlock;

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
            transform.position = actor.GetPos();// + new Vector3(.5f, -.5f, .5f);
        }
        if (!data.hasSpawnedRequest)
        {
            MoveItemRequestPool.Instance.PostRequest(new MoveItemRequest(Items.ItemType.RockBlock, actor.GetPos()));

            data.hasSpawnedRequest = true;
        }

        BlockVisualizer visualizer = GetComponent<BlockVisualizer>();
        if(visualizer) {
            visualizer.RenderBlock(GetBlock());
        }
    }

    public Block GetBlock() 
    {
        return targetBlock;
    }



    void OnDestroy()
    {
        if (inventory)
        {
            inventory.UnregisterOnItemAddedCallback(OnItemAdded);
        }

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