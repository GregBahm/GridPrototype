using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionCursor : MonoBehaviour
{
    [Range(0, 1)]
    public float UpdateSpeed;

    [Range(0, 1)]
    public float ShrinkAmount = .9f;

    public float LineOffset = .1f;

    private float baseLineWidth;
    private Vector3 targetCenter;
    private Vector3[] targets;

    private LineRenderer lineRenderer;

    private bool hideCursor;
    private MouseState mouseState;

    private void Start()
    {
        lineRenderer = GetComponentInChildren<LineRenderer>();
        baseLineWidth = lineRenderer.widthMultiplier;
    }

    private void UpdateCursorVisuals(Vector3 normal)
    {
        if (targets == null)
        {
            lineRenderer.enabled = false;
            return;
        }
        lineRenderer.enabled = true;
        for (int i = 0; i < targets.Length; i++)
        {
            Vector3 currentPos = lineRenderer.GetPosition(i);
            Vector3 posTarget = GetPosTarget(targets[i], normal);
            Vector3 newPos = Vector3.Lerp(currentPos, posTarget, UpdateSpeed * Time.deltaTime * 100);
            lineRenderer.SetPosition(i, newPos);
        }
        float widthMultiplierTarget = hideCursor ? 0 : baseLineWidth;
        lineRenderer.widthMultiplier = Mathf.Lerp(lineRenderer.widthMultiplier, widthMultiplierTarget, UpdateSpeed * Time.deltaTime * 100);
    }

    private Vector3 GetPosTarget(Vector3 rawTarget, Vector3 normal)
    {
        Vector3 ret = GetMouseModifiedPosTarget(rawTarget);
        ret = Vector3.Lerp(targetCenter, ret, ShrinkAmount);
        float lineOffset = hideCursor ? LineOffset * 3 : LineOffset;
        ret += normal * lineOffset;
        return ret;
    }

    private Vector3 GetMouseModifiedPosTarget(Vector3 rawTarget)
    {
        switch (mouseState)
        {
            case MouseState.LeftClickDown:
                return rawTarget + (targetCenter - rawTarget) * .1f;
            case MouseState.RightClickDown:
                return rawTarget + (rawTarget - targetCenter) * .1f;
            case MouseState.Hovering:
            default:
                return rawTarget;
        }
    }

    public void UpdateCursor(MeshMaking.MeshHitTarget meshHitTarget, MouseState mouseState)
    {
        this.mouseState = mouseState;
        hideCursor = meshHitTarget == null;
        if(!hideCursor)
        {
            PlaceCursor(meshHitTarget);
        }
        UpdateCursorVisuals(meshHitTarget?.Normal ?? Vector3.up);
    }

    private void PlaceCursor(MeshMaking.MeshHitTarget meshHitTarget)
    {
        int oldTargetsCount = targets != null ? targets.Length : 0;
        targetCenter = meshHitTarget.Center;
        targets = meshHitTarget.FaceVerts;
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

    public enum MouseState
    {
        Hovering,
        LeftClickDown,
        RightClickDown,
    }
}
