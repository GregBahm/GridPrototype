using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using VoxelVisuals;

namespace GameGrid
{
    public class GroundPoint
    {
        private readonly MainGrid grid;

        public int Index { get; }
        public Vector2 Position { get; set; }
        public IEnumerable<GroundEdge> Edges { get { return grid.GetEdges(this); } }
        public IEnumerable<GroundPoint> DirectConnections { get { return Edges.Select(item => item.GetOtherPoint(this)); } }
        public IEnumerable<GroundPoint> DiagonalConnections { get { return PolyConnections.Select(item => item.GetDiagonalPoint(this)); } }
        public IEnumerable<GroundQuad> PolyConnections { get { return grid.GetConnectedQuads(this); } }
        private readonly DesignationCell[] designationCells;
        public IReadOnlyList<DesignationCell> DesignationCells { get { return designationCells; } }
        public bool IsBorder { get { return Edges.Any(item => item.IsBorder); } }

        public GroundPoint(MainGrid grid, int index, Vector2 initialPosition)
        {
            this.grid = grid;
            Index = index;
            Position = initialPosition;
            designationCells = GetDesignationCells();
        }

        private DesignationCell[] GetDesignationCells()
        {
            DesignationCell[] ret = new DesignationCell[grid.MaxHeight];
            for (int i = 0; i < grid.MaxHeight; i++)
            {
                ret[i] = new DesignationCell(grid, this, i);
            }
            return ret;
        }

        public override string ToString()
        {
            return Index + ":(" + Position.x + "," + Position.y + ")";
        }
    }

}