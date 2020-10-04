using UnityEngine;

public class BlockBuildingSite : MonoBehaviour
{
    InventoryComponent inventory;
    GridActor actor;

    const string prefabName = "Prefabs/BlockBuildingObject";
    static GameObject prefabObj;

    public static BlockBuildingSite InstantiateNew(Vector3Int position)
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
        MoveItemRequestPool.Instance.PostRequest(new MoveItemRequest(Items.ItemType.RockBlock, actor.GetPos()));
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
            GridMap.SetBlock(actor.GetPos(), new RockBlock());
            GameObject.Destroy(this);
        }
    }
}