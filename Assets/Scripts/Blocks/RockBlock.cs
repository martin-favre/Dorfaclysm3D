
using Items;
using UnityEngine;

[System.Serializable]
public class RockBlock : SolidBlock
{
    static readonly Vector2 texturePos = TexturePositions.RockBlock;
    public RockBlock() : base() { }
    public RockBlock(Vector3 rotation) : base(rotation) { }

    public override object Clone()
    {
        return new RockBlock();
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
        return "Rock Block";
    }

    public override Vector2 GetTexturePos()
    {
        return texturePos;
    }
}