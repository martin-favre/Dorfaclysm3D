using System;

public interface IMapGenerator
{
    void Generate(IHasBlocks map, Action onGenerationDone);
    
    // returns 0->1
    float GetProgress();

    bool IsComplete();
}