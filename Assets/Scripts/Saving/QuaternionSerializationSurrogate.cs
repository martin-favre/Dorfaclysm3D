using System.Runtime.Serialization;
using UnityEngine;

public class QuaternionSerializationSurrogate : ISerializationSurrogate
{
    public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
    {
        Quaternion vec = (Quaternion)obj;
        info.AddValue("x", vec.x);
        info.AddValue("y", vec.y);
        info.AddValue("z", vec.z);
        info.AddValue("w", vec.w);
    }

    public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
    {
        Quaternion vec = (Quaternion)obj;
        vec.x = (float)info.GetValue("x", typeof(float));
        vec.y = (float)info.GetValue("y", typeof(float));
        vec.z = (float)info.GetValue("z", typeof(float));
        vec.w = (float)info.GetValue("w", typeof(float));
        obj = vec;
        return obj;
    }
}