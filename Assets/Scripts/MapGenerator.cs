

using System.Collections.Generic;
using UnityEngine;

public class GenerationParameters
{
    public Vector2 offset;
    public Vector2 frequency;
    public float heighFactor;
    public float heightExponential;
    public int noiseIterations;

    public override bool Equals(object obj)
    {
        return obj is GenerationParameters parameters &&
               offset.Equals(parameters.offset) &&
               frequency.Equals(parameters.frequency) &&
               heighFactor == parameters.heighFactor &&
               heightExponential == parameters.heightExponential &&
               noiseIterations == parameters.noiseIterations;
    }
}

public class MapGenerator
{
    IHasBlocks map;
    float progress = 0; // 0 to 1
    bool done;
    GenerationParameters parameters;
    public MapGenerator(IHasBlocks map, GenerationParameters parameters)
    {
        this.map = map;
        this.parameters = parameters;
    }

    public bool Done { get => done; }
    public float Progress { get => progress; }


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
                for(int i = 0; i < parameters.noiseIterations; i++) {
                    noise += Mathf.PerlinNoise(xn, zn) / divider;
                    divider*=2;
                }
                int height = Mathf.RoundToInt(Mathf.Pow(noise*parameters.heighFactor, parameters.heightExponential));
                heightNoise[pos] = height; 
            }
        }
        return heightNoise;
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
                        map.SetBlock(pos, new GrassBlock());
                    }
                    else if (pos.y < height)
                    {
                        map.SetBlock(pos, new RockBlock());
                    }
                    else
                    {
                        map.SetBlock(pos, new AirBlock());
                    }
                    steps++;
                    progress = steps / stepsNeeded;
                }
            }
        }
    }
}