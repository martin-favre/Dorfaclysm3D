
using Items;
using UnityEngine;

[System.Serializable]
public class GrassBlock : SolidBlock
{
    static readonly Vector2 texturePos = TexturePositions.Get(TexturePositions.Name.GrassBlock);
    public GrassBlock() : base() { }

    public override object Clone()
    {
        return new GrassBlock();
    }

    public override Item GetItem()
    {
        return new RockBlockItem();
    }

    public override PartMeshInfo GetMesh(Vector3Int thisBlockPos, int maxY, IHasBlocks blockOwner)
    {
        return GenerateMesh(thisBlockPos, maxY, blockOwner);
    }

    public override string GetName()
    {
        return "Grass Block";
    }

    public override Vector2 GetTexturePos()
    {
        return texturePos;
    }
}