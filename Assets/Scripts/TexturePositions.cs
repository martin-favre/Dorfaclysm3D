using System.Collections.Generic;
using UnityEngine;

public static class TexturePositions
{
    public enum Name
    {
        RockBlock,
        GrassBlock,
        StairUpDownBlock,
    }
    public static readonly Dictionary<Name, Vector2> texturePositions = new Dictionary<Name, Vector2>(){
        {Name.RockBlock, new Vector2(1,1)},
         {Name.GrassBlock, new Vector2(0,1)},
         {Name.StairUpDownBlock, new Vector2(1,0)},
    };

    public static Vector2 Get(Name ident)
    {
        return texturePositions[ident];
    }
}
