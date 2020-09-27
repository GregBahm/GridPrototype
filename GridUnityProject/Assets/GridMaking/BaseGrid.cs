using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GridMaking
{
    class BaseGrid
    {
        public BasePoint[,] Points { get; }

        public IEnumerable<BaseEdge> BaseEdges { get; }
        public IEnumerable<BaseEdge> HexEdges { get; }
        public IEnumerable<BaseEdge> CulledEdges { get; }

        private HashSet<IPolygon> polygons;
        public IEnumerable<IPolygon> Polygons { get { return polygons; } }

        private int gridSize;

        public BaseGrid(int gridSize)
        {
            this.gridSize = gridSize;
            Points = CreateBasePoints();
            BaseEdges = CreateConnections();
            HexEdges = GetHexEdges();
            polygons = GatherPolygons();
            CulledEdges = DestroyConnections();
        }

        private HashSet<IPolygon> GatherPolygons()
        {
            HashSet<IPolygon> ret = new HashSet<IPolygon>();
            foreach (BaseEdge edge in BaseEdges)
            {
                foreach (BaseTriangle triangle in edge.Triangles.Where(item => item != null))
                {
                    if(triangle.Points.All(item => item.IsWithinHex))
                    {
                        ret.Add(triangle);
                    }
                }
            }
            return ret;
        }

        private BasePoint[,] CreateBasePoints()
        {
            BasePoint[,] ret = new BasePoint[gridSize, gridSize];
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    bool isWithinHex = GetIsWithinHex(x, y);
                    bool isBorder = GetIsBorder(x, y);
                    ret[x, y] = new BasePoint(x, y, isBorder, isWithinHex);
                }
            }
            return ret;
        }

        private bool GetIsBorder(int x, int y)
        {
            if(x == 0 || x == gridSize - 1)
            {
                return true;
            }
            if(y == 0 || y == gridSize - 1)
            {
                return true;
            }
            int halfSize = gridSize / 2;
            int sum = x + y;
            return sum == halfSize || sum == halfSize * 3;
        }

        private bool GetIsWithinHex(int x, int y)
        {
            int halfSize = gridSize / 2;
            int sum = x + y;
            return sum > halfSize - 1 && sum < (halfSize * 3) + 1;
        }

        private IEnumerable<BaseEdge> GetHexEdges()
        {
            return BaseEdges.Where(item => item.PointA.IsWithinHex && item.PointB.IsWithinHex).ToArray();
        }

        private void PopulateTriangles(Dictionary<string, BaseEdge> baseEdges)
        {
            for (int x = 0; x < gridSize - 1; x++)
            {
                for (int y = 0; y < gridSize - 1; y++)
                {
                    BasePoint pointA = Points[x, y];
                    BasePoint pointB = Points[x, y + 1];
                    BasePoint pointC = Points[x + 1, y];
                    BasePoint pointD = Points[x + 1, y + 1];
                    BaseEdge edgeAB = baseEdges[BaseEdge.GetKey(pointA, pointB)];
                    BaseEdge edgeAC = baseEdges[BaseEdge.GetKey(pointA, pointC)];
                    BaseEdge edgeBC = baseEdges[BaseEdge.GetKey(pointB, pointC)];
                    BaseEdge edgeCD = baseEdges[BaseEdge.GetKey(pointC, pointD)];
                    BaseEdge edgeBD = baseEdges[BaseEdge.GetKey(pointB, pointD)];

                    BaseTriangle triangleA = new BaseTriangle(edgeAB, edgeBC, edgeAC);
                    BaseTriangle triangleB = new BaseTriangle(edgeBC, edgeCD, edgeBD);

                    edgeAB.Triangles[0] = triangleA;
                    edgeAC.Triangles[0] = triangleA;
                    edgeBC.Triangles[0] = triangleA;
                    edgeBD.Triangles[1] = triangleB;
                    edgeBC.Triangles[1] = triangleB;
                    edgeCD.Triangles[1] = triangleB;
                }
            }
        }

        private IEnumerable<BaseEdge> DestroyConnections()
        {
            IEnumerable<BaseEdge> interiorEdges = HexEdges.Where(item => !item.IsBorderEdge);
            HashSet<BaseEdge> edgesThatCanBeDestroyed = new HashSet<BaseEdge>(interiorEdges);
            while (edgesThatCanBeDestroyed.Any())
            {
                int indexToDelete = Mathf.FloorToInt(edgesThatCanBeDestroyed.Count * UnityEngine.Random.value);
                BaseEdge edgeToDelete = edgesThatCanBeDestroyed.ElementAt(indexToDelete);

                edgeToDelete.Delete = true;
                edgesThatCanBeDestroyed.Remove(edgeToDelete);
                IEnumerable<BaseEdge> edgesThatCantBeDestroyed = edgeToDelete.Triangles.SelectMany(item => item.Edges);
                foreach (var item in edgesThatCantBeDestroyed)
                {
                    edgesThatCanBeDestroyed.Remove(item);
                }
                UpdatePolygons(edgeToDelete);
            }
            return HexEdges.Where(item => !item.Delete).ToArray();
        }

        private void UpdatePolygons(BaseEdge edgeToDelete)
        {
            BaseTriangle triA = edgeToDelete.Triangles[0];
            BaseTriangle triB = edgeToDelete.Triangles[1];
            polygons.Remove(triA);
            polygons.Remove(triB);

            List<BaseEdge> edges = new List<BaseEdge>();
            edges.AddRange(triA.Edges);
            edges.AddRange(triB.Edges);
            HashSet<BaseEdge> hash = new HashSet<BaseEdge>(edges);
            hash.Remove(edgeToDelete);
            BaseQuad quad = new BaseQuad(hash);
            polygons.Add(quad);
        }

        private IEnumerable<BaseEdge> CreateConnections()
        {
            Dictionary<string, BaseEdge> baseEdges = new Dictionary<string, BaseEdge>();
            for (int x = 0; x < gridSize - 1; x++)
            {
                for (int y = 0; y < gridSize - 1; y++)
                {
                    BasePoint pointA = Points[x, y];
                    BasePoint pointB = Points[x, y + 1];
                    BasePoint pointC = Points[x + 1, y];
                    BasePoint pointD = Points[x + 1, y + 1];

                    BaseEdge edgeAB = new BaseEdge(pointA, pointB);
                    BaseEdge edgeAC = new BaseEdge(pointA, pointC);
                    BaseEdge edgeBC = new BaseEdge(pointB, pointC);
                    baseEdges.Add(edgeAB.Key, edgeAB);
                    baseEdges.Add(edgeAC.Key, edgeAC);
                    baseEdges.Add(edgeBC.Key, edgeBC);

                    if (x == gridSize - 2)
                    {
                        BaseEdge edgeCD = new BaseEdge(pointC, pointD);
                        baseEdges.Add(edgeCD.Key, edgeCD);
                    }
                    if (y == gridSize - 2)
                    {
                        BaseEdge edgeBD = new BaseEdge(pointB, pointD);
                        baseEdges.Add(edgeBD.Key, edgeBD);
                    }
                }
            }
            PopulateTriangles(baseEdges);
            return baseEdges.Values;
        }
    }
}