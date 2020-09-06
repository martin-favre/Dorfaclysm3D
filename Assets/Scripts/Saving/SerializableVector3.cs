using UnityEngine;

[System.Serializable]
public class SerializeableVector3
{
    public float x;
    public float y;
    public float z;

    public SerializeableVector3(Vector3 original)
    {
        x = original.x;
        y = original.y;
        z = original.z;
    }

    public Vector3 Get()
    {
        return new Vector3(x, y, z);
    }

}