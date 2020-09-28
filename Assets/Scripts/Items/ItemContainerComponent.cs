using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Items;
public class ItemContainerComponent : MonoBehaviour
{
    const string prefabName = "Prefabs/ItemContainerObject";
    static GameObject prefabObj;

    ItemStack itemStack;

    public static ItemContainerComponent InstantiateNew(IItem item, uint count)
    {
        if (prefabObj == null)
        {
            prefabObj = Resources.Load(prefabName) as GameObject;
            if (!prefabObj) throw new System.Exception("Could not load prefab " + prefabName);
        }
        GameObject obj = Instantiate(prefabObj) as GameObject;
        if (!obj) throw new System.Exception("Could not instantiate prefab " + prefabName);
        ItemContainerComponent comp = obj.GetComponent<ItemContainerComponent>();
        comp.itemStack = new ItemStack(item, count);
        if (!comp) throw new System.Exception("No ItemContainerComponent Component on " + prefabName);
        return comp;
    }
    public IItem GetItem()
    {
        return itemStack.GetItem();
    }

    public bool MayAddItem(IItem item)
    {
        return itemStack.MayAddItem(item);
    }

    public void AddItem(IItem item)
    {
        itemStack.AddItem(item);
    }
}
