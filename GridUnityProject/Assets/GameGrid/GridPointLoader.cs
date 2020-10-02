using System;
using UnityEngine;

namespace GameGrid
{
    [Serializable]
    public class GridPointLoader
    {
        public int Id;
        public Vector2 Pos;

        public GridPointLoader(GridPoint point)
        {
            Id = point.Id;
            Pos = point.Position;
        }

        public GridPoint ToPoint(MainGrid grid)
        {
            return new GridPoint(grid, Id, Pos);
        }
    }

}