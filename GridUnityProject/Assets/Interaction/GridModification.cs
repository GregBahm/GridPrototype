using GameGrid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(GameMain))]
public class GridModification : MonoBehaviour
{
    [SerializeField]
    private int expansionChainLength = 2;
    [SerializeField]
    private int expansionDistance = 1;

    private GameMain gameMain;

    private void Start()
    {
        gameMain = GetComponent<GameMain>();
    }

    public void DoGridModification()
    {
        Vector3 mouseGroundPos = InteractionManager.GetGroundPositionAtScreenpoint(Input.mousePosition);
        GroundEdge closestEdge = GetClosestEdge(gameMain.MainGrid.BorderEdges, mouseGroundPos);
        if (closestEdge != null)
        {
            GridExpansion expansion = new GridExpansion(gameMain.MainGrid, closestEdge, expansionChainLength, expansionDistance);
            PreviewExpansion(expansion);
            if(Input.GetMouseButtonUp(0))
            {
                gameMain.MainGrid.AddToMesh(expansion.PotentialPoints, expansion.PotentialEdges);
                gameMain.UpdateInteractionGrid();
            }
        }
    }

    private void PreviewExpansion(GridExpansion expansion)
    {
        DrawEdge(expansion.ExpansionPoints[0].ExpandedPos, expansion.ExpansionPoints[0].BasePoint.Position, 0);
        for (int i = 1; i < expansion.ExpansionPoints.Count; i++)
        {
            float param = (float)i / expansion.ExpansionPoints.Count;
            DrawEdge(expansion.ExpansionPoints[i].ExpandedPos, expansion.ExpansionPoints[i].BasePoint.Position, param);
            DrawEdge(expansion.ExpansionPoints[i].ExpandedPos, expansion.ExpansionPoints[i - 1].ExpandedPos, param);
        }
    }

    private void DrawEdge(Vector2 pointA, Vector2 pointB, float param)
    {
        Color color = Color.Lerp(Color.cyan, Color.magenta, param);
        Vector3 pointAPos = new Vector3(pointA.x, 0, pointA.y);
        Vector3 pointBPos = new Vector3(pointB.x, 0, pointB.y);
        Debug.DrawLine(pointAPos, pointBPos, color);
    }

    private class GridExpansion
    {
        private readonly GroundEdge mainEdge;
        private readonly float expansionDistance;
        private readonly float expansionChainLength;
        public IReadOnlyList<GridExpansionPoint> ExpansionPoints { get; }

        public IEnumerable<GroundPointBuilder> PotentialPoints { get; }
        public IEnumerable<GroundEdgeBuilder> PotentialEdges { get; }
        
        public GridExpansion(MainGrid mainGrid, GroundEdge mainEdge, int expansionChainLength, float expansionDistance)
        {
            this.mainEdge = mainEdge;
            this.expansionChainLength = expansionChainLength;
            this.expansionDistance = expansionDistance;
            ExpansionPoints = GetExpansionChain().ToList();

            GroundPointBuilder[] points = GetGridPoints(mainGrid).ToArray();
            PotentialPoints = points;
            PotentialEdges = GetGridEdges(mainGrid, points).ToArray();
        }

        private IEnumerable<GridExpansionPoint> GetExpansionChain()
        {
            GridExpansionPoint starterA = new GridExpansionPoint(mainEdge, mainEdge.PointA, expansionDistance);
            GridExpansionPoint starterB = new GridExpansionPoint(mainEdge, mainEdge.PointB, expansionDistance);
            List<GridExpansionPoint> chain = new List<GridExpansionPoint>() { starterA, starterB };
            for (int i = 0; i < expansionChainLength; i++)
            {
                starterA = starterA.GetNextExpansionEdge();
                starterB = starterB.GetNextExpansionEdge();
                chain.Insert(0, starterA);
                chain.Add(starterB);
            }
            return chain;
        }

        private IEnumerable<GroundEdgeBuilder> GetGridEdges(MainGrid mainGrid, GroundPointBuilder[] spokePoints)
        {
            for (int i = 0; i < ExpansionPoints.Count; i++)
            {
                foreach (GroundEdgeBuilder edge in ExpansionPoints[i].CreateSpokeEdges(spokePoints[i].Index))
                {
                    yield return edge;
                }
                if(i != 0)
                {
                    yield return new GroundEdgeBuilder(spokePoints[i - 1].Index, spokePoints[i].Index);
                }
            }
        }

        private IEnumerable<GroundPointBuilder> GetGridPoints(MainGrid mainGrid)
        {
            int i = mainGrid.Points.Count;
            foreach (GridExpansionPoint expansion in ExpansionPoints)
            {
                yield return new GroundPointBuilder(i, expansion.ExpandedPos);
                i++;
            }
        }
    }

    private class GridExpansionPoint
    {
        private readonly float expansionDistance;
        private readonly GroundEdge baseEdge;
        private readonly GroundEdge adjacentEdge;

