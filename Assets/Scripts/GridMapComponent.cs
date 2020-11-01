using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class GridMapComponent : MonoBehaviour, ISaveableComponent
{

    static readonly int chunkSize = 8;
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

    CameraController mainCam;
    private void Awake()
    {
    }

    private void Start()
    {
        mainCam = Camera.main.GetComponent<CameraController>();

    }

    void OnVerticalLevelChange()
    {
        UpdateMaxVerticalLevel();
        RegenerateMeshes();
    }

    void UpdateMaxVerticalLevel()
    {
        if(!mainCam) return;
        int newY = mainCam.GetVerticalPosition();
        foreach (KeyValuePair<Vector3Int, ChunkMeshGenerator> gen in chunkGenerators)
        {
            gen.Value.MaxY = (int?)newY;
        }

    }

    void OnBlockUpdate(Vector3Int pos)
    {
        Vector3Int size = GridMap.Instance.GetSize();
        Vector3Int chunkPos = new Vector3Int(pos.x / chunkSize, pos.y / chunkSize, pos.z / chunkSize);
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
        Vector3Int mapSize = GridMap.Instance.GetSize();
        int xSize = mapSize.x / chunkSize;
        int ySize = mapSize.y / chunkSize;
        int zSize = mapSize.z / chunkSize;
        if (xSize == 0) xSize = 1;
        if (ySize == 0) ySize = 1;
        if (zSize == 0) zSize = 1;
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                for (int z = 0; z < zSize; z++)
                {
                    GameObject newObj = (GameObject)Instantiate(chunkGeneratorPrefab, transform);
                    newObj.transform.position = Vector3.zero;
                    ChunkMeshGenerator comp = newObj.GetComponent<ChunkMeshGenerator>();
                    comp.ChunkOrigin = new Vector3Int(x * chunkSize, y * chunkSize, z * chunkSize);
                    comp.ChunkSize = chunkSize;
                    comp.BlockOwner = GridMap.Instance;
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
        GridMap.Instance.UnregisterCallbackOnBlockChange(OnBlockUpdate);
        BlockEffectMap.UnregisterOnEffectAddedCallback(OnBlockUpdate);
        if (mainCam)
        {
            mainCam.ClearOnVerticalLevelChanged();
        }
    }

    void Update()
    {
        switch (state)
        {
            case State.Created:
                state = State.GeneratingMap;
                Debug.Log("generating new map");
                generationTask = Task.Run(() => GridMap.Instance.GenerateMap(new Vector3Int(16, 16, 16)));
                break;
            case State.GeneratingMap:
            case State.LoadingMap:
                if (GridMap.Instance.IsGenerationDone())
                {
                    print("Map finished generation");
                    state = State.MapGenerated;
                    GridMap.Instance.RegisterCallbackOnBlockChange(OnBlockUpdate);
                    BlockEffectMap.RegisterOnEffectAddedCallback(OnBlockUpdate);
                }
                break;
            case State.MapGenerated:
                print("Creating meshGenerators");
                createMeshCreators();
                UpdateMaxVerticalLevel();
                RegenerateMeshes();
                if (mainCam)
                {
                    mainCam.SetOnVerticalLevelChanged(OnVerticalLevelChange);
                }
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
        save.gridmap = GridMap.Instance.GetSave();
        return save;
    }

    public void Load(IGenericSaveData data)
    {
        SaveData save = (SaveData)data;
        state = State.LoadingMap;
        generationTask = Task.Run(() => GridMap.Instance.LoadSave(save.gridmap));
    }
}
