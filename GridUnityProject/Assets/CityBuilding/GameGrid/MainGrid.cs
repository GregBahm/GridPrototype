using Interiors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using VoxelVisuals;
using static UnityEngine.GraphicsBuffer;

namespace GameGrid
{
    public class MainGrid
    {
        public const int DefaultMaxHeight = 40;
        public int MaxHeight { get; }

        private List<GroundPoint> points = new List<GroundPoint>();
        public IReadOnlyList<GroundPoint> Points { get { return points; } }

        private List<GroundEdge> edges = new List<GroundEdge>();

        public IEnumerable<GroundEdge> Edges { get { return edges; } }

        private List<GroundQuad> quads = new List<GroundQuad>();
        public IEnumerable<GroundQuad> Quads { get { return quads; } }

        public IReadOnlyList<GroundEdge> BorderEdges { get; private set; }

        public IEnumerable<DesignationCell> DesignationCells
        {
            get
            {
                foreach (GroundPoint point in Points)
                {
                    for (int i = 0; i < MaxHeight; i++)
                    {
                        yield return point.DesignationCells[i];
                    }
                }
            }
        }
        public GridInteriors Interiors { get; }

        private readonly Dictionary<GroundPoint, List<GroundEdge>> edgesTable = new Dictionary<GroundPoint, List<GroundEdge>>();
        private readonly Dictionary<GroundPoint, List<GroundQuad>> polyTable = new Dictionary<GroundPoint, List<GroundQuad>>();
        private readonly Dictionary<GroundEdge, List<GroundQuad>> bordersTable = new Dictionary<GroundEdge, List<GroundQuad>>();

        private readonly Dictionary<GroundQuad, List<VisualCell>> visualsTable = new Dictionary<GroundQuad, List<VisualCell>>();

        private readonly HashSet<DesignationCell> filledCells = new HashSet<DesignationCell>();
        public IEnumerable<DesignationCell> FilledCells { get { return filledCells; } }

        public MainGrid(int maxHeight, GroundPointBuilder[] pointBuilders, GroundQuadBuilder[] quadBuilders)
        {
            MaxHeight = maxHeight;

            points = GetGroundPoints(pointBuilders);
            edges = new List<GroundEdge>();
            quads = new List<GroundQuad>(quadBuilders.Length);

            foreach (GroundPoint point in points)
            {
                edgesTable.Add(point, new List<GroundEdge>());
                polyTable.Add(point, new List<GroundQuad>());
            }

            LoadEdgesAndQuads(quadBuilders);
            RegisterGridComponents();

            BorderEdges = Edges.Where(item => item.IsBorder).ToArray();

            if (Edges.Any(edge => edge.Quads.Count() == 0 || edge.Quads.Count() > 2))
            {
                throw new Exception("Malformed data. Ensure all point and edges form quads.");
            }

            foreach (GroundQuad groundQuad in Quads)
            {
                if (!visualsTable.ContainsKey(groundQuad))
                {
                    List<VisualCell> visualCells = new List<VisualCell>();
                    for (int i = 0; i < MaxHeight; i++)
                    {
                        visualCells.Add(new VisualCell(this, groundQuad, i));
                    }
                    visualsTable.Add(groundQuad, visualCells);
                }
            }

            UpdateVoxelVisuals();
            Interiors = new GridInteriors();
        }

        public void SetCellFilled(DesignationCell designationCell, bool value)
        {
            if(value)
            {
                filledCells.Add(designationCell);
            }
            else
            {
                filledCells.Remove(designationCell);
            }
        }

        private void LoadEdgesAndQuads(GroundQuadBuilder[] quadBuilders)
        {
            Dictionary<string, GroundEdge> edgesTable = new Dictionary<string, GroundEdge>();

            for (int i = 0; i < quadBuilders.Length; i++)
            {

                GroundQuadBuilder builder = quadBuilders[i];

                GroundPoint a = points[builder.PointAIndex];
                GroundPoint b = points[builder.PointBIndex];
                GroundPoint c = points[builder.PointCIndex];
                GroundPoint d = points[builder.PointDIndex];

                Vector2 center = (a.Position + b.Position + c.Position + d.Position) * .25f;

                GroundPoint[] sortedPoints = GetPointsClockwiseAroundCenter(center, a, b, c, d);
                GroundEdge[] sortedEdges = GetOrCreateEdges(sortedPoints, edgesTable);

                GroundQuad quad = new GroundQuad(sortedPoints, sortedEdges, center);
                quads.Add(quad);
            }
            edges = edgesTable.Values.ToList();
        }

