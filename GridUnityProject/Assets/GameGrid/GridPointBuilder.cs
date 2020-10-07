using System;
using UnityEngine;

namespace GameGrid
{
    [Serializable]
    public class GridPointBuilder
    {
        public int Id;
        public Vector2 Pos;

        public GridPointBuilder(GridPoint point)
        {
            Id = point.Id;
            Pos = point.Position;
        }
        public GridPointBuilder(int id, Vector2 position)
        {
            Id = id;
            Pos = position;
        }
    }

}