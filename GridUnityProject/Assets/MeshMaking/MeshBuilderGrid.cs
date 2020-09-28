using System.Collections.Generic;
using UnityEngine;
using GridMaking;
using System.Linq;

namespace MeshBuilding
{
    public class MeshBuilderGrid
    {
        private GridMaker gridMaker;

        private readonly IMeshBuilderVert[] verticesSource;
        public int[] Triangles { get; }

        private IEnumerable<MeshBuilderPoly> polys;

        public IEnumerable<Vector3> Vertices
        {
            get
            {
                return verticesSource.Select(item => item.VertPos);
            }
        }

        public MeshBuilderGrid(GridMaker gridMaker)
        {
            this.gridMaker = gridMaker;
            Dictionary<EasedPoint, MeshBuilderAnchorPoint> anchorTable = GetAnchorTable();
            Dictionary<string, MeshBuilderEdge> edgeTable = GetEdgeTable(anchorTable);
            polys = GetPolys(anchorTable, edgeTable);

            verticesSource = GetVertices(anchorTable.Values, edgeTable.Values);
            Triangles = GetTriangles();
        }

        private int[] GetTriangles()
        {
            List<int> ret = new List<int>();
            foreach (MeshBuilderPoly poly in polys)
            {
                ret.AddRange(poly.Triangles);
            }
            return ret.ToArray();
        }

        private IMeshBuilderVert[] GetVertices(IEnumerable<MeshBuilderAnchorPoint> anchors, IEnumerable<MeshBuilderEdge> edges)
        {
            int vertCount = anchors.Count() + edges.Count() + polys.Count();
            IMeshBuilderVert[] ret = new IMeshBuilderVert[vertCount];
            foreach (MeshBuilderAnchorPoint point in anchors)
            {
                ret[point.Index] = point;
            }
            foreach (MeshBuilderEdge edge in edges)
            {
                ret[edge.CenterIndex] = edge;
            }
            foreach (MeshBuilderPoly poly in polys)
            {
                ret[poly.CenterIndex] = poly;
            }
            return ret;
        }

        private Dictionary<EasedPoint, MeshBuilderAnchorPoint> GetAnchorTable()
        {
            int index = 0;
            Dictionary<EasedPoint, MeshBuilderAnchorPoint> ret = new Dictionary<EasedPoint, MeshBuilderAnchorPoint>();
            foreach (EasedPoint point in gridMaker.Points)
            {
                MeshBuilderAnchorPoint anchor = new MeshBuilderAnchorPoint(point, index);
                ret.Add(point, anchor);
                index++;
            }
            return ret;
        }

        private Dictionary<string, MeshBuilderEdge> GetEdgeTable(Dictionary<EasedPoint, MeshBuilderAnchorPoint> anchorTable)
        {
            int index = anchorTable.Count;
            Dictionary<string, MeshBuilderEdge> ret = new Dictionary<string, MeshBuilderEdge>();
            foreach (EasedEdge edge in gridMaker.Edges)
            {
                MeshBuilderEdge newEdge = new MeshBuilderEdge(anchorTable[edge.PointA], anchorTable[edge.PointB], index);
                ret.Add(newEdge.Key, newEdge);
                index++;
            }
            return ret;
        }

        private IEnumerable<MeshBuilderPoly> GetPolys(Dictionary<EasedPoint, MeshBuilderAnchorPoint> anchorTable, Dictionary<string, MeshBuilderEdge> edgeTable)
        {
            int index = anchorTable.Count + edgeTable.Count;
            List<MeshBuilderPoly> ret = new List<MeshBuilderPoly>();
            foreach (EasedQuad quad in gridMaker.Quads)
            {
                MeshBuilderAnchorPoint pointA = anchorTable[quad.Points[0]];
                MeshBuilderAnchorPoint pointB = anchorTable[quad.Points[1]];
                MeshBuilderAnchorPoint pointC = anchorTable[quad.Points[2]];
                MeshBuilderAnchorPoint pointD = anchorTable[quad.Points[3]];

                MeshBuilderEdge edgeAB = edgeTable[MeshBuilderEdge.GetEdgeKey(pointA, pointB)];
                MeshBuilderEdge edgeBC = edgeTable[MeshBuilderEdge.GetEdgeKey(pointB, pointC)];
                MeshBuilderEdge edgeCD = edgeTable[MeshBuilderEdge.GetEdgeKey(pointC, pointD)];
                MeshBuilderEdge edgeDA = edgeTable[MeshBuilderEdge.GetEdgeKey(pointD, pointA)];

                MeshBuilderPoly poly = new MeshBuilderPoly(pointA, pointB, pointC, pointD, edgeAB, edgeBC, edgeCD, edgeDA, index);
                ret.Add(poly);
                index++;
            }
            return ret;
        }
    }
}