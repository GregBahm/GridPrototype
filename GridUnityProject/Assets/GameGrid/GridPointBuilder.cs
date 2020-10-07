using System;
using UnityEngine;

namespace GameGrid
{
    [Serializable]
    public class GridPointBuilder
    {
        public int Index;
        public Vector2 Position;

        public GridPointBuilder(GridPoint point)
            :this(point.Index, point.Position)
        { }
        public GridPointBuilder(int index, Vector2 position)
        {
            Index = index;
            Position = position;
        }
    }

}