using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Items;

namespace Items
{
    public class DroppedItemComponent : MonoBehaviour
    {
        const string prefabName = "Prefabs/ItemContainerObject";
        static GameObject prefabObj;

        GridActor actor;
        InventoryComponent inventory;

        public static DroppedItemComponent InstantiateNew(Vector3Int position)
        {
            if (prefabObj == null)
            {
                prefabObj = Resources.Load(prefabName) as GameObject;
                if (!prefabObj) throw new System.Exception("Could not load prefab " + prefabName);
            }
            GameObject obj = Instantiate(prefabObj) as GameObject;
            if (!obj) throw new System.Exception("Could not instantiate prefab " + prefabName);
            GridActor gridActor = obj.GetComponent<GridActor>();
            if (!gridActor) throw new System.Exception("No GridActor Component on " + prefabName);
            gridActor.Move(position);
            DroppedItemComponent comp = obj.GetComponent<DroppedItemComponent>();
            if (!comp) throw new System.Exception("No ItemContainerComponent Component on " + prefabName);
            return comp;
        }

        void Start()
        {
            actor = GetComponent<GridActor>();
            inventory = GetComponent<InventoryComponent>();
            if (actor && inventory)
            {
                inventory.RegisterOnItemRemovedCallback(OnItemRemoved);
                ItemMap.RegisterInventory(inventory, actor.GetPos());
                transform.position = actor.GetPos() + new Vector3(.5f, -.5f, .5f);
            }
        }

        void OnItemRemoved()
        {
            if (!inventory.HasItems())
            {
                print("No items, DroppedItemComponent removing itself");
                GameObject.Destroy(gameObject);
            }
        }

        void OnDestroy()
        {
            if (actor && inventory)
            {
                ItemMap.UnregisterInventory(inventory, actor.GetPos());
            }
        }


    }
}