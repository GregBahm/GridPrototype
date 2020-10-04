﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameGrid
{
    public class MainGrid
    {
        private List<GridPoint> points = new List<GridPoint>();
        public IEnumerable<GridPoint> Points { get { return points; } }

        private List<GridEdge> edges = new List<GridEdge>();
        public IEnumerable<GridEdge> Edges { get { return edges; } }

        private List<GridQuad> polys = new List<GridQuad>();
        public IEnumerable<GridQuad> Polys { get { return polys; } }

        public IEnumerable<GridEdge> BorderEdges { get; private set; }

        private readonly Dictionary<GridPoint, List<GridEdge>> edgesTable = new Dictionary<GridPoint, List<GridEdge>>();
        private readonly Dictionary<GridPoint, List<GridQuad>> polyTable = new Dictionary<GridPoint, List<GridQuad>>();
        private readonly Dictionary<GridEdge, List<GridQuad>> bordersTable = new Dictionary<GridEdge, List<GridQuad>>();

        public void AddToMesh(IEnumerable<GridPoint> newPoints, IEnumerable<GridEdge> newEdges)
        {
            AddPoints(newPoints);
            AddEdges(newEdges);
            BorderEdges = Edges.Where(item => item.IsBorder).ToArray();
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
            private readonly MainGrid grid;
            private readonly HashSet<string> unavailableDiagonals;
            private readonly Dictionary<string, PotentialDiagonal> availableDiagonals = new Dictionary<string, PotentialDiagonal>();
            private List<GridQuad> quads = new List<GridQuad>();
            public IEnumerable<GridQuad> Quads { get { return quads; } }

            public QuadFinder(MainGrid grid, IEnumerable<GridEdge> borderEdges)
            {
                this.grid = grid;
                unavailableDiagonals = GetUnavailableDiagonals(grid.Polys);
                foreach (GridEdge edge in borderEdges)
                {
                    ProcessEdge(edge);
                }
            }

            private HashSet<string> GetUnavailableDiagonals(IEnumerable<GridQuad> polys)
            {
                HashSet<string> ret = new HashSet<string>();
                foreach (GridQuad quad in polys)
                {
                    foreach (string key in GetKeysFor(quad))
                    {
                        ret.Add(key);
                    }
                }
                return ret;
            }

            private void ProcessEdge(GridEdge edge)
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
                                GridQuad newQuad = new GridQuad(potentialDiagonal.EdgeA, potentialDiagonal.EdgeB, otherHalf.EdgeA, otherHalf.EdgeB);
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

            private void RegisterNewQuad(GridQuad newQuad)
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

            private IEnumerable<string> GetKeysFor(GridQuad quad)
            {
                yield return PotentialDiagonal.GetKey(quad.Points[0].Id, quad.Points[2].Id);
                yield return PotentialDiagonal.GetKey(quad.Points[1].Id, quad.Points[3].Id);
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