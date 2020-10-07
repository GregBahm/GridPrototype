using System;

namespace GameGrid
{
    [Serializable]
    public class GridEdgeBuilder
    {
        public int PointAId;
        public int PointBId;

        public GridEdgeBuilder(GridEdge edge)
        {
            PointAId = edge.PointA.Id;
            PointBId = edge.PointB.Id;
        }

        public GridEdgeBuilder(int pointAId, int pointBId)
        {
            PointAId = pointAId;
            PointBId = pointBId;
        }
    }

}