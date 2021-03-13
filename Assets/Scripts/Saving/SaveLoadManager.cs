using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;
using System;
using System.Runtime.Serialization;

public static class SaveLoadManager
{
    readonly static string savePath = Application.persistentDataPath + "/save.dat";

    static List<SaveLoadComponent> components = new List<SaveLoadComponent>();
    public static void Register(SaveLoadComponent comp)
    {
        components.Add(comp);
    }
    public static void Unregister(SaveLoadComponent comp)
    {
        components.Remove(comp);
    }

    public static void RequestSave()
    {
        List<SaveLoadComponent.GameobjectSaveData> saves = new List<SaveLoadComponent.GameobjectSaveData>();
        foreach (var comp in components)
        {
            saves.Add(comp.GetSave());
        }
        BinaryFormatter bf = CreateBinaryFormatter();
        FileStream file = File.Open(savePath, FileMode.OpenOrCreate);
        bf.Serialize(file, saves);
        file.Close();
    }

    static private void DestroyAllGameobjects()
    {
        foreach (var comp in components)
        {
            GameObject.Destroy(comp.gameObject);
        }
    }

    public static void RequestLoad()
    {

        DestroyAllGameobjects();
        if (!File.Exists(savePath))
        {
            Debug.Log("There's no save to load");
            return;
        }
        BinaryFormatter bf = CreateBinaryFormatter();
        FileStream file = File.Open(savePath, FileMode.Open);
        List<SaveLoadComponent.GameobjectSaveData> saves = bf.Deserialize(file) as List<SaveLoadComponent.GameobjectSaveData>;
        Debug.Log("Size is " + saves.Count);
        foreach (var save in saves)
        {
            try
            {
                string path = "Prefabs/" + save.prefabPath;
                GameObject obj = PrefabLoader.GetPrefab<GameObject>(path);
                if (obj == null) throw new Exception("Could not load prefab " + path);
                SaveLoadComponent comp = GameObject.Instantiate(obj).GetComponent<SaveLoadComponent>();
                if (comp == null) throw new Exception("No SaveLoadComponent on gameobject " + obj.name);
                comp.Load(save);
            }
            catch (Exception e)
            {
                Debug.LogError("Error when loading: " + e.ToString());
            }
        }

        file.Close();
    }

    private static BinaryFormatter CreateBinaryFormatter()
    {
        BinaryFormatter form = new BinaryFormatter();
        SurrogateSelector ss = new SurrogateSelector();
        {
            Vector3IntSerializationSurrogate v3iss = new Vector3IntSerializationSurrogate();
            ss.AddSurrogate(typeof(Vector3Int),
                    new StreamingContext(StreamingContextStates.All),
                    v3iss);
        }
        {
            Vector3SerializationSurrogate v3ss = new Vector3SerializationSurrogate();
            ss.AddSurrogate(typeof(Vector3),
            new StreamingContext(StreamingContextStates.All),
            v3ss);

        }
        {
            QuaternionSerializationSurrogate qss = new QuaternionSerializationSurrogate();
            ss.AddSurrogate(typeof(Quaternion),
            new StreamingContext(StreamingContextStates.All),
            qss);
        }
        {
            Vector2SerializationSurrogate v2ss = new Vector2SerializationSurrogate();
            ss.AddSurrogate(typeof(Vector2),
            new StreamingContext(StreamingContextStates.All),
            v2ss);
        }

        form.SurrogateSelector = ss;
        return form;
    }
}