using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionCursor : MonoBehaviour
{

    private LineRenderer lineRenderer;

    private void Start()
    {
        lineRenderer = GetComponentInChildren<LineRenderer>();
    }

    public void PlaceCursor(MeshMaking.MeshHitTarget potentialMeshInteraction)
    {
        lineRenderer.positionCount = potentialMeshInteraction.FaceVerts.Length;
        lineRenderer.SetPositions(potentialMeshInteraction.FaceVerts);
    }
}
