using UnityEngine;

[System.Serializable]
public class SerializeableQuaternion
{
    public float x;
    public float y;
    public float z;

    public float w;

    public SerializeableQuaternion(Quaternion original)
    {
        x = original.x;
        y = original.y;
        z = original.z;
        w = original.w;
    }

    public Quaternion Get()
    {
        return new Quaternion(x, y, z, w);
    }

}