using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

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
        private readonly VoxelCell[] voxels;
        public IReadOnlyList<VoxelCell> Voxels { get { return voxels; } }

        public GroundPoint(MainGrid grid, int index, Vector2 initialPosition)
        {
            this.grid = grid;
            Index = index;
            Position = initialPosition;
            voxels = GetVoxels();
        }

        private VoxelCell[] GetVoxels()
        {
            VoxelCell[] ret = new VoxelCell[grid.VoxelHeight];
            for (int i = 0; i < grid.VoxelHeight; i++)
            {
                ret[i] = new VoxelCell(grid, this, i);
            }
            return ret;
        }

        public override string ToString()
        {
            return Index + ":(" + Position.x + "," + Position.y + ")";
        }
    }

}