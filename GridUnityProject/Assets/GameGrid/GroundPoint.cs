using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

        public GroundPoint(MainGrid grid, int index, Vector2 initialPosition)
        {
            this.grid = grid;
            Index = index;
            Position = initialPosition;
        }

        public override string ToString()
        {
            return Index + ":(" + Position.x + "," + Position.y + ")";
        }
    }

}