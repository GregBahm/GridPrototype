using System;
using UnityEngine;

[Serializable]
public class ItemBlueprint
{
    public Texture2D Texture;
    public ConnectionType Up;
    public ConnectionType Down;
    public ConnectionType Left;
    public ConnectionType Right;

    public override string ToString()
    {
        return Texture.name;
    }
}
