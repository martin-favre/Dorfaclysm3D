using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Logging;

public class GridMapComponent : MonoBehaviour, ISaveableComponent
{

    static readonly int chunkSize = 8;
    public GameObject chunkGeneratorPrefab;

    Dictionary<Vector3Int, ChunkMeshGenerator> chunkGenerators;

    Task generationTask;
    LilLogger logger = new LilLogger("GridMapComponent");
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

    SimpleObserver<CameraController> cameraObserver;
    private void Awake()
    {
    }

    private void Start()
    {
        mainCam = Camera.main.GetComponent<CameraController>();
    }

    public bool regenerateMap = false;
    public Vector3Int mapSize;
    public Vector2 offset;
    public Vector2 frequency;
    public float heightExponential;
    public float heighFactor;
    public int noiseIterations;
    public float waterLevel;
    public float snowLevel;
    GenerationParameters oldParameters = new GenerationParameters();

    void UpdateMaxVerticalLevel()
    {
        if (!mainCam) return;
        int newY = mainCam.GetVerticalPosition();
        ChunkMeshGenerator.MaxY = (int?)newY;
    }

    void OnBlockUpdate(Vector3Int pos)
    {
        logger.Log("OnBlockUpdate");
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
        logger.Log("Regenerating meshes");
        foreach (KeyValuePair<Vector3Int, ChunkMeshGenerator> gen in chunkGenerators)
        {
            gen.Value.GenerateMesh();
        }
    }

    GenerationParameters GetParameters()
    {
        return new GenerationParameters
        {
            offset = offset,
            frequency = frequency,
            heighFactor = heighFactor,
            heightExponential = heightExponential,
            noiseIterations = noiseIterations,
            waterLevel = waterLevel,
            snowLevel = snowLevel,
            size = mapSize
        };


    }

    // Only to be called by user in editor
    // As not much though has been placed in here
    private void RegenerateMap()
    {
        GridMap.Instance.UnregisterCallbackOnBlockChange(OnBlockUpdate);
        BlockEffectMap.UnregisterOnEffectAddedCallback(OnBlockUpdate);
        oldParameters = GetParameters();
        generationTask = Task.Run(() =>
        {
            MapGenerator generator = new MapGenerator(oldParameters);
            GridMap.Instance.GenerateMap(generator);
            RegenerateMeshes();
            GridMap.Instance.RegisterCallbackOnBlockChange(OnBlockUpdate);
            BlockEffectMap.RegisterOnEffectAddedCallback(OnBlockUpdate);
        });
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
    }

    void Update()
    {


        switch (state)
        {
            case State.Created:
                state = State.GeneratingMap;
                logger.Log("generating new map");

                GenerationParameters parameters = GetParameters();
                MapGenerator generator = new MapGenerator(parameters);
                oldParameters = parameters;
                generationTask = Task.Run(() => GridMap.Instance.GenerateMap(generator));
                break;
            case State.GeneratingMap:
            case State.LoadingMap:
                if (GridMap.Instance.IsGenerationDone())
                {
                    logger.Log("Map finished generation");
                    state = State.MapGenerated;
                    GridMap.Instance.RegisterCallbackOnBlockChange(OnBlockUpdate);
                    BlockEffectMap.RegisterOnEffectAddedCallback(OnBlockUpdate);
                }
                break;
            case State.MapGenerated:
                logger.Log("Creating meshGenerators");
                BlockMeshes.LoadMeshes();
                createMeshCreators();
                UpdateMaxVerticalLevel();
                RegenerateMeshes();
                state = State.MeshGeneratorsCreated;
                cameraObserver = new SimpleObserver<CameraController>(mainCam, (cam) =>
                {
                    UpdateMaxVerticalLevel();
                    RegenerateMeshes();
                });

                break;
            case State.MeshGeneratorsCreated:
                break;
        }
        if (!oldParameters.Equals(GetParameters()) && generationTask != null && !generationTask.Status.Equals(TaskStatus.Running))
        {
            regenerateMap = false;
            RegenerateMap();
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
        logger.Log("Saved GridMapComponent");
        return save;
    }

    public void Load(IGenericSaveData data)
    {
        SaveData save = (SaveData)data;
        state = State.LoadingMap;
        generationTask = Task.Run(() => GridMap.Instance.LoadSave(save.gridmap));
        logger.Log("Loaded GridMapComponent");
    }
}
