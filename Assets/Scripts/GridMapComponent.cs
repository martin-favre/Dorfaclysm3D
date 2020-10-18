using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class GridMapComponent : MonoBehaviour, ISaveableComponent
{

    static readonly Vector3Int chunkSize = new Vector3Int(8, 8, 8);
    public GameObject chunkGeneratorPrefab;

    Dictionary<Vector3Int, ChunkMeshGenerator> chunkGenerators;

    Task generationTask;
    enum State
    {
        Created,
        LoadingMap,
        GeneratingMap,
        MapGenerated,
        MeshGeneratorsCreated
    }


    State state = State.Created;
    private void Awake()
    {
    }

    void OnBlockUpdate(Vector3Int pos)
    {
        Vector3Int size = GridMap.GetSize();
        Vector3Int chunkMapSize = new Vector3Int(size.x / chunkSize.x, size.y / chunkSize.y, size.z / chunkSize.z);
        int index = (pos.x + size.x * (pos.y + size.y * pos.z)) / (chunkSize.x * chunkSize.y * chunkSize.z);
        Vector3Int chunkPos = new Vector3Int(pos.x / chunkSize.x, pos.y / chunkSize.y, pos.z / chunkSize.z);
        foreach (Vector3Int delta in DeltaPositions.DeltaPositions3D)
        {
            Vector3Int neighbourPos = chunkPos + delta;
            ChunkMeshGenerator neighbour;
            chunkGenerators.TryGetValue(neighbourPos, out neighbour);
            if (neighbour != null)
            {
                neighbour.GenerateMesh();
            }
        }
        chunkGenerators[chunkPos].GenerateMesh();
    }

    public void RegenerateMeshes()
    {
        Debug.Log("Regenerating meshes");
        foreach (KeyValuePair<Vector3Int, ChunkMeshGenerator> gen in chunkGenerators)
        {
            gen.Value.GenerateMesh();
        }
    }

    private void createMeshCreators()
    {
        chunkGenerators = new Dictionary<Vector3Int, ChunkMeshGenerator>();
        Vector3Int mapSize = GridMap.GetSize();
        int xSize = mapSize.x / chunkSize.x;
        int ySize = mapSize.y / chunkSize.y;
        int zSize = mapSize.z / chunkSize.z;
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                for (int z = 0; z < zSize; z++)
                {
                    GameObject newObj = (GameObject)Instantiate(chunkGeneratorPrefab, transform);
                    newObj.transform.position = Vector3.zero;
                    ChunkMeshGenerator comp = newObj.GetComponent<ChunkMeshGenerator>();
                    comp.ChunkOrigin = new Vector3Int(x * chunkSize.x, y * chunkSize.y, z * chunkSize.z);
                    chunkGenerators[new Vector3Int(x, y, z)] = comp;
                }
            }
        }

    }

    void OnDestroy()
    {
        if (generationTask != null)
        {
            generationTask.GetAwaiter().GetResult(); // Sleep until task done
        }
        GridMap.UnregisterCallbackOnBlockChange(OnBlockUpdate);
    }

    void Update()
    {
        switch (state)
        {
            case State.Created:
                state = State.GeneratingMap;
                Debug.Log("generating new map");
                generationTask = Task.Run(() => GridMap.GenerateMap(new Vector3Int(8, 8, 8)));
                break;
            case State.GeneratingMap:
            case State.LoadingMap:
                if (GridMap.IsGenerationDone())
                {
                    print("Map finished generation");
                    state = State.MapGenerated;
                    GridMap.RegisterCallbackOnBlockChange(OnBlockUpdate);
                }
                break;
            case State.MapGenerated:
                print("Creating meshGenerators");
                createMeshCreators();
                state = State.MeshGeneratorsCreated;
                break;
            case State.MeshGeneratorsCreated:
                break;
        }
    }

    [System.Serializable]
    private class SaveData : GenericSaveData<GridMapComponent>
    {
        public object gridmap;
    }

    public IGenericSaveData Save()
    {
        SaveData save = new SaveData();
        save.gridmap = GridMap.GetSave();
        return save;
    }

    public void Load(IGenericSaveData data)
    {
        SaveData save = (SaveData)data;
        state = State.LoadingMap;
        generationTask = Task.Run(() => GridMap.LoadSave(save.gridmap));
    }
}
