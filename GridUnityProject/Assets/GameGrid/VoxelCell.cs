using GameGrid;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameGrid
{
    public class VoxelCell
    {
        private readonly MainGrid grid;
        public GroundPoint GroundPoint { get; }

        public Vector3 CellPosition { get { return new Vector3(GroundPoint.Position.x, Height, GroundPoint.Position.y); } }

        public int Height { get; }

        public bool Filled
        {
            get { return grid.IsFilled(this); }
            set
            {
                grid.SetCellFilled(this, value);
            }
        }

        public VoxelCell(MainGrid grid, GroundPoint groundPoint, int height)
        {
            this.grid = grid;
            GroundPoint = groundPoint;
            Height = height;
        }

        public override string ToString()
        {
            return "(" + GroundPoint.Index + ", " + Height + ")";
        }
    }
}
