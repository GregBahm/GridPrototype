using System;

namespace GameGrid
{
    [Serializable]
    public class GridEdgeLoader
    {
        public int PointAId;
        public int PointBIds;

        public GridEdgeLoader(GridEdge edge)
        {
            PointAId = edge.PointA.Id;
            PointBIds = edge.PointB.Id;
        }
    }

}