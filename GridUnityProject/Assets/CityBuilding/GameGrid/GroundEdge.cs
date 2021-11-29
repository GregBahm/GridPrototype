using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameGrid
{
    public class GroundEdge
    {
        private readonly MainGrid grid;

        public GroundPoint PointA { get; }
        public GroundPoint PointB { get; }

        public Vector2 MidPoint { get { return (PointA.Position + PointB.Position) / 2; } }

        public IEnumerable<GroundQuad> Quads { get { return grid.GetConnectedQuads(this); } }

        public bool IsBorder { get { return grid.GetIsBorder(this); } }

        public GroundEdge(MainGrid grid, GroundPoint pointA, GroundPoint pointB)
        {
            this.grid = grid;
            if(pointA.Index == pointB.Index)
            {
                throw new ArgumentException("Can't make an edge out of two points with the same ID");
            }
            PointA = pointA.Index < pointB.Index ? pointA : pointB;
            PointB = pointA.Index < pointB.Index ? pointB : pointA;
        }

        public GroundPoint GetOtherPoint(GroundPoint point)
        {
            return PointA == point ? PointB : PointA;
        }

        public override string ToString()
        {
            return PointA.ToString() + " -> " + PointB.ToString();
        }
    }

}