using UnityEngine;

namespace Items
{
    [System.Serializable]
    public class GrassBlockItem : Item
    {
        public object Clone()
        {
            return new GrassBlockItem();
        }

        public string GetDescription()
        {
            return "A block of grass";
        }

        public string GetName()
        {
            return "Grass Block";
        }

        public uint GetStackSize()
        {
            return 100;
        }

        public Vector2 GetTexturePosition()
        {
            return TexturePositions.GrassBlock;
        }

        public int GetValue()
        {
            return 1;
        }
    }
}