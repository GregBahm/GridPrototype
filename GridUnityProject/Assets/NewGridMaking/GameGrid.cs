using System;
using System.Collections;
using System.Collections.Generic;
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
            GridLoader gridLoader = JsonUtility.FromJson<GridLoader>(data);
            Dictionary<int, GridPoint> lookupTable = gridLoader.Points.ToDictionary(item => item.Id, item => new GridPoint(ret, item.Id, item.Pos));
            IEnumerable<GridEdge> edges = gridLoader.Edges.Select(item => new GridEdge(lookupTable[item.PointAId], lookupTable[item.PointBIds])).ToArray();
            ret.AddPoints(lookupTable.Values);
            ret.AddEdges(edges);
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

        private readonly Dictionary<GridPoint, List<GridPoint>> directConnetions = new Dictionary<GridPoint, List<GridPoint>>();
        private readonly Dictionary<GridPoint, List<GridPoint>> diagonalConnections = new Dictionary<GridPoint, List<GridPoint>>();
        private readonly Dictionary<GridPoint, List<GridEdge>> edgesTable = new Dictionary<GridPoint, List<GridEdge>>();
        private readonly Dictionary<GridPoint, List<GridPoly>> polyTable = new Dictionary<GridPoint, List<GridPoly>>();

        public void AddPoints(IEnumerable<GridPoint> newPoints)
        {
            points.AddRange(newPoints);
            foreach (GridPoint point in newPoints)
            {
                directConnetions.Add(point, new List<GridPoint>());
                diagonalConnections.Add(point, new List<GridPoint>());
                edgesTable.Add(point, new List<GridEdge>());
                polyTable.Add(point, new List<GridPoly>());
            }
        }

        public void AddEdges(IEnumerable<GridEdge> edges)
        {
            this.edges.AddRange(edges);
            foreach (GridEdge edge in edges)
            {
                directConnetions[edge.PointA].Add(edge.PointB);
                directConnetions[edge.PointB].Add(edge.PointA);
                edgesTable[edge.PointA].Add(edge);
                edgesTable[edge.PointB].Add(edge);
            }
            // Updates Polygons
            // Update Diagonal connections
        }

        internal IEnumerable<GridPoint> GetDirectConnections(GridPoint gridPoint)
        {
            return directConnetions[gridPoint];
        }

        internal List<GridPoint> GetDiagonalConnections(GridPoint gridPoint)
        {
            return diagonalConnections[gridPoint];
        }

        internal IEnumerable<GridEdge> GetEdges(GridPoint gridPoint)
        {
            return edgesTable[gridPoint];
        }

        internal IEnumerable<GridPoly> GetConnectedQuads(GridPoint gridPoint)
        {
            return polyTable[gridPoint];
        }
    }

    public class GridEdge
    {
        public GridPoint PointA { get; }
        public GridPoint PointB { get; }

        public Vector2 MidPoint { get { return (PointA.Position + PointB.Position) / 2; } }

        public GridEdge(GridPoint pointA, GridPoint pointB)
        {
            PointA = pointA;
            PointB = pointB;
        }
    }

    public class GridPoint
    {
        private readonly MasterGrid grid;

        public int Id { get; }
        public Vector2 Position { get; set; }
        public IEnumerable<GridEdge> Edges { get { return grid.GetEdges(this); } }
        public IEnumerable<GridPoint> DirectConnections { get { return grid.GetDirectConnections(this); } }
        public IEnumerable<GridPoint> DiagonalConnections { get { return grid.GetDiagonalConnections(this); } }
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
        public IEnumerable<GridPoint> Points { get; }

        public GridPoly(IEnumerable<GridPoint> points)
        {
            Points = points.ToArray();
        }
    }

}