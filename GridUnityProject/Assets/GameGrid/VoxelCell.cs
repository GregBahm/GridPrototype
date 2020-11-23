using GameGrid;
using System;
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

        public VoxelCell CellBelow
        {
            get
            {
                if (Height == 0) return null;
                return GroundPoint.Voxels[Height - 1];
            }
        }

        public VoxelCell CellAbove
        {
            get
            {
                if (Height == MainGrid.VoxelHeight - 1) return null;
                return GroundPoint.Voxels[Height + 1];
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

        internal IEnumerable<VoxelCell> GetConnectedCells()
        {
            foreach(GroundPoint point in GroundPoint.DirectConnections.Concat(GroundPoint.DiagonalConnections))
            {
                VoxelCell cell = point.Voxels[Height];
                yield return cell;
                if (cell.CellBelow != null)
                {
                    yield return cell.CellBelow;
                }
                if(cell.CellAbove != null)
                {
                    yield return cell.CellAbove;
                }
            }
        }
    }
}
