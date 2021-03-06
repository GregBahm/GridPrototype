﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameGrid
{
    public class MainGrid
    {
        public static int VoxelHeight { get; } = 40;

        private List<GroundPoint> points = new List<GroundPoint>();
        public IReadOnlyList<GroundPoint> Points { get { return points; } }

        private List<GroundEdge> edges = new List<GroundEdge>();

        public IEnumerable<GroundEdge> Edges { get { return edges; } }

        private List<GroundQuad> polys = new List<GroundQuad>();
        public IEnumerable<GroundQuad> Polys { get { return polys; } }

        public IEnumerable<GroundEdge> BorderEdges { get; private set; }

        public IEnumerable<VoxelCell> Voxels
        {
            get
            {
                foreach (GroundPoint point in Points)
                {
                    for (int i = 0; i < VoxelHeight; i++)
                    {
                        yield return point.Voxels[i];
                    }
                }
            }
        }

        private readonly Dictionary<GroundPoint, List<GroundEdge>> edgesTable = new Dictionary<GroundPoint, List<GroundEdge>>();
        private readonly Dictionary<GroundPoint, List<GroundQuad>> polyTable = new Dictionary<GroundPoint, List<GroundQuad>>();
        private readonly Dictionary<GroundEdge, List<GroundQuad>> bordersTable = new Dictionary<GroundEdge, List<GroundQuad>>();

        private readonly HashSet<VoxelCell> filledCells = new HashSet<VoxelCell>();
        public IEnumerable<VoxelCell> FilledCells { get { return filledCells; } }

        public MainGrid(IEnumerable<GroundPointBuilder> points, IEnumerable<GroundEdgeBuilder> edges)
        {
            AddToMesh(points, edges);
        }

        internal void SetCellFilled(VoxelCell voxelCell, bool value)
        {
            if(value)
            {
                filledCells.Add(voxelCell);
            }
            else
            {
                filledCells.Remove(voxelCell);
            }
        }

        public bool IsFilled(VoxelCell cell)
        {
            return filledCells.Contains(cell);
        }

        public void AddToMesh(IEnumerable<GroundPointBuilder> newPoints, IEnumerable<GroundEdgeBuilder> newEdges)
        {
            IEnumerable<GroundPoint> points = newPoints.Select(item => new GroundPoint(this, item.Index, item.Position)).ToArray();
            AddPoints(points);
            IEnumerable<GroundEdge> edges = newEdges.Select(item => new GroundEdge(this, Points[item.PointAIndex], Points[item.PointBIndex])).ToArray();
            AddEdges(edges);
            BorderEdges = Edges.Where(item => item.IsBorder).ToArray();

            if(Edges.Any(edge => edge.Quads.Count() == 0 || edge.Quads.Count() > 2))
            {
                throw new Exception("Malformed data.");
            }

            UpdateVoxelVisuals();
        }

        private void UpdateVoxelVisuals()
        {
            foreach (VoxelCell voxel in Voxels)
            {
                voxel.InitializeVisuals();
            }
            foreach (var component in Voxels.SelectMany(item => item.Visuals.Components))
            {
                component.InitializeNeighbors();
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

        internal void DoEase()
        {
            foreach (GroundPoint point in Points)
            {
                DoEasePoint(point, 1);
            }
        }
        private void DoEasePoint(GroundPoint point, float targetCellLength)
        {
            Vector2 normalAverage = Vector2.zero;
            GroundPoint[] allConnections = point.DirectConnections.Concat(point.DiagonalConnections).ToArray();
            foreach (GroundPoint connection in allConnections)
            {
                Vector2 diff = point.Position - connection.Position;
                Vector2 diffNormal = diff.normalized * targetCellLength;
                Vector2 targetPos = connection.Position + diffNormal;
                normalAverage += targetPos;
            }
            normalAverage /= allConnections.Length;
            point.Position = normalAverage;
        }

        private void AddEdges(IEnumerable<GroundEdge> newEdges)
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
            polys.AddRange(quadFinder.Quads);
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

        internal IEnumerable<GroundEdge> GetEdges(GroundPoint gridPoint)
        {
            return edgesTable[gridPoint];
        }

        internal IEnumerable<GroundQuad> GetConnectedQuads(GroundPoint gridPoint)
        {
            return polyTable[gridPoint];
        }

        internal bool GetIsBorder(GroundEdge gridEdge)
        {
            return bordersTable[gridEdge].Count < 2;
        }

        internal IEnumerable<GroundQuad> GetConnectedQuads(GroundEdge gridEdge)
        {
            return bordersTable[gridEdge];
        }

        private IEnumerable<PotentialDiagonal> GetPotentialDiagonals(GroundEdge edge)
        {
            List<PotentialDiagonal> ret = new List<PotentialDiagonal>();
            ret.AddRange(GetPotentialDiagonals(edge.PointA));
            ret.AddRange(GetPotentialDiagonals(edge.PointB));
            return ret;
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
                unavailableDiagonals = GetUnavailableDiagonals(grid.Polys);
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
                IEnumerable<PotentialDiagonal> potentialDiagonals = grid.GetPotentialDiagonals(edge);
                foreach (PotentialDiagonal potentialDiagonal in potentialDiagonals)
                {
                    if (!unavailableDiagonals.Contains(potentialDiagonal.Key))
                    {

                        if (availableDiagonals.ContainsKey(potentialDiagonal.Key))
                        {
                            PotentialDiagonal otherHalf = availableDiagonals[potentialDiagonal.Key];
                            if (potentialDiagonal.SharedPoint != otherHalf.SharedPoint)
                            {
                                GroundQuad newQuad = new GroundQuad(potentialDiagonal.EdgeA, potentialDiagonal.EdgeB, otherHalf.EdgeA, otherHalf.EdgeB);
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
                yield return PotentialDiagonal.GetKey(quad.Points[0].Index, quad.Points[2].Index);
                yield return PotentialDiagonal.GetKey(quad.Points[1].Index, quad.Points[3].Index);
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