        public GroundPoint BasePoint { get; }
        public Vector2 ExpandedPos { get; }
        public bool IsConvex { get; }

        public GridExpansionPoint(GroundEdge baseEdge, GroundPoint basePoint, float expansionDistance)
        {
            this.baseEdge = baseEdge;
            BasePoint = basePoint;
            this.expansionDistance = expansionDistance;
            adjacentEdge = GetAdjacentBorder();
            IsConvex = GetIsConvex();
            ExpandedPos = IsConvex ? GetConvexExpandedPos() : GetNonConvexExpandedPos();
        }

        private bool GetIsConvex()
        {
            return false;
            Vector2 basePos = BasePoint.Position;
            Vector2 adjacentA = baseEdge.GetOtherPoint(BasePoint).Position;
            Vector2 adjacentB = adjacentEdge.GetOtherPoint(BasePoint).Position;

            Vector2 toA = (basePos - adjacentA).normalized;
            Vector2 toB = (basePos - adjacentB).normalized;
            return Vector2.Dot(toA, toB) > .0f;
        }

        private Vector2 GetConvexExpandedPos()
        {
            Vector2 adjacentA = baseEdge.GetOtherPoint(BasePoint).Position;
            Vector2 adjacentB = adjacentEdge.GetOtherPoint(BasePoint).Position;
            Vector2 mid = (adjacentA + adjacentB) / 2;
            Vector2 offset = (mid - BasePoint.Position).normalized * 1.5f;
            return BasePoint.Position + offset;
        }

        private Vector2 GetNonConvexExpandedPos()
        {
            Vector2 toQuadA = (baseEdge.MidPoint - baseEdge.Quads.First().Center).normalized;
            Vector2 toQuadB = (adjacentEdge.MidPoint - adjacentEdge.Quads.First().Center).normalized;
            Vector2 opposite = (toQuadA + toQuadB) / 2;
            return BasePoint.Position + opposite;
        }

        public GridExpansionPoint GetNextExpansionEdge()
        {
            return new GridExpansionPoint(adjacentEdge, adjacentEdge.GetOtherPoint(BasePoint), expansionDistance);
        }

        private GroundEdge GetAdjacentBorder()
        {
            return BasePoint.Edges.First(pointEdge => pointEdge != baseEdge && pointEdge.IsBorder);
        }

        public IEnumerable<GroundEdgeBuilder> CreateSpokeEdges(int spokeIndex)
        {
            if(IsConvex)
            {
                GroundPoint startA = baseEdge.GetOtherPoint(BasePoint);
                GroundPoint startB = adjacentEdge.GetOtherPoint(BasePoint);
                yield return new GroundEdgeBuilder(startA.Index, spokeIndex);
                yield return new GroundEdgeBuilder(startB.Index, spokeIndex);
            }
            else
            {
                yield return new GroundEdgeBuilder(BasePoint.Index, spokeIndex);
            }
        }
    }

    private GroundEdge GetClosestEdge(IEnumerable<GroundEdge> borderEdges, Vector3 mouseGroundPos)
    {
        Vector2 gridSpacePos = new Vector2(mouseGroundPos.x, mouseGroundPos.z);
        float closests = float.PositiveInfinity;
        GroundEdge ret = null;
        foreach (GroundEdge edge in borderEdges)
        {
            float? dist = GetDistance(edge, gridSpacePos);
            if (dist.HasValue && dist.Value < closests)
            {
                closests = dist.Value;
                ret = edge;
            }
        }
        return ret;
    }

    private float? GetDistance(GroundEdge edge, Vector2 gridSpacePos)
    {
        Vector2 edgeLine = (edge.PointA.Position - edge.PointB.Position).normalized;
        Vector2 toMouse = (edge.PointA.Position - gridSpacePos).normalized;
        Vector2 toQuad = (edge.PointA.Position - edge.Quads.First().Center).normalized;
        float mouseDot = Vector2.Dot(edgeLine, toMouse);
        float quadDot = Vector2.Dot(toQuad, toMouse);
        if (mouseDot > quadDot)
        {
            return FindDistanceToSegment(gridSpacePos, edge.PointA.Position, edge.PointB.Position);
        }
        return null;
    }

    private float FindDistanceToSegment(
    Vector2 target, Vector2 segStart, Vector2 segEnd)
    {
        Vector2 segLength = segEnd - segStart;
        float param = ((target.x - segStart.x) * segLength.x + (target.y - segStart.y) * segLength.y) /
            (segLength.x * segLength.x + segLength.y * segLength.y);

        if (param < 0)
        {
            return (target - segStart).magnitude;
        }
        if (param > 1)
        {
            return (target - segEnd).magnitude;
        }
        return (target - Vector2.Lerp(segStart, segEnd, param)).magnitude;
    }
}