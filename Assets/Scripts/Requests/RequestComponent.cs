using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Saves and loads the RequestPools
public class RequestComponent : MonoBehaviour, ISaveableComponent
{
    [System.Serializable]
    private class SaveData : GenericSaveData<RequestComponent>
    {
        public IGenericSaveData miningRequestPoolSave;
        public IGenericSaveData moveItemRequestPoolSave;
    }
    public void Load(IGenericSaveData data)
    {
        SaveData save = data as SaveData;
        MiningRequestPool.Instance.Load(save.miningRequestPoolSave);
        MoveItemRequestPool.Instance.Load(save.moveItemRequestPoolSave);
    }

    public IGenericSaveData Save()
    {
        return new SaveData()
        {
            miningRequestPoolSave = MiningRequestPool.Instance.GetSave(),
            moveItemRequestPoolSave = MoveItemRequestPool.Instance.GetSave()
        };
    }
}
