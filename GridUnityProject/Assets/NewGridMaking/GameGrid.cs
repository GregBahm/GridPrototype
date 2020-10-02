using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

        private List<GridPoly> polys = new List<GridPoly>();
        public IEnumerable<GridPoly> Polys { get { return polys; } }

        private readonly Dictionary<GridPoint, List<GridEdge>> edgesTable = new Dictionary<GridPoint, List<GridEdge>>();
        private readonly Dictionary<GridPoint, List<GridPoly>> polyTable = new Dictionary<GridPoint, List<GridPoly>>();
        private readonly Dictionary<GridEdge, List<GridPoly>> bordersTable = new Dictionary<GridEdge, List<GridPoly>>();

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
                polyTable.Add(point, new List<GridPoly>());
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
                bordersTable.Add(edge, new List<GridPoly>());
            }
            foreach (GridPoint point in edgesToSort)
            {
                List<GridEdge> edges = edgesTable[point];
                List<GridEdge> sortedList = edges.OrderByDescending(item => GetAngle(item, point)).ToList();
                edgesTable[point] = sortedList;
            }


            // Update Diagonal connections
        }

        private float GetAngle(GridEdge item, GridPoint point)
        {
            GridPoint otherPoint = item.GetOtherPoint(point);
            return Vector2.Angle(Vector2.up, otherPoint.Position - point.Position);
        }

        internal IEnumerable<GridEdge> GetEdges(GridPoint gridPoint)
        {
            return edgesTable[gridPoint];
        }

        internal IEnumerable<GridPoly> GetConnectedQuads(GridPoint gridPoint)
        {
            return polyTable[gridPoint];
        }

        internal bool GetIsBorder(GridEdge gridEdge)
        {
            return bordersTable[gridEdge].Count < 2;
        }

        internal IEnumerable<GridPoly> GetConnectedQuads(GridEdge gridEdge)
        {
            return bordersTable[gridEdge];
        }

        private class TwinnerTable
        {
            // I will need to form these rings of twinners around each point connected to a border edge
            // 
        }

        private class Twinner
        {
            public string Key { get; }
            public GridEdge EdgeA { get; }
            public GridEdge EdgeB { get; }

            public Twinner(GridEdge edgeA, GridEdge edgeB, GridPoint sharedPoint)
            {
                EdgeA = edgeA;
                EdgeB = edgeB;
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
        }
    }

    public class GridEdge
    {
        private readonly MasterGrid grid;

        public GridPoint PointA { get; }
        public GridPoint PointB { get; }

        public Vector2 MidPoint { get { return (PointA.Position + PointB.Position) / 2; } }

        public IEnumerable<GridPoly> Quads { get { return grid.GetConnectedQuads(this); } }
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
    }

    public class GridPoint
    {
        private readonly MasterGrid grid;

        public int Id { get; }
        public Vector2 Position { get; set; }
        public IEnumerable<GridEdge> Edges { get { return grid.GetEdges(this); } }
        public IEnumerable<GridPoint> DirectConnections { get { return Edges.Select(item => item.GetOtherPoint(this)); } }
        public IEnumerable<GridPoint> DiagonalConnections { get { return PolyConnections.Select(item => item.GetDiagonalPoint(this)); } }
        public IEnumerable<GridPoly> PolyConnections { get { return grid.GetConnectedQuads(this); } }

        public GridPoint(MasterGrid grid, int id, Vector2 initialPosition)
        {
            this.grid = grid;
            Id = id;
            Position = initialPosition;
        }
    }

    public class GridPoly
    {
        private readonly Dictionary<GridPoint, GridPoint> diagonalsTable;

        private readonly GridPoint[] points;
        public IEnumerable<GridPoint> Points { get { return points; } }

        public Vector2 Center { get { return GetCenter(points); } }

        public GridPoly(IEnumerable<GridPoint> points)
        {
            this.points = points.ToArray();
            if (this.points.Length != 4)
            {
                throw new ArgumentException("Can't create a GridPoly with " + this.points.Length + " points");
            }
            GridPoint[] sortedPoints = GetSortedPoints();
            diagonalsTable = GetDiagonalsTable(sortedPoints);
        }

        public GridPoint GetDiagonalPoint(GridPoint point)
        {
            return diagonalsTable[point];
        }

        private GridPoint[] GetSortedPoints()
        {
            Vector2 center = GetCenter(points);
            return points.OrderByDescending(item => Vector2.Angle(Vector2.up, item.Position - center)).ToArray();
        }

        private static Vector2 GetCenter(GridPoint[] points)
        {
            return (points[0].Position + points[1].Position + points[2].Position + points[3].Position) / 4;
        }

        private Dictionary<GridPoint, GridPoint> GetDiagonalsTable(GridPoint[] sortedPoints)
        {
            Dictionary<GridPoint, GridPoint> ret = new Dictionary<GridPoint, GridPoint>();
            for (int i = 0; i < 4; i++)
            {
                int opposingIndex = (i + 2) % 4;
                ret.Add(sortedPoints[i], sortedPoints[opposingIndex]);
            }
            return ret;
        }
    }

}