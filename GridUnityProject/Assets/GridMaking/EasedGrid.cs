using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GridMaking
{
    class EasedGrid
    {
        public IEnumerable<EasedPoint> Points { get; }

        public IEnumerable<EasedEdge> Edges { get; }

        public IEnumerable<EasedQuad> Quads { get; }

        private readonly Vector2 centerPoint;

        public EasedGrid(TesselatedGrid tessalatedGrid, Vector2 centerPoint)
        {
            this.centerPoint = centerPoint;
            Dictionary<TessalationPoint, EasedPoint> pointTable = CreatePointTable(tessalatedGrid.Points);
            Points = GetPopulatedPoints(pointTable, tessalatedGrid.Edges);
            Edges = GetEdges(tessalatedGrid, pointTable);
            Quads = GetQuads(tessalatedGrid, pointTable);
        }

        private IEnumerable<EasedEdge> GetEdges(TesselatedGrid tesselatedGrid, Dictionary<TessalationPoint, EasedPoint> pointTable)
        {
            return tesselatedGrid.Edges.Select(item =>
                new EasedEdge(pointTable[item.PointA], pointTable[item.PointB])
                ).ToArray();
        }

        private IEnumerable<EasedQuad> GetQuads(TesselatedGrid tessalatedGrid, Dictionary<TessalationPoint, EasedPoint> pointTable)
        {
            List<EasedQuad> ret = new List<EasedQuad>();
            foreach (TessalationPolygon polygon in tessalatedGrid.Polygons)
            {
                IEnumerable<EasedQuad> quadsOfThePolygon = GetQuadsOfThePolygon(polygon, pointTable);
                ret.AddRange(quadsOfThePolygon);
            }
            return ret;
        }

        private IEnumerable<EasedQuad> GetQuadsOfThePolygon(TessalationPolygon polygon, Dictionary<TessalationPoint, EasedPoint> pointTable)
        {
            foreach (TessalationPoint point in polygon.BasePoints)
            {
                TessalationBaseEdge[] edges = polygon.BaseEdges.Where(edge => edge.PointA == point || edge.PointB == point).ToArray();
                TessalationPoint[] points = new TessalationPoint[]
                {
                    point,
                    edges[0].SubPoint,
                    edges[1].SubPoint,
                    polygon.SubPoint
                };
                yield return new EasedQuad(points.Select(item => pointTable[item]));
            }
        }

        private TessalationEdge GetSubEdge(TessalationPoint point, TessalationBaseEdge tessalationBaseEdge)
        {
            throw new NotImplementedException();
        }

        public void DoEase(float weight, float borderWeight, float hexSize, float targetCellLength)
        {
            foreach (EasedPoint point in Points)
            {
                if(point.IsBorder)
                {
                    DoBorderEase(point, borderWeight, hexSize);
                }
                else
                {
                    DoEasePointB(point, weight, targetCellLength);
                }
            }
        }

        // This one tries to move so that all of it's connections are the target length
        private void DoEasePointB(EasedPoint point, float weight, float targetCellLength)
        {
            Vector2 normalAverage = Vector2.zero;
            foreach (EasedPoint connection in point.ConnectedPoints)
            {
                Vector2 diff = point.CurrentPos - connection.CurrentPos;
                Vector2 diffNormal = diff.normalized * targetCellLength;
                Vector2 targetPos = connection.CurrentPos + diffNormal;
                normalAverage += targetPos;
            }
            normalAverage /= point.ConnectedPoints.Count;
            point.CurrentPos = Vector2.Lerp(point.BasePos, normalAverage, weight);
        }

        // This one tries to move the point to the average position of it's connections
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