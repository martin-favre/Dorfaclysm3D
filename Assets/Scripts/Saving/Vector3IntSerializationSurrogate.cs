using System.Runtime.Serialization;
using UnityEngine;

public class Vector3IntSerializationSurrogate : ISerializationSurrogate
{
    public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
    {
        Vector3Int vec = (Vector3Int)obj;
        info.AddValue("x", vec.x);
        info.AddValue("y", vec.y);
        info.AddValue("z", vec.z);
    }

    public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
    {
        Vector3Int vec = (Vector3Int)obj;
        vec.x = (int)info.GetValue("x", typeof(int));
        vec.y = (int)info.GetValue("y", typeof(int));
        vec.z = (int)info.GetValue("z", typeof(int));
        obj = vec;
        return obj;
    }
}