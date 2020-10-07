using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameGrid
{
    public class GridEdge
    {
        private readonly MainGrid grid;

        public GridPoint PointA { get; }
        public GridPoint PointB { get; }

        public Vector2 MidPoint { get { return (PointA.Position + PointB.Position) / 2; } }

        public IEnumerable<GridQuad> Quads { get { return grid.GetConnectedQuads(this); } }

        public bool IsBorder { get { return grid.GetIsBorder(this); } }

        public GridEdge(MainGrid grid, GridPoint pointA, GridPoint pointB)
        {
            this.grid = grid;
            if(pointA.Index == pointB.Index)
            {
                throw new ArgumentException("Can't make an edge out of two points with the same ID");
            }
            PointA = pointA.Index < pointB.Index ? pointA : pointB;
            PointB = pointA.Index < pointB.Index ? pointB : pointA;
        }

        public GridPoint GetOtherPoint(GridPoint point)
        {
            return PointA == point ? PointB : PointA;
        }

        public override string ToString()
        {
            return PointA.ToString() + " -> " + PointB.ToString();
        }
    }

}