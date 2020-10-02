using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace GameGrid
{
    public class GridPoint
    {
        private readonly MainGrid grid;

        public int Id { get; }
        public Vector2 Position { get; set; }
        public IEnumerable<GridEdge> Edges { get { return grid.GetEdges(this); } }
        public IEnumerable<GridPoint> DirectConnections { get { return Edges.Select(item => item.GetOtherPoint(this)); } }
        public IEnumerable<GridPoint> DiagonalConnections { get { return PolyConnections.Select(item => item.GetDiagonalPoint(this)); } }
        public IEnumerable<GridQuad> PolyConnections { get { return grid.GetConnectedQuads(this); } }

        public GridPoint(MainGrid grid, int id, Vector2 initialPosition)
        {
            this.grid = grid;
            Id = id;
            Position = initialPosition;
        }

        public override string ToString()
        {
            return Id + ":(" + Position.x + "," + Position.y + ")";
        }
    }

}