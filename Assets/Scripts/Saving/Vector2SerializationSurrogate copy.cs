using System.Runtime.Serialization;
using UnityEngine;

public class Vector2SerializationSurrogate : ISerializationSurrogate
{
    public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
    {
        Vector2 vec = (Vector2)obj;
        info.AddValue("x", vec.x);
        info.AddValue("y", vec.y);
    }

    public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
    {
        Vector2 vec = (Vector2)obj;
        vec.x = (float)info.GetValue("x", typeof(float));
        vec.y = (float)info.GetValue("y", typeof(float));
        obj = vec;
        return obj;
    }
}