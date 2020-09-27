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

        public IEnumerable<TessalationPolygon> Polygons { get; }

        public TesselatedGrid(BaseGrid baseGrid)
        {
            Dictionary<BasePoint, TessalationPoint> pointTable = GetPointTable(baseGrid.Points);
            Dictionary<BaseEdge, TessalationBaseEdge> edgeTable = GetEdgeTable(pointTable, baseGrid.CulledEdges);
            Polygons = GetPolygons(edgeTable, baseGrid.Polygons).ToArray();

            Edges = GetAllTheEdges();
            Points = GetAllThePoints();
        }

        private Dictionary<BaseEdge, TessalationBaseEdge> GetEdgeTable(Dictionary<BasePoint, TessalationPoint> pointTable, IEnumerable<BaseEdge> culledConnections)
        {
            Dictionary<BaseEdge, TessalationBaseEdge> ret = new Dictionary<BaseEdge, TessalationBaseEdge>();
            foreach (BaseEdge edge in culledConnections)
            {
                TessalationBaseEdge newEdge = new TessalationBaseEdge(pointTable[edge.PointA], pointTable[edge.PointB]);
                ret.Add(edge, newEdge);
            }
            return ret;
        }

        private IEnumerable<TessalationEdge> GetAllTheEdges()
        {
            HashSet<TessalationEdge> ret = new HashSet<TessalationEdge>();
            foreach (TessalationPolygon polygon in Polygons)
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

        private IEnumerable<TessalationPolygon> GetPolygons(Dictionary<BaseEdge, TessalationBaseEdge> edgeTable, IEnumerable<IPolygon> polygons)
        {
            foreach(IPolygon polygon in polygons)
            {
                yield return new TessalationPolygon(polygon.Edges.Select(item => edgeTable[item]));
            }
        }

        private Dictionary<BasePoint, TessalationPoint> GetPointTable(BasePoint[,] points)
        {
            Dictionary<BasePoint, TessalationPoint> ret = new Dictionary<BasePoint, TessalationPoint>();
            foreach (BasePoint point in points)
            {
                ret.Add(point, new TessalationPoint(new Vector2(point.PosX, point.PosY), point.IsBorder));
            }
            return ret;
        }
    }
}