using UnityEngine;

namespace Items
{
    public interface Item : System.ICloneable
    {
        string GetName();
        string GetDescription();
        int GetValue();
        uint GetStackSize();
        Vector2 GetTexturePosition();
    }
}