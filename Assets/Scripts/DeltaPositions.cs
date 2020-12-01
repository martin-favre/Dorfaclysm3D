using UnityEngine;
public class DeltaPositions
{

    public static readonly Vector3Int[] DeltaPositions3D = {
        new Vector3Int(0, 0, -1),
        new Vector3Int(0, -1, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(1, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, 0, 1)
    };
    public static readonly Vector3Int[] DeltaPositionsHorizontal = {
        new Vector3Int(0, 0, -1),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(1, 0, 0),
        new Vector3Int(0, 0, 1)
    };

}