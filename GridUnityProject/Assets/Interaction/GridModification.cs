using GameGrid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(GameMain))]
public class GridModification : MonoBehaviour
{
    private MainGrid mainGrid;

    private void Start()
    {
        mainGrid = GetComponent<GameMain>().MainGrid;
    }

    public void DoGridModification()
    {
        Vector3 mouseGroundPos = InteractionManager.GetGroundPositionAtScreenpoint(Input.mousePosition);
        GridEdge closestEdge = GetClosestEdge(mainGrid.BorderEdges, mouseGroundPos);
        if(closestEdge != null)
        {
            Vector3 pointAPos = new Vector3(closestEdge.PointA.Position.x, 0, closestEdge.PointA.Position.y);
            Vector3 pointBPos = new Vector3(closestEdge.PointB.Position.x, 0, closestEdge.PointB.Position.y);
            Debug.DrawLine(pointAPos, mouseGroundPos);
            Debug.DrawLine(pointBPos, mouseGroundPos);
            // Then what, hu?
        }
    }

    private GridEdge GetClosestEdge(IEnumerable<GridEdge> borderEdges, Vector3 mouseGroundPos)
    {
        Vector2 gridSpacePos = new Vector2(mouseGroundPos.x, mouseGroundPos.z);
        float closests = float.PositiveInfinity;
        GridEdge ret = null;
        foreach (GridEdge edge in borderEdges)
        {
            float dist = GetDistance(edge, gridSpacePos);
            if(dist < closests)
            {
                closests = dist;
                ret = edge;
            }
        }
        return ret;
    }

    private float GetDistance(GridEdge edge, Vector2 gridSpacePos)
    {
        Vector2 edgeLine = (edge.PointA.Position - edge.PointB.Position).normalized;
        Vector2 toMouse = (edge.PointA.Position - gridSpacePos).normalized;
        Vector2 toQuad = (edge.PointA.Position - edge.Quads.First().Center).normalized;
        float mouseDot = Vector2.Dot(edgeLine, toMouse)

    }
}