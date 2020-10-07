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
        GridEdge closestEdge = GetClosestEdge(gameMain.MainGrid.BorderEdges, mouseGroundPos);
        if (closestEdge != null)
        {
            GridExpansion expansion = new GridExpansion(closestEdge, expansionChainLength, expansionDistance);
            PreviewExpansion(expansion);
            if(Input.GetMouseButtonUp(0))
            {
                expansion.ApplyToGrid(gameMain.MainGrid);
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

    private void DrawEdge(GridEdge edge)
    {
        Vector3 pointAPos = new Vector3(edge.PointA.Position.x, 0, edge.PointA.Position.y);
        Vector3 pointBPos = new Vector3(edge.PointB.Position.x, 0, edge.PointB.Position.y);
        Debug.DrawLine(pointAPos, pointBPos, Color.cyan);
    }

    private class GridExpansion
    {
        private readonly GridEdge mainEdge;
        private readonly float expansionDistance;
        private readonly float expansionChainLength;
        public IReadOnlyList<GridExpansionPoint> ExpansionPoints { get; }
        
        public GridExpansion(GridEdge mainEdge, int expansionChainLength, float expansionDistance)
        {
            this.mainEdge = mainEdge;
            this.expansionChainLength = expansionChainLength;
            this.expansionDistance = expansionDistance;
            ExpansionPoints = GetExpansionChain().ToList();
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

        internal void ApplyToGrid(MainGrid mainGrid)
        {
            GridPointBuilder[] points = GetGridPoints(mainGrid).ToArray();
            GridEdgeBuilder[] edges = GetGridEdges(mainGrid, points).ToArray();
            mainGrid.AddToMesh(points, edges);
        }

        private IEnumerable<GridEdgeBuilder> GetGridEdges(MainGrid mainGrid, GridPointBuilder[] points)
        {
            yield return new GridEdgeBuilder(ExpansionPoints[0].BasePoint.Index, points[0].Index);
            for (int i = 1; i < ExpansionPoints.Count; i++)
            {
                yield return new GridEdgeBuilder(ExpansionPoints[i].BasePoint.Index, points[i].Index);
                yield return new GridEdgeBuilder(points[i - 1].Index, points[i].Index);
            }
        }

        private IEnumerable<GridPointBuilder> GetGridPoints(MainGrid mainGrid)
        {
            int i = mainGrid.Points.Count;
            foreach (GridExpansionPoint expansion in ExpansionPoints)
            {
                yield return new GridPointBuilder(i, expansion.ExpandedPos);
                i++;
            }
        }
    }

    private class GridExpansionPoint
    {
        private readonly float expansionDistance;
        private readonly GridEdge baseEdge;
        private readonly GridEdge adjacentEdge;

        public GridPoint BasePoint { get; }
        public Vector2 ExpandedPos { get; }

        public GridExpansionPoint(GridEdge baseEdge, GridPoint basePoint, float expansionDistance)
        {
            this.baseEdge = baseEdge;
            BasePoint = basePoint;
            this.expansionDistance = expansionDistance;
            adjacentEdge = GetAdjacentBorder();
            ExpandedPos = GetExpandedPos();
        }

        private Vector2 GetExpandedPos()
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

        private GridEdge GetAdjacentBorder()
        {
            return BasePoint.Edges.First(pointEdge => pointEdge != baseEdge && pointEdge.IsBorder);
        }
    }

    private GridEdge GetClosestEdge(IEnumerable<GridEdge> borderEdges, Vector3 mouseGroundPos)
    {
        Vector2 gridSpacePos = new Vector2(mouseGroundPos.x, mouseGroundPos.z);
        float closests = float.PositiveInfinity;
        GridEdge ret = null;
        foreach (GridEdge edge in borderEdges)
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

    private float? GetDistance(GridEdge edge, Vector2 gridSpacePos)
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