using System;
using UnityEngine;

namespace GameGrid
{
    [Serializable]
    public class GroundPointBuilder
    {
        public int Index;
        public Vector2 Position;

        public GroundPointBuilder(GroundPoint point)
            :this(point.Index, point.Position)
        { }
        public GroundPointBuilder(int index, Vector2 position)
        {
            Index = index;
            Position = position;
        }
    }

}