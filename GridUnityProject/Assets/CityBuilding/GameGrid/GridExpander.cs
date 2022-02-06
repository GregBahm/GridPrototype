using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GameGrid
{   
    public class GridExpander
    {
        private readonly MainGrid grid;

        public int VertExpansionCount { get; }
        public float ExpansionDistance { get; }

        public IReadOnlyList<GroundPointBuilder> Points { get; private set; }
        public IReadOnlyList<GroundEdgeBuilder> Edges { get; private set; }

        public GridExpander(MainGrid grid, int vertExpansionCount, float expansionDistance)
        {
            this.grid = grid;
            VertExpansionCount = vertExpansionCount;
            ExpansionDistance = expansionDistance;
        }

        public void Update(Vector2 gridSpaceCursorPos)
        {
            GroundPoint basePoint = GetClosestBorderPoint(gridSpaceCursorPos);

            ExpansionChain chain = new ExpansionChain(basePoint, VertExpansionCount, grid, ExpansionDistance);
            if(chain.IsFullRing)
            {
                SetFullRingPointsAndEdges(chain);
            }
            else
            {
                SetPartialRingPointsAndEdges(chain);
            }
        }

        private void SetPartialRingPointsAndEdges(ExpansionChain chain)
        {
            List<GroundPointBuilder> points = new List<GroundPointBuilder>();
            List<GroundEdgeBuilder> edges = new List<GroundEdgeBuilder>();

            for (int i = 0; i < chain.ExpansionVertChain.Count - 1; i++)
            {
                GroundPointBuilder point = chain.ExpansionVertChain[i].NewPoint;
                GroundEdgeBuilder spokEdge = new GroundEdgeBuilder(point.Index, chain.ExpansionVertChain[i].BasePoint.Index);
                int nextVertIndex = (i + 1) % chain.ExpansionVertChain.Count;
                GroundPointBuilder rimEdgePoint = chain.ExpansionVertChain[nextVertIndex].NewPoint;
                GroundEdgeBuilder rimEdge = new GroundEdgeBuilder(point.Index, rimEdgePoint.Index);
                points.Add(point);
                edges.Add(spokEdge);
                edges.Add(rimEdge);
            }
            points.Add(chain.ExpansionVertChain.Last().NewPoint);
            edges.Add(chain.ExpansionVertChain.Last().SpokeEdge);

            AddChainReturnQuad(points, edges, chain.ChainReturnPointStart, chain.ExpansionVertChain.First(), chain.NewStartPointIndex);
            AddChainReturnQuad(points, edges, chain.ChainReturnPointEnd, chain.ExpansionVertChain.Last(), chain.NewEndPointIndex);

            Points = points;
            Edges = edges;
        }

        private void AddChainReturnQuad(List<GroundPointBuilder> points, List<GroundEdgeBuilder> edges, GroundPoint returnPoint, ExpanderVert expansionPoint, int spanPointIndex)
        {
            Vector2 spanPointPos = (returnPoint.Position + expansionPoint.NewPoint.Position) / 2;
            Vector2 toBase = spanPointPos - expansionPoint.BasePoint.Position;
            spanPointPos += toBase * .5f;
            GroundPointBuilder spanPoint = new GroundPointBuilder(spanPointIndex, spanPointPos);

            GroundEdgeBuilder expansionToSpan = new GroundEdgeBuilder(expansionPoint.NewPoint.Index, spanPointIndex);
            GroundEdgeBuilder spanToReturn = new GroundEdgeBuilder(spanPointIndex, returnPoint.Index);

            points.Add(spanPoint);
            edges.Add(expansionToSpan);
            edges.Add(spanToReturn);
        }

        private void SetFullRingPointsAndEdges(ExpansionChain chain)
        {
            List<GroundPointBuilder> points = new List<GroundPointBuilder>();
            List<GroundEdgeBuilder> edges = new List<GroundEdgeBuilder>();
            for (int i = 0; i < chain.ExpansionVertChain.Count; i++)
            {
                GroundPointBuilder point = chain.ExpansionVertChain[i].NewPoint;
                GroundEdgeBuilder spokEdge = chain.ExpansionVertChain[i].SpokeEdge;
                int nextVertIndex = (i + 1) % chain.ExpansionVertChain.Count;
                GroundPointBuilder rimEdgePoint = chain.ExpansionVertChain[nextVertIndex].NewPoint;
                GroundEdgeBuilder rimEdge = new GroundEdgeBuilder(point.Index, rimEdgePoint.Index);
                points.Add(point);
                edges.Add(spokEdge);
                edges.Add(rimEdge);
            }
            Points = points;
            Edges = edges;
        }

        internal void PreviewExpansion()
        {
            Dictionary<int, Vector2> positions = Points.ToDictionary(item => item.Index, item => item.Position);
            foreach (GroundEdgeBuilder edge in Edges)
            {
                Vector2 posA = positions.ContainsKey(edge.PointAIndex) ? positions[edge.PointAIndex]
                    : grid.Points[edge.PointAIndex].Position;
                Vector2 posB = positions.ContainsKey(edge.PointBIndex) ? positions[edge.PointBIndex]
                    : grid.Points[edge.PointBIndex].Position;
                Vector3 fullPosA = new Vector3(posA.x, 0, posA.y);
                Vector3 fullPosB = new Vector3(posB.x, 0, posB.y);
                Debug.DrawLine(fullPosA, fullPosB);
            }
        }

        private class ExpansionChain
        {
            public bool IsFullRing { get; }
            public GroundPoint ChainReturnPointStart { get; }
            public int NewStartPointIndex { get; }
            public GroundPoint ChainReturnPointEnd { get; }
            public int NewEndPointIndex { get; }
            public List<ExpanderVert> ExpansionVertChain { get; }

            private readonly int expansions;
            private readonly MainGrid grid;
            private readonly float expansionDistance;

            public ExpansionChain(GroundPoint basePoint, int expansions, MainGrid grid, float expansionDistance)
            {
                this.expansionDistance = expansionDistance;
                this.grid = grid;
                int maxExpansions = GetMaxExpansions(grid);
                this.expansions = Mathf.Clamp(expansions, 0, maxExpansions);
                ExpansionVertChain = GetExpanderVertChain(basePoint);
                IsFullRing = grid.BorderEdges.Count == ExpansionVertChain.Count;
                if(!IsFullRing)
                {
                    if(expansions == 0)
                    {
                        ChainReturnPointStart = ExpansionVertChain[0].NeighborA;
                        ChainReturnPointEnd = ExpansionVertChain[0].NeighborB;
                    }
                    else
                    {
                        HashSet<GroundPoint> usedPoints = new HashSet<GroundPoint>(ExpansionVertChain.Select(item => item.BasePoint));
                        ChainReturnPointStart = GetChainReturnPoint(usedPoints, ExpansionVertChain.First());
                        ChainReturnPointEnd = GetChainReturnPoint(usedPoints, ExpansionVertChain.Last());
                    }
                    NewStartPointIndex = grid.Points.Count + 1 + this.expansions;
                    NewEndPointIndex = grid.Points.Count + 2 + this.expansions;
                }
            }

            private GroundPoint GetChainReturnPoint(HashSet<GroundPoint> usedPoints, ExpanderVert expanderVert)
            {
                return usedPoints.Contains(expanderVert.NeighborA) ? expanderVert.NeighborB : expanderVert.NeighborA;
            }

            private List<ExpanderVert> GetExpanderVertChain(GroundPoint basePoint)
            {
                HashSet<GroundPoint> wrappedPoints = new HashSet<GroundPoint>();
                List<ExpanderVert> expanderVerts = new List<ExpanderVert>();

                int newVertIndex = grid.Points.Count;
                ExpanderVert firstExpander = new ExpanderVert(basePoint, newVertIndex, expansionDistance);
                expanderVerts.Add(firstExpander);
                wrappedPoints.Add(basePoint);

                float oddExpansions = Mathf.FloorToInt((float)expansions / 2);
                float evenExpansions = Mathf.CeilToInt((float)expansions / 2);

                GroundPoint nextPoint = firstExpander.NeighborA;
                for (int i = 0; i < oddExpansions; i++)
                {
                    wrappedPoints.Add(nextPoint);
                    newVertIndex++;
                    ExpanderVert newPoint = new ExpanderVert(nextPoint, newVertIndex, expansionDistance);
                    expanderVerts.Add(newPoint);
                    nextPoint = wrappedPoints.Contains(newPoint.NeighborA) ? newPoint.NeighborB : newPoint.NeighborA;
                }
                nextPoint = firstExpander.NeighborB;
                for (int i = 0; i < evenExpansions; i++)
                {
                    wrappedPoints.Add(nextPoint);
                    newVertIndex++;
                    ExpanderVert newPoint = new ExpanderVert(nextPoint, newVertIndex, expansionDistance);
                    expanderVerts.Insert(0, newPoint);
                    nextPoint = wrappedPoints.Contains(newPoint.NeighborA) ? newPoint.NeighborB : newPoint.NeighborA;
                }
                return expanderVerts;
            }

            private int GetMaxExpansions(MainGrid grid)
            {
                int borderCount = grid.BorderEdges.Count;
                return (borderCount) - 1;
            }
        }

        private GroundPoint GetClosestBorderPoint(Vector2 gridSpaceCursorPos)
        {
            GroundPoint ret = null;
            float minDist = float.PositiveInfinity;
            IEnumerable<GroundPoint> borderPoints = GetBorderPoints();
            foreach (GroundPoint point in borderPoints)
            {
                float pointDist = (point.Position - gridSpaceCursorPos).sqrMagnitude;
                if(pointDist < minDist)
                {
                    ret = point;
                    minDist = pointDist;
                }
            }
            return ret;
        }

        private IEnumerable<GroundPoint> GetBorderPoints()
        {
            HashSet<GroundPoint> points = new HashSet<GroundPoint>();
            foreach (GroundEdge edge in grid.BorderEdges)
            {
                points.Add(edge.PointA);
                points.Add(edge.PointB);
            }
            return points;
        }

        public IEnumerable<GroundPointBuilder> PointsToAdd { get; }
        public IEnumerable<GroundEdgeBuilder> EdgesToAdd { get; }
    }
    class ExpanderVert
    {
        public GroundPoint BasePoint { get; }
        public GroundPoint NeighborA { get; }
        public GroundPoint NeighborB { get; }

        public GroundPointBuilder NewPoint { get; }
        public GroundEdgeBuilder SpokeEdge { get; }

        public ExpanderVert(GroundPoint basePoint, int newPointId, float expansionDistance)
        {
            BasePoint = basePoint;
            
            GroundEdge[] borderEdges = BasePoint.Edges.Where(edge => edge.IsBorder).ToArray();
            if (borderEdges.Length != 2)
            {
                throw new Exception("ExpanderPoint's base point does not have exactly two border edges.");
            }
            NeighborA = borderEdges[0].GetOtherPoint(basePoint);
            NeighborB = borderEdges[1].GetOtherPoint(basePoint);
            Vector2 position = GetPosition(expansionDistance);
            NewPoint = new GroundPointBuilder(newPointId, position);
            SpokeEdge = new GroundEdgeBuilder(BasePoint.Index, newPointId);
        }

        private Vector2 GetPosition(float expansionDistance)
        {
            Vector2 ab = (NeighborA.Position - NeighborB.Position).normalized;
            Vector2 abCross = new Vector2(ab.y, -ab.x);
            Vector2 newPos = BasePoint.Position + abCross * expansionDistance;
            Vector2 altNewPos = BasePoint.Position + abCross * -expansionDistance;
            if (newPos.sqrMagnitude > altNewPos.sqrMagnitude) // Always want to extrude points farther away from the origin
                return newPos;
            return altNewPos;
        }
    }
}
