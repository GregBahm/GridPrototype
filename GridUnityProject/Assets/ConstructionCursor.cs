using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionCursor : MonoBehaviour
{
    [Range(0, 1)]
    public float UpdateSpeed;

    public float LineOffset = .1f;

    private Vector3[] targets;

    private LineRenderer lineRenderer;

    private void Start()
    {
        lineRenderer = GetComponentInChildren<LineRenderer>();
    }

    private void Update()
    {
        if(targets == null)
        {
            lineRenderer.enabled = false;
            return;
        }
        lineRenderer.enabled = true;
        for (int i = 0; i < targets.Length; i++)
        {
            Vector3 currentPos = lineRenderer.GetPosition(i);
            Vector3 newPos = Vector3.Lerp(currentPos, targets[i] + new Vector3(0, LineOffset, 0), UpdateSpeed * Time.deltaTime * 100);
            lineRenderer.SetPosition(i, newPos);
        }
    }

    public void PlaceCursor(MeshMaking.MeshHitTarget potentialMeshInteraction)
    {
        int oldTargetsCount = targets != null ? targets.Length : 0;
        targets = potentialMeshInteraction.FaceVerts;
        lineRenderer.positionCount = targets.Length;

        // If new points are added, set their start position to the last position of the targets set
        if (oldTargetsCount < targets.Length && oldTargetsCount != 0)
        {
            Vector3 lastOldTarget = lineRenderer.GetPosition(oldTargetsCount - 1);
            for (int i = oldTargetsCount; i < targets.Length; i++)
            {
                lineRenderer.SetPosition(i, lastOldTarget);
            }
        } 
    }
}
