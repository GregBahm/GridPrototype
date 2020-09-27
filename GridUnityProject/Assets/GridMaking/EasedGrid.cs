using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GridMaking
{
    class EasedGrid
    {
        public IEnumerable<EasedPoint> EasedPoints { get; }

        public IEnumerable<EasedEdge> EasedEdges { get; }

        private readonly Vector2 centerPoint;

        public EasedGrid(TesselatedGrid tesselatedGrid, Vector2 centerPoint)
        {
            this.centerPoint = centerPoint;
            Dictionary<TessalationPoint, EasedPoint> pointTable = CreatePointTable(tesselatedGrid.Points);
            EasedPoints = GetPopulatedPoints(pointTable, tesselatedGrid.Edges);
            EasedEdges = tesselatedGrid.Edges.Select(item =>
                new EasedEdge(pointTable[item.PointA], pointTable[item.PointB])
                ).ToArray();
        }

        public void DoEase(float weight, float borderWeight, float hexSize)
        {
            foreach (EasedPoint point in EasedPoints)
            {
                if(point.IsBorder)
                {
                    DoBorderEase(point, borderWeight, hexSize);
                }
                else
                {
                    DoEasePoint(point, weight);
                }
            }
        }

        private void DoEasePoint(EasedPoint point, float weight)
        {
            Vector2 sumPos = Vector2.zero;
            foreach (EasedPoint connection in point.ConnectedPoints)
            {
                sumPos += connection.CurrentPos;
            }
            sumPos /= point.ConnectedPoints.Count;
            point.CurrentPos = Vector2.Lerp(point.BasePos, sumPos, weight);
        }

        private void DoBorderEase(EasedPoint point, float borderWeight, float hexSize)
        {
            Vector2 offset = (point.BasePos - centerPoint).normalized * hexSize / 2.1f;
            Vector2 roundTarget = centerPoint + offset;
            point.CurrentPos = Vector2.Lerp(point.BasePos, roundTarget, borderWeight);
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