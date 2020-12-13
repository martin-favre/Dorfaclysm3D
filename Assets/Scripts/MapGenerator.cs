

using System;
using System.Collections.Generic;
using UnityEngine;

public class GenerationParameters
{
    public Vector2 offset;
    public Vector2 frequency;
    public float heighFactor;
    public float heightExponential;
    public int noiseIterations;
    public float waterLevel;
    public float snowLevel;
    

    public override bool Equals(object obj)
    {
        return obj is GenerationParameters parameters &&
               offset.Equals(parameters.offset) &&
               frequency.Equals(parameters.frequency) &&
               heighFactor == parameters.heighFactor &&
               heightExponential == parameters.heightExponential &&
               noiseIterations == parameters.noiseIterations &&
               waterLevel == parameters.waterLevel &&
               snowLevel == parameters.snowLevel;
    }

    public override int GetHashCode()
    {
        int hashCode = -429925968;
        hashCode = hashCode * -1521134295 + offset.GetHashCode();
        hashCode = hashCode * -1521134295 + frequency.GetHashCode();
        hashCode = hashCode * -1521134295 + heighFactor.GetHashCode();
        hashCode = hashCode * -1521134295 + heightExponential.GetHashCode();
        hashCode = hashCode * -1521134295 + noiseIterations.GetHashCode();
        hashCode = hashCode * -1521134295 + waterLevel.GetHashCode();
        hashCode = hashCode * -1521134295 + snowLevel.GetHashCode();
        return hashCode;
    }
}

public class MapGenerator
{
    IHasBlocks map;
    float progress = 0; // 0 to 1
    GenerationParameters parameters;
    Action callOnDone;
    public MapGenerator(IHasBlocks map, GenerationParameters parameters)
    {
        this.map = map;
        this.parameters = parameters;
    }

    public float Progress { get => progress; }
    System.Random random = new System.Random();

    Dictionary<Vector2Int, int> getHeighNoise()
    {
        Dictionary<Vector2Int, int> heightNoise = new Dictionary<Vector2Int, int>();
        for (int x = 0; x < map.GetSize().x; x++)
        {
            for (int z = 0; z < map.GetSize().z; z++)
            {
                Vector2Int pos = new Vector2Int(x, z);
                float xn = ((float)x + parameters.offset.x) * parameters.frequency.x;
                float zn = ((float)z + parameters.offset.y) * parameters.frequency.y;

                float noise = 0;
                float divider = 2;
                for (int i = 0; i < parameters.noiseIterations; i++)
                {
                    noise += Mathf.PerlinNoise(xn, zn) / divider;
                    divider *= 2;
                }
                int height = Mathf.RoundToInt(Mathf.Pow(noise * parameters.heighFactor, parameters.heightExponential));
                heightNoise[pos] = height;
            }
        }
        return heightNoise;
    }

    public void RegisterCallOnDone(Action onDone)
    {
        callOnDone = onDone;
    }

    void GenerateLeaves(Vector3Int pos, float chance)
    {
        if (random.NextDouble() > chance) return;

        Block block;
        map.TryGetBlock(pos, out block);
        if (block != null && block.Type != Block.BlockType.airBlock) return;
        map.SetBlock(pos, new LeafBlock());
        foreach (Vector3Int dir in DeltaPositions.DeltaPositions3D)
        {
            GenerateLeaves(pos + dir, chance*0.7f);
        }

    }

    void MakeTree(Vector3Int pos)
    {
        int height = random.Next(1, 10);
        Vector3Int peakPos = pos;
        for (int i = 0; i < height; i++)
        {
            map.SetBlock(peakPos, new WoodBlock());
            peakPos += new Vector3Int(0, 1, 0);
        }
        peakPos -= new Vector3Int(0, 1, 0);

        foreach (Vector3Int dir in DeltaPositions.DeltaPositions3D)
        {
            GenerateLeaves(peakPos + dir, 1);
        }
    }



    public bool BlockSet(Vector3Int pos)
    {
        Block block;
        return map.TryGetBlock(pos, out block);
    }

    public void Generate(Vector3Int size)
    {
        map.SetSize(size);

        int stepsNeeded = size.x * size.y * size.z;
        int steps = 0;

        Dictionary<Vector2Int, int> heightNoise = getHeighNoise();

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    Vector3Int pos = new Vector3Int(x, y, z);
                    int height = heightNoise[new Vector2Int(x, z)];
                    if (pos.y == height)
                    {
                        if (pos.y <= parameters.snowLevel)
                        {
                            map.SetBlock(pos, new GrassBlock());
                        }
                        else
                        {
                            map.SetBlock(pos, new RockBlock());
                        }
                    }
                    else if (pos.y < height)
                    {
                        map.SetBlock(pos, new RockBlock());
                    }
                    else if (pos.y == height + 1)
                    {
                        if (pos.y > parameters.snowLevel)
                        {
                            map.SetBlock(pos, new SnowBlock());
                        }
                        else if (random.Next(1, 40) == 1)
                        
                        {
                            //oneTree = false;
                            MakeTree(pos);
                        }
                    }
                    else if (pos.y < parameters.waterLevel)
                    {
                        map.SetBlock(pos, new WaterBlock());
                    }
                    else if(!BlockSet(pos))
                    {
                        map.SetBlock(pos, new AirBlock());
                    }
                    steps++;
                    progress = steps / stepsNeeded;
                }
            }
        }
        callOnDone?.Invoke();
    }
}