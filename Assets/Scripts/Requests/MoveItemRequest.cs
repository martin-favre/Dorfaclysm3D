
using System;
using Items;
using UnityEngine;

[System.Serializable]
public class MoveItemRequest : PlayerRequest, IEquatable<MoveItemRequest>
{
    readonly Type typeToFind;
    readonly Vector3Int positionToMoveTo;

    public MoveItemRequest(Type typeToFind, Vector3Int positionToMoveTo)
    {
        this.typeToFind = typeToFind;
        this.positionToMoveTo = positionToMoveTo;
    }

    public Vector3Int PositionToMoveTo => positionToMoveTo;

    public Type TypeToFind => typeToFind;

    public bool Equals(MoveItemRequest other)
    {
        // Moveitemrequests are completely unique
        // Two requests to move the same item from the same place 
        // should both be posted in the requestpool
        // without being afraid of being ignored due to duplication  
        return Guid == other.Guid;
    }

    public override void Finish()
    {
        Cancel();
    }

    public override int GetHashCode()
    {
        return Guid.GetHashCode();
    }
}
