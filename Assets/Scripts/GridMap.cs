using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Concurrent;
using System.Threading;
using System;
using Items;

public class GridMap : IHasBlocks
{
    [System.Serializable]
    private class SaveData
    {
        public Vector3Int size = new Vector3Int();
        public ConcurrentDictionary<Vector3Int, Block> blocks = new ConcurrentDictionary<Vector3Int, Block>();
        public bool generated = false;

    }
    static readonly GridMap instance = new GridMap();
    Vector3Int mapSize = Vector3Int.zero;
    SaveData data = new SaveData();
    bool generated = false;
    readonly object blockLock = new object();
    ConcurrentList<Action<Vector3Int>> runOnBlockChange = new ConcurrentList<Action<Vector3Int>>();

    public static GridMap Instance { get => instance; }

    GridMap() { }

    public object GetSave()
    {
        return data;
    }

    public void LoadSave(object data)
    {
        lock (blockLock)
        {
            data = (SaveData)data;
        }
    }

    public void GenerateMap(Vector3Int size)
    {
        SetSize(size);
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int z = 0; z < mapSize.z; z++)
                {
                    Vector3Int pos = new Vector3Int(x, y, z);

                    if (y == x && (z == 5 || z == 10))
                    {
                        SetBlock(pos, new StairUpDownBlock(new Vector3(0, 90, 0)));
                    }
                    else if (y == x)
                    {
                        SetBlock(pos, new GrassBlock());
                    }
                    else if (y < x)
                    {
                        SetBlock(pos, new RockBlock());
                    }
                    else
                    {
                        SetBlock(pos, new AirBlock());
                    }
                }
            }
        }
        SetGenerationDone();
    }

    private void SetGenerationDone()
    {
        generated = true;
    }

    public bool IsGenerationDone()
    {
        return generated;
    }
    public void RegisterCallbackOnBlockChange(Action<Vector3Int> func)
    {
        runOnBlockChange.Add(func);
    }

    public void UnregisterCallbackOnBlockChange(Action<Vector3Int> func)
    {
        runOnBlockChange.Remove(func);
    }

    public bool IsPosInMap(Vector3Int pos)
    {
        return data.blocks.ContainsKey(pos);
    }

    public bool IsPosFree(Vector3Int pos)
    {
        if (!IsBlockFree(pos)) return false;
        return GridActorMap.IsPosFree(pos);
    }

    private bool IsBlockFree(Vector3Int pos)
    {
        Block block;
        TryGetBlock(pos, out block);
        if (block != null)
        {
            return block.supportsWalkingThrough();
        }
        return false; // If we don't have a block, you can't walk there
    }

    public bool TryGetBlock(Vector3Int pos, out Block block)
    {
        if (!IsGenerationDone())
        {
            block = null;
            return false;
        }
        return data.blocks.TryGetValue(pos, out block);
    }

    public Block GetBlock(Vector3Int pos)
    {
        Block block;

        if (!TryGetBlock(pos, out block))
        {
            throw new KeyNotFoundException("No block in position");
        }
        return block;
    }
    private void SetSize(Vector3Int size)
    {
        lock (blockLock)
        {
            mapSize = size;
        }
    }

    public Vector3Int GetSize()
    {
        lock (blockLock)
        {
            return mapSize;
        }
    }

    bool InMap(Vector3Int pos)
    {
        return data.blocks.ContainsKey(pos);
    }

    private void RunCallbacks(Vector3Int pos)
    {
        runOnBlockChange.ForEach((a) => { a(pos); });
    }

    public void PutItem(Vector3Int pos, Item itemToAdd)
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

    public void SetBlock(Vector3Int pos, Block block)
    {

        Block prevBlock;
        lock (blockLock)
        {
            data.blocks.TryGetValue(pos, out prevBlock);
            data.blocks[pos] = block;
        }

        if (IsGenerationDone())
        {
            RunCallbacks(pos);
            if (prevBlock != null && block.Type == Block.BlockType.airBlock)
            {
                PutItem(pos, prevBlock.GetItem());
            }

        }
    }

}
