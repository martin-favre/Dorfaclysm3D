using UnityEngine;

namespace Items
{
    [System.Serializable]
    public class RockBlockItem : Item
    {
        public object Clone()
        {
            return new RockBlockItem();
        }

        public string GetDescription()
        {
            return "A block of rock";
        }

        public string GetName()
        {
            return "Rock Block";
        }

        public uint GetStackSize()
        {
            return 100;
        }

        public Vector2 GetTexturePosition()
        {
            return TexturePositions.RockBlock;
        }

        public int GetValue()
        {
            return 1;
        }
    }
}