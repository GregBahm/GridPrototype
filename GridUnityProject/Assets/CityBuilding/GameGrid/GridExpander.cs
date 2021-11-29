using GameGrid;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.GameGrid
{
    class GridExpander
    {
        public IEnumerable<GroundPointBuilder> Points { get; }
        public IEnumerable<GroundEdgeBuilder> Edges { get; }

        public GridExpander(MainGrid grid, float angleThreshold)
        {
            ExpanderPoint[] expanderPoints = GetExpanderPoints(grid, angleThreshold).ToArray();

            List<GroundPointBuilder> points = new List<GroundPointBuilder>();
            List<GroundEdgeBuilder> edges = new List<GroundEdgeBuilder>();
            for (int i = 0; i < expanderPoints.Length; i++)
            {
                int nextIndex = (i + 1) % expanderPoints.Length;
                ExpanderPoint current = expanderPoints[i];
                ExpanderPoint next = expanderPoints[nextIndex];
                points.AddRange(current.Points);
                edges.AddRange(current.SpokeEdges);

                GroundEdgeBuilder rimEdge = new GroundEdgeBuilder(current.Points.Last().Index, next.Points.First().Index);
                edges.Add(rimEdge);
            }
            Points = points;
            Edges = edges;
        }

        public void PreviewExpansion(MainGrid grid)
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

        private IEnumerable<ExpanderPoint> GetExpanderPoints(MainGrid grid, float angleThreshold)
        {
            GroundEdge[] borderEdges = grid.BorderEdges.ToArray();
            
            GroundEdge currentEdge = borderEdges.First();
            GroundPoint currentPoint = currentEdge.PointA;
            int pointIndex = grid.Points.Count;
            ExpanderPoint newPoint = new ExpanderPoint(currentEdge, currentPoint, 1, angleThreshold, grid.Points.Count);
            yield return newPoint;
            for (int i = 1; i < borderEdges.Length; i++)
            {
                pointIndex += newPoint.Points.Count;
                currentEdge = GetOtherBorderEdge(currentEdge, currentPoint);
                currentPoint = currentEdge.GetOtherPoint(currentPoint);
                newPoint = new ExpanderPoint(currentEdge, currentPoint, 1, angleThreshold, pointIndex);
                yield return newPoint;
            }
        }

        private GroundEdge GetOtherBorderEdge(GroundEdge currentEdge, GroundPoint currentPoint)
        {
            return currentPoint.Edges.First(edge => edge.IsBorder && edge != currentEdge);
        }

        private class ExpanderPoint
        {
            private readonly float angleThreshold;
            private readonly GroundEdge borderEdgeA;
            private readonly GroundEdge borderEdgeB;

            private readonly int vertStartIndex;

            public GroundPoint BasePoint { get; }
            public bool ExceedsAngleThreshold { get; }

            private readonly Vector2 expandedPoint;
            private readonly Vector2 perpendicularA;
            private readonly Vector2 perpendicularB;

            public ReadOnlyCollection<GroundPointBuilder> Points { get; }
            public ReadOnlyCollection<GroundEdgeBuilder> SpokeEdges { get; }

            public ExpanderPoint(GroundEdge baseEdge, GroundPoint basePoint, float expansionDistance, float angleThreshold, int vertStartIndex)
            {
                BasePoint = basePoint;
                this.angleThreshold = angleThreshold;
                this.vertStartIndex = vertStartIndex;

                GroundEdge[] borderEdges = BasePoint.Edges.Where(edge => edge.IsBorder).ToArray();
                if(borderEdges.Length != 2)
                {
                    throw new Exception("ExpanderPoint's base point does not have exactly two border edges.");
                }
                if(borderEdges[0] != baseEdge && borderEdges[1] != baseEdge)
                {
                    throw new Exception("ExpanderPoint's base edge is not one of its border edges.");
                }
                borderEdgeA = baseEdge;
                borderEdgeB = borderEdges[0] == baseEdge ? borderEdges[1] : borderEdges[0];
                ExceedsAngleThreshold = GetExceedsAngleThreshold();

                ExpansionComponet componetA = new ExpansionComponet(borderEdgeA, basePoint);
                ExpansionComponet componetB = new ExpansionComponet(borderEdgeB, basePoint);
                expandedPoint = ((componetA.Offset + componetB.Offset) / 2).normalized * expansionDistance + BasePoint.Position;
                perpendicularA = componetA.PointPos;
                perpendicularB = componetB.PointPos;
                Points = GetPoints().ToList().AsReadOnly();
                SpokeEdges = GetSpokeEdges().ToList().AsReadOnly();

            }

            private class ExpansionComponet
            {
                public Vector2 Offset { get; }
                public Vector2 PointPos { get; }

                public ExpansionComponet(GroundEdge edge, GroundPoint basePoint)
                {
                    Vector2 quadCenter = edge.Quads.First().Center;
                    Vector2 edgeSlope = (edge.GetOtherPoint(basePoint).Position - basePoint.Position).normalized;
                    Vector2 perpendicular = new Vector2(-edgeSlope.y, edgeSlope.x);
                    Vector2 toCenter = (basePoint.Position - quadCenter).normalized;
                    float dotToCenter = Vector2.Dot(perpendicular, toCenter);

                    Offset = dotToCenter > 0 ? perpendicular : -perpendicular;
                    PointPos = basePoint.Position + Offset;
                }
            }


            public IEnumerable<GroundPointBuilder> GetPoints()
            {
                if (ExceedsAngleThreshold)
                {
                    yield return new GroundPointBuilder(vertStartIndex, perpendicularA);
                    yield return new GroundPointBuilder(vertStartIndex + 1, expandedPoint);
                    yield return new GroundPointBuilder(vertStartIndex + 2, perpendicularB);
                }
                else
                {
                    yield return new GroundPointBuilder(vertStartIndex, expandedPoint);
                }
            }

            public IEnumerable<GroundEdgeBuilder> GetSpokeEdges()
            {
                yield return new GroundEdgeBuilder(vertStartIndex, BasePoint.Index);
                if (ExceedsAngleThreshold)
                {
                    yield return new GroundEdgeBuilder(vertStartIndex + 2, BasePoint.Index);
                    yield return new GroundEdgeBuilder(vertStartIndex, vertStartIndex + 1);
                    yield return new GroundEdgeBuilder(vertStartIndex + 1, vertStartIndex + 2);
                }
            }

            private bool GetExceedsAngleThreshold()
            {
                Vector2 basePos = BasePoint.Position;
                Vector2 adjacentA = borderEdgeA.GetOtherPoint(BasePoint).Position;
                Vector2 adjacentB = borderEdgeB.GetOtherPoint(BasePoint).Position;

                Vector2 toA = (basePos - adjacentA).normalized;
                Vector2 toB = (basePos - adjacentB).normalized;
                return Vector2.Dot(toA, toB) > angleThreshold;
            }
        }
    }
}
