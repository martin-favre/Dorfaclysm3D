using System;
using Items;
using UnityEngine;

public class DroppedItemManager
{
    public static void PutItem(Vector3Int pos, Item itemToAdd)
    {
        if (itemToAdd == null) return;
        Debug.Log("GridMap, Adding item to " + pos);
        GridActor[] actors = GridActorMap.GetGridActors(pos);
        DroppedItemComponent itemCont = null;
        foreach (GridActor actor in actors)
        {
            DroppedItemComponent tmpCont = actor.GetComponent<DroppedItemComponent>();
            if (tmpCont != null)
            {
                itemCont = tmpCont;
                break;
            }
        }
        if (itemCont == null)
        {
            itemCont = DroppedItemComponent.InstantiateNew(pos);

            Debug.Log("GridMap, Spawned new DroppedItemComponent");
        }

        InventoryComponent inv = itemCont.gameObject.GetComponent<InventoryComponent>();
        if (inv)
        {
            inv.AddItem(itemToAdd);
        }
        else
        {
            Debug.LogWarning("GridMap, No inventory to drop item in");
        }
    }

    public static Item GetItem(Vector3Int pos, Type typeToFind)
    {
        GridActor[] actors = GridActorMap.GetGridActors(pos);
        foreach (GridActor actor in actors)
        {
            InventoryComponent comp = actor.GetComponent<InventoryComponent>();
            if (comp && comp.HasItem(typeToFind))
            {
                return comp.GetItem(typeToFind);
            }
        }

        return null;
    }

}