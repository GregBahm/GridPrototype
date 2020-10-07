using System;
using UnityEngine;

namespace GameGrid
{
    [Serializable]
    public class GridEdgeBuilder
    {
        public int PointAIndex;
        public int PointBIndex;

        public GridEdgeBuilder(GridEdge edge)
            :this(edge.PointA.Index, edge.PointB.Index)
        { }

        public GridEdgeBuilder(int pointAIndex, int pointBIndex)
        {
            PointAIndex = pointAIndex;
            PointBIndex = pointBIndex;
        }
    }

}