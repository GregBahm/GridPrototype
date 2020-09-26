using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GridMaking
{
    class BaseGrid
    {
        public TrianglePoint[,] Points { get; }
        public IEnumerable<TriangleEdge> BaseConnections { get; }
        public IEnumerable<TriangleEdge> CulledConnections { get; }

        private HashSet<IPolygon> polygons = new HashSet<IPolygon>();
        public IEnumerable<IPolygon> Polygons { get { return polygons; } }

        private int gridRows;
        private int gridColumns;

        public BaseGrid(int rows, int columns)
        {
            gridRows = rows;
            gridColumns = columns;
            Points = CreateTrianglePoints();
            BaseConnections = CreateConnectins(Points);
            CulledConnections = DestroyConnections(BaseConnections);
        }

        private TrianglePoint[,] CreateTrianglePoints()
        {
            TrianglePoint[,] ret = new TrianglePoint[gridRows, gridColumns];
            for (int x = 0; x < gridRows; x++)
            {
                for (int y = 0; y < gridColumns; y++)
                {
                    bool isBorder = x == 0 || x == gridRows - 1 || y == 0 || y == gridColumns - 1;
                    ret[x, y] = new TrianglePoint(x, y, isBorder);
                }
            }
            return ret;
        }

        private void PopulateTriangles(Dictionary<string, TriangleEdge> baseEdges, TrianglePoint[,] points)
        {
            for (int x = 0; x < gridRows - 1; x++)
            {
                for (int y = 0; y < gridColumns - 1; y++)
                {
                    TrianglePoint pointA = points[x, y];
                    TrianglePoint pointB = points[x, y + 1];
                    TrianglePoint pointC = points[x + 1, y];
                    TrianglePoint pointD = points[x + 1, y + 1];
                    TriangleEdge edgeAB = baseEdges[TriangleEdge.GetKey(pointA, pointB)];
                    TriangleEdge edgeAC = baseEdges[TriangleEdge.GetKey(pointA, pointC)];
                    TriangleEdge edgeBC = baseEdges[TriangleEdge.GetKey(pointB, pointC)];
                    TriangleEdge edgeCD = baseEdges[TriangleEdge.GetKey(pointC, pointD)];
                    TriangleEdge edgeBD = baseEdges[TriangleEdge.GetKey(pointB, pointD)];

                    BaseTriangle triangleA = new BaseTriangle(edgeAB, edgeBC, edgeAC);
                    BaseTriangle triangleB = new BaseTriangle(edgeBC, edgeCD, edgeBD);

                    polygons.Add(triangleA); // damn side effects
                    polygons.Add(triangleB);

                    edgeAB.Triangles[0] = triangleA;
                    edgeAC.Triangles[0] = triangleA;
                    edgeBC.Triangles[0] = triangleA;
                    edgeBD.Triangles[1] = triangleB;
                    edgeBC.Triangles[1] = triangleB;
                    edgeCD.Triangles[1] = triangleB;
                }
            }
        }

        private IEnumerable<TriangleEdge> DestroyConnections(IEnumerable<TriangleEdge> connections)
        {
            IEnumerable<TriangleEdge> interiorEdges = connections.Where(item => item.Triangles.All(tri => tri != null));
            HashSet<TriangleEdge> edgesThatCanBeDestroyed = new HashSet<TriangleEdge>(interiorEdges);
            while (edgesThatCanBeDestroyed.Any())
            {
                int indexToDelete = Mathf.FloorToInt(edgesThatCanBeDestroyed.Count * UnityEngine.Random.value);
                TriangleEdge edgeToDelete = edgesThatCanBeDestroyed.ElementAt(indexToDelete);

                edgeToDelete.Delete = true;
                edgesThatCanBeDestroyed.Remove(edgeToDelete);
                IEnumerable<TriangleEdge> edgesThatCantBeDestroyed = edgeToDelete.Triangles.SelectMany(item => item.Edges);
                foreach (var item in edgesThatCantBeDestroyed)
                {
                    edgesThatCanBeDestroyed.Remove(item);
                }
                UpdatePolygons(edgeToDelete);
            }
            return connections.Where(item => !item.Delete).ToArray();
        }

        private void UpdatePolygons(TriangleEdge edgeToDelete)
        {
            BaseTriangle triA = edgeToDelete.Triangles[0];
            BaseTriangle triB = edgeToDelete.Triangles[1];
            polygons.Remove(triA);
            polygons.Remove(triB);

            List<TriangleEdge> edges = new List<TriangleEdge>();
            edges.AddRange(triA.Edges);
            edges.AddRange(triB.Edges);
            HashSet<TriangleEdge> hash = new HashSet<TriangleEdge>(edges);
            hash.Remove(edgeToDelete);
            BaseQuad quad = new BaseQuad(hash);
            polygons.Add(quad);
        }

        private IEnumerable<TriangleEdge> CreateConnectins(TrianglePoint[,] points)
        {
            Dictionary<string, TriangleEdge> baseEdges = new Dictionary<string, TriangleEdge>();
            for (int x = 0; x < gridRows - 1; x++)
            {
                for (int y = 0; y < gridColumns - 1; y++)
                {
                    TrianglePoint pointA = points[x, y];
                    TrianglePoint pointB = points[x, y + 1];
                    TrianglePoint pointC = points[x + 1, y];
                    TrianglePoint pointD = points[x + 1, y + 1];

                    TriangleEdge edgeAB = new TriangleEdge(pointA, pointB);
                    TriangleEdge edgeAC = new TriangleEdge(pointA, pointC);
                    TriangleEdge edgeBC = new TriangleEdge(pointB, pointC);
                    baseEdges.Add(edgeAB.Key, edgeAB);
                    baseEdges.Add(edgeAC.Key, edgeAC);
                    baseEdges.Add(edgeBC.Key, edgeBC);

                    if (x == gridRows - 2)
                    {
                        TriangleEdge edgeCD = new TriangleEdge(pointC, pointD);
                        baseEdges.Add(edgeCD.Key, edgeCD);
                    }
                    if (y == gridColumns - 2)
                    {
                        TriangleEdge edgeBD = new TriangleEdge(pointB, pointD);
                        baseEdges.Add(edgeBD.Key, edgeBD);
                    }
                }
            }
            PopulateTriangles(baseEdges, points);
            return baseEdges.Values;
        }
    }
}