using UnityEngine;

namespace Items
{
    [System.Serializable]
    public class SnowBlockItem : Item
    {
        public object Clone()
        {
            return new SnowBlockItem();
        }

        public string GetDescription()
        {
            return "A block of snow";
        }

        public string GetName()
        {
            return "Snow Block";
        }

        public uint GetStackSize()
        {
            return 100;
        }

        public Vector2 GetTexturePosition()
        {
            return TexturePositions.SnowBlock;
        }

        public int GetValue()
        {
            return 1;
        }
    }
}