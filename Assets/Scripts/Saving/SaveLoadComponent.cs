using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SaveLoadComponent : MonoBehaviour
{
    // Prefab must be located under Assets/Resources/
    public string prefabName;

    [System.Serializable]
    public struct GameobjectSaveData
    {
        public string prefabPath;
        public IComponentSaveData[] saves;
    }
    public GameobjectSaveData GetSave()
    {
        GameobjectSaveData save = new GameobjectSaveData();
        if (prefabName != null)
        {
            save.prefabPath = prefabName;
            print(save.prefabPath);
            if (save.prefabPath.Length == 0) throw new Exception("Invalid prefab path on object " + gameObject.name);
        }
        else
        {
            Debug.LogError("No prefab on saveable object");
        }

        save.saves = GetSavesFromComponents();
        return save;
    }
    // Start is called before the first frame update
    void Start()
    {
        SaveLoadManager.Register(this);
    }

    void OnDestroy()
    {
        SaveLoadManager.Unregister(this);
    }

    IComponentSaveData[] GetSavesFromComponents()
    {
        var saveables = GetSaveableComponents();
        IComponentSaveData[] saves = new IComponentSaveData[saveables.Count];
        int i = 0;
        foreach (var saveable in saveables)
        {
            saves[i] = saveable.Save();
        }
        return saves;
    }

    List<ISaveableComponent> GetSaveableComponents()
    {
        List<ISaveableComponent> saveables = new List<ISaveableComponent>();
        var comps = GetComponents<Component>();
        foreach (Component c in comps)
        {
            ISaveableComponent saveable = c as ISaveableComponent;
            if (saveable != null)
            {
                saveables.Add(saveable);
            }
        }
        return saveables;
    }

    public void Load(GameobjectSaveData fullSave)
    {
        foreach (var save in fullSave.saves)
        {
            try
            {
                Type type = Type.GetType(save.GetAssemblyName());
                Component comp = GetComponent(type);
                if (comp == null) throw new Exception("No component of type " + type.ToString() + " on gameobject " + gameObject.name);
                ISaveableComponent saveable = (ISaveableComponent)comp;
                if (saveable == null) throw new Exception("Component of type " + type.ToString() + " on gameobject " + gameObject.name + " is not a ISaveable");
                saveable.Load(save);
            }
            catch (Exception e)
            {
                Debug.LogError("Error when loading component: " + e.ToString());
            }
        }

    }

    void Update()
    {

    }
}
