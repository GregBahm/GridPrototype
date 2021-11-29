using System;
using UnityEngine;

namespace GameGrid
{
    [Serializable]
    public class GroundEdgeBuilder
    {
        public int PointAIndex;
        public int PointBIndex;

        public GroundEdgeBuilder(GroundEdge edge)
            :this(edge.PointA.Index, edge.PointB.Index)
        { }

        public GroundEdgeBuilder(int pointAIndex, int pointBIndex)
        {
            PointAIndex = pointAIndex;
            PointBIndex = pointBIndex;
        }
    }

}