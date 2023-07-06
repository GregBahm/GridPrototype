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

        public MainGrid(int maxHeight, IEnumerable<GroundPointBuilder> points, IEnumerable<GroundEdgeBuilder> edges)
        {
            MaxHeight = maxHeight;
            AddToMesh(points, edges);
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

        public bool IsFilled(DesignationCell cell)
        {
            return filledCells.Contains(cell);
        }

        public void AddToMesh(IEnumerable<GroundPointBuilder> newPoints, IEnumerable<GroundEdgeBuilder> newEdges)
        {
            GroundPointBuilder[] sortedNewPoints = newPoints.OrderBy(item => item.Index).ToArray();
            ValidatePointIndicies(sortedNewPoints);

            IEnumerable<GroundPoint> points = sortedNewPoints.Select(item => new GroundPoint(this, item.Index, item.Position)).ToArray();
            AddPoints(points);
            IEnumerable<GroundEdge> edges = newEdges.Select(item => new GroundEdge(this, Points[item.PointAIndex], Points[item.PointBIndex])).ToArray();
            AddEdgesAndQuads(edges);
            BorderEdges = Edges.Where(item => item.IsBorder).ToArray();

            if(Edges.Any(edge => edge.Quads.Count() == 0 || edge.Quads.Count() > 2))
            {
                throw new Exception("Malformed data. Ensure all point and edges form quads.");
            }

            foreach (GroundQuad groundQuad in Quads)
            {
                if(!visualsTable.ContainsKey(groundQuad))
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
        }

        // New points must start at the beginning of the current index and increment up from there
        private void ValidatePointIndicies(GroundPointBuilder[] sortedNewPoints)
        {
            int startingIndex = Points.Count;
            for (int i = 0; i < sortedNewPoints.Length; i++)
            {
                if(sortedNewPoints[i].Index != startingIndex + i)
                {
                    throw new InvalidOperationException("New Points being added to mesh do not have correct indices.");
                }
            }
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

        private void AddPoints(IEnumerable<GroundPoint> newPoints)
        {
            points.AddRange(newPoints);
            foreach (GroundPoint point in newPoints)
            {
                edgesTable.Add(point, new List<GroundEdge>());
                polyTable.Add(point, new List<GroundQuad>());
            }
        }

        public void DoEase()
        {
            GroundPointEaser[] easers = new GroundPointEaser[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                easers[i] = new GroundPointEaser(points[i]);
            }
            foreach (GroundPointEaser easer in easers)
            {
                easer.Point.Position = Vector2.Lerp(easer.Point.Position, easer.OptimalPosition, .5f);
            }
        }

        private void AddEdgesAndQuads(IEnumerable<GroundEdge> newEdges)
        {
            HashSet<GroundPoint> edgesToSort = new HashSet<GroundPoint>();
            edges.AddRange(newEdges);
            foreach (GroundEdge edge in newEdges)
            {
                edgesTable[edge.PointA].Add(edge);
                edgesTable[edge.PointB].Add(edge);
                edgesToSort.Add(edge.PointA);
                edgesToSort.Add(edge.PointB);
                bordersTable.Add(edge, new List<GroundQuad>());
            }
            foreach (GroundPoint point in edgesToSort)
            {
                List<GroundEdge> edges = edgesTable[point];
                List<GroundEdge> sortedList = edges.OrderByDescending(item => GetSignedAngle(item, point)).ToList();
                edgesTable[point] = sortedList;
            }

            QuadFinder quadFinder = new QuadFinder(this, edges.Where(item => item.IsBorder).ToArray());
            quads.AddRange(quadFinder.Quads);
            foreach (GroundQuad quad in quadFinder.Quads)
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

        private float GetSignedAngle(GroundEdge item, GroundPoint point)
        {
            GroundPoint otherPoint = item.GetOtherPoint(point);
            return Vector2.SignedAngle(Vector2.up, otherPoint.Position - point.Position);
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

        private IEnumerable<PotentialDiagonal> GetPotentialDiagonals(GroundEdge edge)
        {
            Dictionary<string, PotentialDiagonal> ret = new Dictionary<string, PotentialDiagonal>();
            foreach (PotentialDiagonal item in GetPotentialDiagonals(edge.PointA))
            {
                if (!ret.ContainsKey(item.Key))
                    ret.Add(item.Key, item);
            }
            foreach (PotentialDiagonal item in GetPotentialDiagonals(edge.PointB))
            {
                if (!ret.ContainsKey(item.Key))
                    ret.Add(item.Key, item);
            }
            return ret.Values;
        }
        private IEnumerable<PotentialDiagonal> GetPotentialDiagonals(GroundPoint point)
        {
            List<GroundEdge> edgeList = edgesTable[point];
            for (int i = 0; i < edgeList.Count; i++)
            {
                int nextIndex = (i + 1) % edgeList.Count;
                yield return new PotentialDiagonal(edgeList[i], edgeList[nextIndex], point);
            }
        }

        private class QuadFinder
        {
            private readonly MainGrid grid;
            private readonly HashSet<string> unavailableDiagonals;
            private readonly Dictionary<string, PotentialDiagonal> availableDiagonals = new Dictionary<string, PotentialDiagonal>();
            private List<GroundQuad> quads = new List<GroundQuad>();
            public IEnumerable<GroundQuad> Quads { get { return quads; } }

            public QuadFinder(MainGrid grid, IEnumerable<GroundEdge> borderEdges)
            {
                this.grid = grid;
                unavailableDiagonals = GetUnavailableDiagonals(grid.Quads);
                foreach (GroundEdge edge in borderEdges)
                {
                    ProcessEdge(edge);
                }
            }

            private HashSet<string> GetUnavailableDiagonals(IEnumerable<GroundQuad> polys)
            {
                HashSet<string> ret = new HashSet<string>();
                foreach (GroundQuad quad in polys)
                {
                    foreach (string key in GetKeysFor(quad))
                    {
                        ret.Add(key);
                    }
                }
                return ret;
            }

            private void ProcessEdge(GroundEdge edge)
            {
                IEnumerable<PotentialDiagonal> potentialDiagonals = grid.GetPotentialDiagonals(edge); // Every point connection to a point of this edge. If it's not an edge, it's the diagonal of a quad 

                foreach (PotentialDiagonal potentialDiagonal in potentialDiagonals)
                {
                    if (!unavailableDiagonals.Contains(potentialDiagonal.Key)) // Need to check in line because unavailable changes during the loop
                    {
                        if (availableDiagonals.ContainsKey(potentialDiagonal.Key)) // Another edge has already put this diagonal in the list
                        {
                            PotentialDiagonal otherHalf = availableDiagonals[potentialDiagonal.Key];
                            if (potentialDiagonal.SharedPoint != otherHalf.SharedPoint)
                            {
                                GroundQuad newQuad = new GroundQuad(potentialDiagonal.EdgeA, potentialDiagonal.EdgeB, otherHalf.EdgeA, otherHalf.EdgeB);
                                if (quads.Any(item => item.ToString() == newQuad.ToString()))
                                {
                                    throw new Exception("That ain't right.");
                                }
                                RegisterNewQuad(newQuad);
                                quads.Add(newQuad);
                            }
                        }
                        else
                        {
                            availableDiagonals.Add(potentialDiagonal.Key, potentialDiagonal);
                        }
                    }
                }
            }

            private void RegisterNewQuad(GroundQuad newQuad)
            {
                foreach (string key in GetKeysFor(newQuad))
                {
                    if(availableDiagonals.ContainsKey(key))
                    {
                        availableDiagonals.Remove(key);
                    }
                    unavailableDiagonals.Add(key);
                }
            }

            private IEnumerable<string> GetKeysFor(GroundQuad quad)
            {
                yield return PotentialDiagonal.GetKey(quad.Points[0].Index, quad.Points[1].Index);
                yield return PotentialDiagonal.GetKey(quad.Points[0].Index, quad.Points[2].Index);
                yield return PotentialDiagonal.GetKey(quad.Points[0].Index, quad.Points[3].Index);
                yield return PotentialDiagonal.GetKey(quad.Points[1].Index, quad.Points[2].Index);
                yield return PotentialDiagonal.GetKey(quad.Points[1].Index, quad.Points[3].Index);
                yield return PotentialDiagonal.GetKey(quad.Points[2].Index, quad.Points[3].Index);
            }
        }

        private class PotentialDiagonal
        {
            public string Key { get; }
            public GroundEdge EdgeA { get; }
            public GroundEdge EdgeB { get; }
            public GroundPoint SharedPoint { get; }

            public PotentialDiagonal(GroundEdge edgeA, GroundEdge edgeB, GroundPoint sharedPoint)
            {
                EdgeA = edgeA;
                EdgeB = edgeB;
                SharedPoint = sharedPoint;
                GroundPoint otherPointA = edgeA.GetOtherPoint(sharedPoint);
                GroundPoint otherPointB = edgeB.GetOtherPoint(sharedPoint);
                Key = GetKey(otherPointA.Index, otherPointB.Index);
            }

            public static string GetKey(int id1, int id2)
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
        private class Squarifier
        {
            private readonly Vector2[] outputs;
            public Vector2 OutputA { get { return outputs[0]; } }
            public Vector2 OutputB { get { return outputs[1]; } }
            public Vector2 OutputC { get { return outputs[2]; } }
            public Vector2 OutputD { get { return outputs[3]; } }

            public Squarifier(GroundQuad quad, float size)
                : this(quad.Points[0].Position, quad.Points[1].Position, quad.Points[2].Position, quad.Points[3].Position, size)
            { }
            public Squarifier(Vector2 inputA, Vector2 inputB, Vector2 inputC, Vector2 inputD, float size)
            {
                Vector2 ac = new Vector2(inputA.x, inputA.y) - new Vector2(inputC.x, inputC.y);
                Vector2 bd = new Vector2(inputB.x, inputB.y) - new Vector2(inputD.x, inputD.y);

                Vector2 crossBd = new Vector2(bd.y, -bd.x);

                Vector2 average = (ac.normalized + crossBd.normalized) / 2;
                average = average.normalized;
                Vector2 offsetA = average;
                Vector2 offsetB = new Vector2(average.y, -average.x);
                Vector2 offsetC = -average;
                Vector2 offsetD = new Vector2(-average.y, average.x);

                Vector2 center = (inputA + inputB + inputC + inputD) / 4;

                PositionPair outputA = new PositionPair(offsetA * size + center, 0);
                PositionPair outputB = new PositionPair(offsetB * size + center, 1);
                PositionPair outputC = new PositionPair(offsetC * size + center, 2);
                PositionPair outputD = new PositionPair(offsetD * size + center, 3);
                List<PositionPair> sourceOutputs = new List<PositionPair> { outputA, outputB, outputC, outputD };

                outputs = new Vector2[4];

                PositionPair bestA = GetBest(inputA, sourceOutputs);
                outputs[0] = bestA.Pos;
                sourceOutputs.Remove(bestA);

                PositionPair bestB = GetBest(inputB, sourceOutputs);
                outputs[1] = bestB.Pos;
                sourceOutputs.Remove(bestB);

                PositionPair bestC = GetBest(inputC, sourceOutputs);
                outputs[2] = bestC.Pos;
                sourceOutputs.Remove(bestC);

                outputs[3] = sourceOutputs[0].Pos;
            }

            private class PositionPair
            {
                public int Index { get; }
                public Vector2 Pos { get; }
                public PositionPair(Vector2 pos, int index)
                {
                    Index = index;
                    Pos = pos;
                }
            }

            private PositionPair GetBest(Vector2 input, List<PositionPair> options)
            {
                float min = Mathf.Infinity;
                PositionPair ret = null;
                foreach (var item in options)
                {
                    float dist = (input - item.Pos).sqrMagnitude;
                    if (dist < min)
                    {
                        min = dist;
                        ret = item;
                    }
                }
                return ret;
            }
        }

        private struct GroundPointEaser
        {
            public GroundPoint Point { get; }
            public Vector2 OptimalPosition { get; }

            public GroundPointEaser(GroundPoint groundPoint)
            {
                Point = groundPoint;
                //OptimalPosition = groundPoint.IsBorder ? groundPoint.Position : GetOptimalPosition(groundPoint);
                OptimalPosition = GetOptimalPosition(groundPoint);
            }

            private static Vector2 GetOptimalPosition(GroundPoint groundPoint)
            {
                Vector2[] connected = groundPoint.DirectConnections.Select(item => item.Position).ToArray();
                Vector2 offsetSum = Vector2.zero;
                for (int i = 0; i < connected.Length; i++)
                {

                    Vector2 diff = connected[i] - groundPoint.Position;
                    Vector2 idealPos = diff.normalized;
                    offsetSum -= idealPos;
                }
                offsetSum /= connected.Length;
                return offsetSum + groundPoint.Position;
            }

            private static Connection GetConnection(Vector2 source, Vector2 target)
            {
                Vector2 diff = source - target;
                Vector2 idealPos = diff.normalized + source;
                float weight = Mathf.Abs(diff.magnitude);
                weight *= weight;
                return new Connection(idealPos, weight);
            }

            private struct Connection
            {
                public Vector2 Offset { get; }
                public float Weight { get; }

                public Connection(Vector2 targetPosition, float weight)
                {
                    Offset = targetPosition;
                    Weight = weight;
                }
            }

            private static IEnumerable<GroundPoint> GetConnectedPoints(GroundPoint point)
            {
                HashSet<GroundPoint> ret = new HashSet<GroundPoint>(point.DirectConnections);
                foreach (GroundPoint diagonal in point.DiagonalConnections)
                {
                    ret.Add(diagonal);
                }
                return ret;
            }
        }
    }
}