using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        public IEnumerable<DesignationCell> Voxels
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

        public event EventHandler GridChanged;

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
            GridChanged?.Invoke(this, EventArgs.Empty);
        }

        // new points must start at the beginning of the current index and increment up from there
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
            foreach (DesignationCell voxel in Voxels)
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
            foreach (GroundPoint point in Points.Where(point => point.IsBorder))
            {
                DoEaseBorderPoint(point);
            }
            foreach (GroundPoint point in Points.Where(point => !point.IsBorder))
            {
                DoEaseInteriorPoint(point);
            }
        }
        private void DoEaseBorderPoint(GroundPoint point)
        {
            Vector2 positionAverage = Vector2.zero;
            Vector2[] borderConnections = point.DirectConnections.Where(item => item.IsBorder).Select(item => item.Position).ToArray();
            foreach (Vector2 connection in borderConnections)
            {
                positionAverage += connection;
            }
            positionAverage /= borderConnections.Count();
            positionAverage = positionAverage.normalized * point.Position.magnitude;
            //point.Position = positionAverage;
        }

        private void DoEaseInteriorPoint(GroundPoint point)
        {
            Vector2 positionOffset = Vector2.zero;
            foreach(GroundPoint connection in point.DirectConnections)
            {
                Vector2 toPosition = point.Position - connection.Position;//connection.Position - point.Position;
                float weight = Mathf.Abs(toPosition.magnitude - 1f);
                weight = Mathf.Pow(weight, 5);
                positionOffset += toPosition.normalized * weight;
            }
            point.Position += positionOffset;
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
    }

}