        private GroundEdge[] GetOrCreateEdges(GroundPoint[] sortedPoints, Dictionary<string, GroundEdge> edgesTable)
        {
            GroundEdge edgeA = GetOrCreateEdges(sortedPoints[0], sortedPoints[1], edgesTable);
            GroundEdge edgeB = GetOrCreateEdges(sortedPoints[1], sortedPoints[2], edgesTable);
            GroundEdge edgeC = GetOrCreateEdges(sortedPoints[2], sortedPoints[3], edgesTable);
            GroundEdge edgeD = GetOrCreateEdges(sortedPoints[3], sortedPoints[0], edgesTable);
            return new GroundEdge[] { edgeA, edgeB, edgeC, edgeD };
        }

        private GroundEdge GetOrCreateEdges(GroundPoint pointA, GroundPoint pointB, Dictionary<string, GroundEdge> edgesTable)
        {
            GroundEdge edge = new GroundEdge(this, pointA, pointB);
            string key = edge.ToString();
            if(edgesTable.ContainsKey(key))
            {
                return edgesTable[key];
            }
            edgesTable.Add(key, edge);
            return edge;
        }

        private GroundPoint[] GetPointsClockwiseAroundCenter(Vector2 center, GroundPoint a, GroundPoint b, GroundPoint c, GroundPoint d)
        {
            GroundPoint[] asSet = new GroundPoint[] { a, b, c, d };
            return asSet.OrderByDescending(item => GetSignedAngle(center, item.Position)).ToArray();
        }

        private List<GroundPoint> GetGroundPoints(GroundPointBuilder[] newPoints)
        {
            GroundPoint[] ret = new GroundPoint[newPoints.Length];
            for (int i = 0; i < newPoints.Length; i++)
            {
                GroundPointBuilder point = newPoints[i];
                ret[point.Index] = new GroundPoint(this, i, point.Position);
            }
            return ret.ToList();
        }

        private void UpdateVoxelVisuals()
        {
            foreach (DesignationCell voxel in DesignationCells)
            {
                voxel.PopulateVisuals();
            }
            foreach (VisualCell visualCell in visualsTable.SelectMany(item => item.Value))
            {
                visualCell.InitializeNeighbors();
            }
        }

        private void RegisterGridComponents()
        {
            foreach (GroundEdge edge in edges)
            {
                edgesTable[edge.PointA].Add(edge);
                edgesTable[edge.PointB].Add(edge);
                bordersTable.Add(edge, new List<GroundQuad>());
            }
            foreach (GroundPoint point in points)
            {
                List<GroundEdge> edges = edgesTable[point];
                List<GroundEdge> sortedList = edges.OrderByDescending(item => GetSignedAngle(item, point)).ToList();
                edgesTable[point] = sortedList;
            }

            foreach (GroundQuad quad in quads)
            {
                foreach (GroundEdge edge in quad.Edges)
                {
                    bordersTable[edge].Add(quad);
                }
                foreach (GroundPoint point in quad.Points)
                {
                    polyTable[point].Add(quad);
                }
            }
        }

        private static float GetSignedAngle(GroundEdge item, GroundPoint point)
        {
            GroundPoint otherPoint = item.GetOtherPoint(point);
            return Vector2.SignedAngle(Vector2.up, otherPoint.Position - point.Position);
        }

        private static float GetSignedAngle(Vector2 center, Vector2 point)
        {
            return Vector2.SignedAngle(Vector2.up, point - center);
        }

        public IEnumerable<GroundEdge> GetEdges(GroundPoint gridPoint)
        {
            return edgesTable[gridPoint];
        }

        public IEnumerable<GroundQuad> GetConnectedQuads(GroundPoint gridPoint)
        {
            return polyTable[gridPoint];
        }

        public VisualCell GetVisualCell(GroundQuad quad, int height)
        {
            return visualsTable[quad][height];
        }

        public bool GetIsBorder(GroundEdge gridEdge)
        {
            return bordersTable[gridEdge].Count < 2;
        }

        public IEnumerable<GroundQuad> GetConnectedQuads(GroundEdge gridEdge)
        {
            return bordersTable[gridEdge];
        }
    }
}