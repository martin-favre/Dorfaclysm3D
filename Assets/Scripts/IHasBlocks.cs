
using UnityEngine;

public interface IHasBlocks
{
    bool TryGetBlock(Vector3Int pos, out Block block);
    Block GetBlock(Vector3Int pos);
    void SetBlock(Vector3Int pos, Block block);
    Vector3Int GetSize();
}