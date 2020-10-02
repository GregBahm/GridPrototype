using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEngine;

namespace GameGrid
{
    [Serializable]
    public class GridLoader
    {
        private const string SaveFilePath = "TheSaveFile";

        public GridPointLoader[] Points;

        internal static MasterGrid LoadDefaultGrid()
        {
            MasterGrid ret = new MasterGrid();
            GridPoint origin = new GridPoint(ret, 0, Vector2.zero);
            List<GridPoint> points = new List<GridPoint>() { origin };
            List<GridEdge> edges = new List<GridEdge>();

            for (int i = 0; i < 6; i++)
            {
                float theta = 60 * i * Mathf.Deg2Rad;
                Vector2 pos = new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));
                GridPoint newPoint = new GridPoint(ret, i + 1, pos);
                points.Add(newPoint);
                edges.Add(new GridEdge(ret, newPoint, origin));
            }
            for (int i = 0; i < 6; i++)
            {
                float theta = (60 * i + 30) * Mathf.Deg2Rad;
                Vector2 pos = new Vector2(Mathf.Cos(theta) * 1.5f, Mathf.Sin(theta) * 1.5f);
                GridPoint newPoint = new GridPoint(ret, i + 7, pos);
                points.Add(newPoint);
            }
            for (int i = 0; i < 6; i++)
            {
                float theta = 60 * i * Mathf.Deg2Rad;
                Vector2 pos = new Vector2(Mathf.Cos(theta) * 2f, Mathf.Sin(theta) * 2);
                GridPoint newPoint = new GridPoint(ret, i + 13, pos);
                points.Add(newPoint);
            }
            for (int i = 0; i < 6; i++)
            {
                int startIndex = i + 1;
                int mid = i + 7;
                int endIndex = ((i + 1) % 6) + 1;

                int outerStart = i + 13;
                int outerEnd = ((i + 1) % 6) + 13;

                GridEdge edgeA = new GridEdge(ret, points[startIndex], points[mid]);
                GridEdge edgeB = new GridEdge(ret, points[mid], points[endIndex]);
                GridEdge edgeC = new GridEdge(ret, points[outerStart], points[mid]);
                GridEdge edgeD = new GridEdge(ret, points[mid], points[outerEnd]);
                edges.Add(edgeA);
                edges.Add(edgeB);
                edges.Add(edgeC);
                edges.Add(edgeD);
            }
            ret.AddToMesh(points, edges);
            return ret;
        }

        public GridEdgeLoader[] Edges;

        public GridLoader(IEnumerable<GridPointLoader> points, IEnumerable<GridEdgeLoader> edges)
        {
            Points = points.ToArray();
            Edges = edges.ToArray();
        }

        public static MasterGrid LoadGrid()
        {
            MasterGrid ret = new MasterGrid();
            string data = PlayerPrefs.GetString(SaveFilePath);
            if(string.IsNullOrWhiteSpace(data))
            {
                Debug.Log("No save data found. Loading default grid");
                return LoadDefaultGrid();
            }
            GridLoader gridLoader = JsonUtility.FromJson<GridLoader>(data);
            Dictionary<int, GridPoint> lookupTable = gridLoader.Points.ToDictionary(item => item.Id, item => new GridPoint(ret, item.Id, item.Pos));
            IEnumerable<GridEdge> edges = gridLoader.Edges.Select(item => new GridEdge(ret, lookupTable[item.PointAId], lookupTable[item.PointBIds])).ToArray();
            ret.AddToMesh(lookupTable.Values, edges);
            return ret;
        }
        public static void SaveGrid(MasterGrid grid)
        {
            GridPointLoader[] points = grid.Points.Select(item => new GridPointLoader(item)).ToArray();
            GridEdgeLoader[] edges = grid.Edges.Select(item => new GridEdgeLoader(item)).ToArray();
            GridLoader loader = new GridLoader(points, edges);
            string asJson = JsonUtility.ToJson(loader);
            PlayerPrefs.SetString(SaveFilePath, asJson);
            PlayerPrefs.Save();
        }
    }

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

    [Serializable]
    public class GridPointLoader
    {
        public int Id;
        public Vector2 Pos;

        public GridPointLoader(GridPoint point)
        {
            Id = point.Id;
            Pos = point.Position;
        }

        public GridPoint ToPoint(MasterGrid grid)
        {
            return new GridPoint(grid, Id, Pos);
        }
    }

    public class MasterGrid
    {
        private List<GridPoint> points = new List<GridPoint>();
        public IEnumerable<GridPoint> Points { get { return points; } }

        private List<GridEdge> edges = new List<GridEdge>();
        public IEnumerable<GridEdge> Edges { get { return edges; } }

        private List<GridQuad> polys = new List<GridQuad>();
        public IEnumerable<GridQuad> Polys { get { return polys; } }

        private readonly Dictionary<GridPoint, List<GridEdge>> edgesTable = new Dictionary<GridPoint, List<GridEdge>>();
        private readonly Dictionary<GridPoint, List<GridQuad>> polyTable = new Dictionary<GridPoint, List<GridQuad>>();
        private readonly Dictionary<GridEdge, List<GridQuad>> bordersTable = new Dictionary<GridEdge, List<GridQuad>>();

        public void AddToMesh(IEnumerable<GridPoint> newPoints, IEnumerable<GridEdge> newEdges)
        {
            AddPoints(newPoints);
            AddEdges(newEdges);
        }

        private void AddPoints(IEnumerable<GridPoint> newPoints)
        {
            points.AddRange(newPoints);
            foreach (GridPoint point in newPoints)
            {
                edgesTable.Add(point, new List<GridEdge>());
                polyTable.Add(point, new List<GridQuad>());
            }
        }

        private void AddEdges(IEnumerable<GridEdge> newEdges)
        {
            HashSet<GridPoint> edgesToSort = new HashSet<GridPoint>();
            edges.AddRange(newEdges);
            foreach (GridEdge edge in newEdges)
            {
                edgesTable[edge.PointA].Add(edge);
                edgesTable[edge.PointB].Add(edge);
                edgesToSort.Add(edge.PointA);
                edgesToSort.Add(edge.PointB);
                bordersTable.Add(edge, new List<GridQuad>());
            }
            foreach (GridPoint point in edgesToSort)
            {
                List<GridEdge> edges = edgesTable[point];
                List<GridEdge> sortedList = edges.OrderByDescending(item => GetSignedAngle(item, point)).ToList();
                edgesTable[point] = sortedList;
            }

            QuadFinder quadFinder = new QuadFinder(this, edges.Where(item => item.IsBorder).ToArray());
            polys.AddRange(quadFinder.Quads);
            foreach (GridQuad quad in quadFinder.Quads)
            {
                foreach (GridEdge edge in quad.Edges)
                {
                    bordersTable[edge].Add(quad);
                }
            }
        }

        private float GetSignedAngle(GridEdge item, GridPoint point)
        {
            GridPoint otherPoint = item.GetOtherPoint(point);
            return Vector2.SignedAngle(Vector2.up, otherPoint.Position - point.Position);
        }

        internal IEnumerable<GridEdge> GetEdges(GridPoint gridPoint)
        {
            return edgesTable[gridPoint];
        }

        internal IEnumerable<GridQuad> GetConnectedQuads(GridPoint gridPoint)
        {
            return polyTable[gridPoint];
        }

        internal bool GetIsBorder(GridEdge gridEdge)
        {
            return bordersTable[gridEdge].Count < 2;
        }

        internal IEnumerable<GridQuad> GetConnectedQuads(GridEdge gridEdge)
        {
            return bordersTable[gridEdge];
        }

        private IEnumerable<PotentialDiagonal> GetPotentialDiagonals(GridEdge edge)
        {
            List<PotentialDiagonal> ret = new List<PotentialDiagonal>();
            ret.AddRange(GetPotentialDiagonals(edge.PointA));
            ret.AddRange(GetPotentialDiagonals(edge.PointB));
            return ret;
        }
        private IEnumerable<PotentialDiagonal> GetPotentialDiagonals(GridPoint point)
        {
            List<GridEdge> edgeList = edgesTable[point];
            for (int i = 0; i < edgeList.Count; i++)
            {
                int nextIndex = (i + 1) % edgeList.Count;
                yield return new PotentialDiagonal(edgeList[i], edgeList[nextIndex], point);
            }
        }

        private class QuadFinder
        {
            private readonly MasterGrid grid;
            private readonly Dictionary<string, PotentialDiagonal> availableDiagonals = new Dictionary<string, PotentialDiagonal>();
            private List<GridQuad> quads = new List<GridQuad>();
            public IEnumerable<GridQuad> Quads { get { return quads; } }

            public QuadFinder(MasterGrid grid, IEnumerable<GridEdge> borderEdges)
            {
                this.grid = grid;
                foreach (GridEdge edge in borderEdges)
                {
                    ProcessEdge(edge);
                }
            }

            private void ProcessEdge(GridEdge edge)
            {
                IEnumerable<PotentialDiagonal> potentialDiagonals = grid.GetPotentialDiagonals(edge);
                foreach (PotentialDiagonal potentialDiagonal in potentialDiagonals)
                {
                    if (availableDiagonals.ContainsKey(potentialDiagonal.Key))
                    {
                        PotentialDiagonal otherHalf = availableDiagonals[potentialDiagonal.Key];
                        if(potentialDiagonal.SharedPoint != otherHalf.SharedPoint)
                        {
                            availableDiagonals.Remove(potentialDiagonal.Key);
                            quads.Add(new GridQuad(potentialDiagonal.EdgeA, potentialDiagonal.EdgeB, otherHalf.EdgeA, otherHalf.EdgeB));
                        }
                    }
                    else
                    {
                        availableDiagonals.Add(potentialDiagonal.Key, potentialDiagonal);
                    }
                }
            }
        }

        private class PotentialDiagonal
        {
            public string Key { get; }
            public GridEdge EdgeA { get; }
            public GridEdge EdgeB { get; }
            public GridPoint SharedPoint { get; }

            public PotentialDiagonal(GridEdge edgeA, GridEdge edgeB, GridPoint sharedPoint)
            {
                EdgeA = edgeA;
                EdgeB = edgeB;
                SharedPoint = sharedPoint;
                GridPoint otherPointA = edgeA.GetOtherPoint(sharedPoint);
                GridPoint otherPointB = edgeB.GetOtherPoint(sharedPoint);
                Key = GetKey(otherPointA.Id, otherPointB.Id);
            }

            private string GetKey(int id1, int id2)
            {
                if(id1 < id2)
                {
                    return id1 + " to " + id2;
                }
                return id2 + " to " + id1;
            }

            public override string ToString()
            {
                return "[" + Key + "] for " + SharedPoint.ToString();
            }
        }
    }

    public class GridEdge
    {
        private readonly MasterGrid grid;

        public GridPoint PointA { get; }
        public GridPoint PointB { get; }

        public Vector2 MidPoint { get { return (PointA.Position + PointB.Position) / 2; } }

        public IEnumerable<GridQuad> Quads { get { return grid.GetConnectedQuads(this); } }

        public bool IsBorder { get { return grid.GetIsBorder(this); } }

        public GridEdge(MasterGrid grid, GridPoint pointA, GridPoint pointB)
        {
            this.grid = grid;
            if(pointA.Id == pointB.Id)
            {
                throw new ArgumentException("Can't make an edge out of two points with the same ID");
            }
            PointA = pointA.Id < pointB.Id ? pointA : pointB;
            PointB = pointA.Id < pointB.Id ? pointB : pointA;
        }

        public GridPoint GetOtherPoint(GridPoint point)
        {
            return PointA == point ? PointB : PointA;
        }

        public override string ToString()
        {
            return PointA.ToString() + " -> " + PointB.ToString();
        }
    }

    public class GridPoint
    {
        private readonly MasterGrid grid;

        public int Id { get; }
        public Vector2 Position { get; set; }
        public IEnumerable<GridEdge> Edges { get { return grid.GetEdges(this); } }
        public IEnumerable<GridPoint> DirectConnections { get { return Edges.Select(item => item.GetOtherPoint(this)); } }
        public IEnumerable<GridPoint> DiagonalConnections { get { return PolyConnections.Select(item => item.GetDiagonalPoint(this)); } }
        public IEnumerable<GridQuad> PolyConnections { get { return grid.GetConnectedQuads(this); } }

        public GridPoint(MasterGrid grid, int id, Vector2 initialPosition)
        {
            this.grid = grid;
            Id = id;
            Position = initialPosition;
        }

        public override string ToString()
        {
            return Id + ":(" + Position.x + "," + Position.y + ")";
        }
    }

    public class GridQuad
    {
        private readonly Dictionary<GridPoint, GridPoint> diagonalsTable;

        private readonly GridPoint[] points;
        public IEnumerable<GridPoint> Points { get { return points; } }
        private readonly GridEdge[] edges;
        public IEnumerable<GridEdge> Edges { get { return edges; } }

        public Vector2 Center { get { return GetCenter(points); } }

        public GridQuad(params GridEdge[] edges)
        {
            if (edges.Length != 4)
            {
                throw new ArgumentException("Can't create a GridQuad with " + this.edges.Length + " edges");
            }
            this.edges = edges;
            this.points = GetPoints();
            diagonalsTable = GetDiagonalsTable();
        }

        private GridPoint[] GetPoints()
        {
            HashSet<GridPoint> rawPoints = new HashSet<GridPoint>
            {
                edges[0].PointA,
                edges[0].PointB,
                edges[1].PointA,
                edges[1].PointB,
                edges[2].PointA,
                edges[2].PointB,
                edges[3].PointA,
                edges[3].PointB,
            };
            if (rawPoints.Count != 4)
            {
                throw new ArgumentException("Can't create a GridQuad with " + rawPoints.Count + " unique points");
            }
            return GetSortedPoints(rawPoints.ToArray());
        }

        public GridPoint GetDiagonalPoint(GridPoint point)
        {
            return diagonalsTable[point];
        }

        private GridPoint[] GetSortedPoints(GridPoint[] rawPoints)
        {
            Vector2 center = GetCenter(rawPoints);
            return rawPoints.OrderByDescending(item => Vector2.SignedAngle(Vector2.up, item.Position - center)).ToArray();
        }

        private static Vector2 GetCenter(GridPoint[] points)
        {
            return (points[0].Position + points[1].Position + points[2].Position + points[3].Position) / 4;
        }

        private Dictionary<GridPoint, GridPoint> GetDiagonalsTable()
        {
            Dictionary<GridPoint, GridPoint> ret = new Dictionary<GridPoint, GridPoint>();
            for (int i = 0; i < 4; i++)
            {
                int opposingIndex = (i + 2) % 4;
                ret.Add(points[i], points[opposingIndex]);
            }
            return ret;
        }

        public override string ToString()
        {
            return points[0].Id + "," + points[1].Id + "," + points[2].Id + "," + points[3].Id;
        }
    }

}