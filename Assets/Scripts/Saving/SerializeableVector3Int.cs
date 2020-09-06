using UnityEngine;

[System.Serializable]
public class SerializeableVector3Int
{
    public int x;
    public int y;
    public int z;

    public SerializeableVector3Int(Vector3Int original)
    {
        x = original.x;
        y = original.y;
        z = original.z;
    }

    public Vector3Int Get()
    {
        return new Vector3Int(x, y, z);
    }

}