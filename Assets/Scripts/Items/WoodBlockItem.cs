using UnityEngine;

namespace Items
{
    [System.Serializable]
    public class WoodBlockItem : Item
    {
        public object Clone()
        {
            return new WoodBlockItem();
        }

        public string GetDescription()
        {
            return "A block of wood";
        }

        public string GetName()
        {
            return "Wood Block";
        }

        public uint GetStackSize()
        {
            return 100;
        }

        public Vector2 GetTexturePosition()
        {
            return TexturePositions.texturePositions[TexturePositions.Name.WoodBlock];
        }

        public int GetValue()
        {
            return 1;
        }
    }
}