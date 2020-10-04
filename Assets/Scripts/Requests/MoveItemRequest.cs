
using Items;
using UnityEngine;

[System.Serializable]
public class MoveItemRequest : PlayerRequest
{
    readonly ItemType typeToFind;
    readonly Vector3Int positionToMoveTo;

    public MoveItemRequest(ItemType typeToFind, Vector3Int positionToMoveTo)
    {
        this.typeToFind = typeToFind;
        this.positionToMoveTo = positionToMoveTo;
    }

    public Vector3Int PositionToMoveTo => positionToMoveTo;

    public ItemType TypeToFind => typeToFind;

    public override bool Equals(object obj)
    {
        return obj is MoveItemRequest request &&
               typeToFind == request.typeToFind &&
               positionToMoveTo.Equals(request.positionToMoveTo) &&
               PositionToMoveTo.Equals(request.PositionToMoveTo) &&
               TypeToFind == request.TypeToFind;
    }

    public override void Finish()
    {
        Cancel();
    }

    public override int GetHashCode()
    {
        int hashCode = -1314807358;
        hashCode = hashCode * -1521134295 + typeToFind.GetHashCode();
        hashCode = hashCode * -1521134295 + positionToMoveTo.GetHashCode();
        return hashCode;
    }
}
