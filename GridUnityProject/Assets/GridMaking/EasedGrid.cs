using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GridMaking
{
    class EasedGrid
    {
        public IEnumerable<EasedPoint> EasedPoints { get; }

        public IEnumerable<EasedEdge> EasedEdges { get; }

        public EasedGrid(TesselatedGrid tesselatedGrid)
        {
            Dictionary<TessalationPoint, EasedPoint> pointTable = CreatePointTable(tesselatedGrid.Points);
            EasedPoints = GetPopulatedPoints(pointTable, tesselatedGrid.Edges);
            EasedEdges = tesselatedGrid.Edges.Select(item =>
                new EasedEdge(pointTable[item.PointA], pointTable[item.PointB])
                ).ToArray();
        }

        public void DoEase(float weight, float borderWeight)
        {
            foreach (EasedPoint point in EasedPoints)
            {
                Vector2 sumPos = Vector2.zero;
                foreach (EasedPoint connection in point.ConnectedPoints)
                {
                    sumPos += connection.CurrentPos;
                }
                sumPos /= point.ConnectedPoints.Count;
                float actualWeight = point.IsBorder ? borderWeight : weight;
                point.CurrentPos = Vector2.Lerp(point.BasePos, sumPos, actualWeight);
            }
        }

        private IEnumerable<EasedPoint> GetPopulatedPoints(
            Dictionary<TessalationPoint, EasedPoint> pointTable,
            IEnumerable<TessalationEdge> culledConnections)
        {
            foreach (TessalationEdge edge in culledConnections)
            {
                EasedPoint pointA = pointTable[edge.PointA];
                EasedPoint pointB = pointTable[edge.PointB];
                pointA.ConnectedPoints.Add(pointB);
                pointB.ConnectedPoints.Add(pointA);
            }
            return pointTable.Values;
        }

        private Dictionary<TessalationPoint, EasedPoint> CreatePointTable(IEnumerable<TessalationPoint> points)
        {
            Dictionary<TessalationPoint, EasedPoint> ret = new Dictionary<TessalationPoint, EasedPoint>();
            foreach (TessalationPoint item in points)
            {
                ret.Add(item, new EasedPoint(item.Pos, item.IsBorder));
            }
            return ret;
        }
    }
}