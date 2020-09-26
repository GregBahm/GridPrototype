using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GridMaking
{
    class TesselatedGrid
    {
        public IEnumerable<TessalationPoint> Points { get; }

        public IEnumerable<TessalationEdge> Edges { get; }

        public TesselatedGrid(BaseGrid baseGrid)
        {
            Dictionary<TrianglePoint, TessalationPoint> pointTable = GetPointTable(baseGrid.Points);
            Dictionary<TriangleEdge, TessalationBaseEdge> edgeTable = GetEdgeTable(pointTable, baseGrid.CulledConnections);
            IEnumerable<TessalationPolygon> polys = GetPolygons(edgeTable, baseGrid.Polygons).ToArray();

            Edges = GetAllTheEdges(polys);
            Points = GetAllThePoints();
        }

        private Dictionary<TriangleEdge, TessalationBaseEdge> GetEdgeTable(Dictionary<TrianglePoint, TessalationPoint> pointTable, IEnumerable<TriangleEdge> culledConnections)
        {
            Dictionary<TriangleEdge, TessalationBaseEdge> ret = new Dictionary<TriangleEdge, TessalationBaseEdge>();
            foreach (TriangleEdge edge in culledConnections)
            {
                TessalationBaseEdge newEdge = new TessalationBaseEdge(pointTable[edge.PointA], pointTable[edge.PointB]);
                ret.Add(edge, newEdge);
            }
            return ret;
        }

        private IEnumerable<TessalationEdge> GetAllTheEdges(IEnumerable<TessalationPolygon> polys)
        {
            HashSet<TessalationEdge> ret = new HashSet<TessalationEdge>();
            foreach (TessalationPolygon polygon in polys)
            {
                foreach (TessalationBaseEdge baseEdge in polygon.BaseEdges)
                {
                    ret.Add(baseEdge.SubEdgeA);
                    ret.Add(baseEdge.SubEdgeB);
                }
                foreach (TessalationEdge subEdge in polygon.SubEdges)
                {
                    ret.Add(subEdge);
                }
            }
            return ret;
        }

        private IEnumerable<TessalationPoint> GetAllThePoints()
        {
            HashSet<TessalationPoint> ret = new HashSet<TessalationPoint>();
            foreach (TessalationEdge edge in Edges)
            {
                ret.Add(edge.PointA);
                ret.Add(edge.PointB);
            }
            return ret;
        }

        private IEnumerable<TessalationPolygon> GetPolygons(Dictionary<TriangleEdge, TessalationBaseEdge> edgeTable, IEnumerable<IPolygon> polygons)
        {
            foreach(IPolygon polygon in polygons)
            {
                yield return new TessalationPolygon(polygon.Edges.Select(item => edgeTable[item]));
            }
        }

        private Dictionary<TrianglePoint, TessalationPoint> GetPointTable(TrianglePoint[,] points)
        {
            Dictionary<TrianglePoint, TessalationPoint> ret = new Dictionary<TrianglePoint, TessalationPoint>();
            foreach (TrianglePoint point in points)
            {
                ret.Add(point, new TessalationPoint(new Vector2(point.PosX, point.PosY), point.IsBorder));
            }
            return ret;
        }
    }

    class TessalationBaseEdge
    {
        public TessalationPoint PointA { get; }
        public TessalationPoint PointB { get; }

        public TessalationPoint SubPoint { get; }

        public TessalationEdge SubEdgeA { get; }
        public TessalationEdge SubEdgeB { get; }

        public TessalationBaseEdge(TessalationPoint pointA, TessalationPoint pointB)
        {
            PointA = pointA;
            PointB = pointB;
            SubPoint = new TessalationPoint((pointA.Pos + pointB.Pos) / 2, pointA.IsBorder && pointB.IsBorder);
            SubEdgeA = new TessalationEdge(PointA, SubPoint);
            SubEdgeB = new TessalationEdge(SubPoint, pointB);
        }
    }

    class TessalationEdge
    {
        public TessalationPoint PointA { get; }
        public TessalationPoint PointB { get; }

        public TessalationEdge(TessalationPoint pointA, TessalationPoint pointB)
        {
            PointA = pointA;
            PointB = pointB;
        }
    }

    class TessalationPolygon
    {
        public TessalationBaseEdge[] BaseEdges { get; }
        public TessalationPoint SubPoint { get; }
        public IEnumerable<TessalationEdge> SubEdges { get; }

        public TessalationPolygon(IEnumerable<TessalationBaseEdge> edges)
        {
            BaseEdges = edges.ToArray();
            SubPoint = GetSubPoint();
            SubEdges = CreateSubEdges().ToArray();
        }

        private IEnumerable<TessalationEdge> CreateSubEdges()
        {
            foreach (TessalationBaseEdge edge in BaseEdges)
            {
                yield return new TessalationEdge(edge.SubPoint, SubPoint);
            }
        }

        private TessalationPoint GetSubPoint()
        {
            Vector2 pointPos = Vector2.zero;
            foreach (TessalationBaseEdge edge in BaseEdges)
            {
                pointPos += edge.SubPoint.Pos;
            }
            pointPos /= BaseEdges.Length;
            return new TessalationPoint(pointPos, false);
        }
    }

    class TessalationPoint
    {
        public Vector2 Pos { get; }
        public bool IsBorder { get; }

        public TessalationPoint(Vector2 pos, bool isBorder)
        {
            Pos = pos;
            IsBorder = isBorder;
        }
    }
}