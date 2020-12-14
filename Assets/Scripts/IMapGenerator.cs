using System;

public interface IMapGenerator
{
    void Generate(IHasBlocks map, Action onGenerationDone);
}