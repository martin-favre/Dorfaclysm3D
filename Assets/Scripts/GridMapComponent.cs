using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class GridMapComponent : MonoBehaviour, ISaveable
{

    static readonly Vector3Int mChunkSize = new Vector3Int(8, 8, 8);
    public Sprite mTexture;
    public GameObject mChunkGeneratorPrefab;

    Dictionary<Vector3Int, ChunkMeshGenerator> mChunkGenerators;

    Task mGenerationTask;
    enum State
    {
        Created,
        LoadingMap,
        GeneratingMap,
        MapGenerated,
        MeshGeneratorsCreated
    }


    State mState = State.Created;
    private void Awake()
    {
    }

    void OnBlockUpdate(Vector3Int pos)
    {
        Vector3Int size = GridMap.GetSize();
        Vector3Int chunkMapSize = new Vector3Int(size.x / mChunkSize.x, size.y / mChunkSize.y, size.z / mChunkSize.z);
        int index = (pos.x + size.x * (pos.y + size.y * pos.z)) / (mChunkSize.x * mChunkSize.y * mChunkSize.z);
        Vector3Int chunkPos = new Vector3Int(pos.x / mChunkSize.x, pos.y / mChunkSize.y, pos.z / mChunkSize.z);
        Debug.Log("Update on block " + pos + " index " + index);
        foreach (Vector3Int delta in DeltaPositions.DeltaPositions3D)
        {
            Vector3Int neighbourPos = chunkPos + delta;
            ChunkMeshGenerator neighbour;
            mChunkGenerators.TryGetValue(neighbourPos, out neighbour);
            if (neighbour != null)
            {
                neighbour.GenerateMesh();
            }
        }
        mChunkGenerators[chunkPos].GenerateMesh();
    }

    private void createMeshCreators()
    {
        mChunkGenerators = new Dictionary<Vector3Int, ChunkMeshGenerator>();
        Vector3Int mapSize = GridMap.GetSize();
        int xSize = mapSize.x / mChunkSize.x;
        int ySize = mapSize.y / mChunkSize.y;
        int zSize = mapSize.z / mChunkSize.z;
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                for (int z = 0; z < zSize; z++)
                {
                    GameObject newObj = (GameObject)Instantiate(mChunkGeneratorPrefab, transform);
                    newObj.transform.position = Vector3.zero;
                    ChunkMeshGenerator comp = newObj.GetComponent<ChunkMeshGenerator>();
                    comp.ChunkOrigin = new Vector3Int(x * mChunkSize.x, y * mChunkSize.y, z * mChunkSize.z);
                    mChunkGenerators[new Vector3Int(x, y, z)] = comp;
                }
            }
        }

    }

    void OnDestroy()
    {
        if (mGenerationTask != null)
        {
            mGenerationTask.GetAwaiter().GetResult(); // Sleep until task done
        }
        GridMap.UnregisterCallbackOnBlockChange(OnBlockUpdate);
    }

    void Update()
    {
        switch (mState)
        {
            case State.Created:
                mState = State.GeneratingMap;
                Debug.Log("generating new map");
                mGenerationTask = Task.Run(() => GridMap.GenerateMap(new Vector3Int(16, 16, 16)));
                break;
            case State.GeneratingMap:
            case State.LoadingMap:
                if (GridMap.IsGenerationDone())
                {
                    print("Map finished generation");
                    mState = State.MapGenerated;
                    GridMap.RegisterCallbackOnBlockChange(OnBlockUpdate);
                }
                break;
            case State.MapGenerated:
                print("Creating meshGenerators");
                createMeshCreators();
                mState = State.MeshGeneratorsCreated;
                break;
            case State.MeshGeneratorsCreated:
                break;
        }
    }

    [System.Serializable]
    private class SaveData : IComponentSaveData
    {
        public string GetAssemblyName()
        {
            return typeof(GridMapComponent).AssemblyQualifiedName;
        }
        public object gridmap;
    }

    public IComponentSaveData Save()
    {
        SaveData save = new SaveData();
        save.gridmap = GridMap.GetSave();
        return save;
    }

    public void Load(IComponentSaveData data)
    {
        SaveData save = (SaveData)data;
        mState = State.LoadingMap;
        mGenerationTask = Task.Run(() => GridMap.LoadSave(save.gridmap));
    }
}
