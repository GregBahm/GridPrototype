using Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshInteraction))]
[RequireComponent(typeof(CameraInteraction))]
public class InteractionManager : MonoBehaviour
{
    [SerializeField]
    private float dragStartDistance;

    private MeshInteraction meshInteraction;
    private CameraInteraction cameraInteraction;

    private void Start()
    {
        meshInteraction = GetComponent<MeshInteraction>();
        cameraInteraction = GetComponent<CameraInteraction>();
    }

    private void Update()
    {
        UpdateCursorHighlight();
        HandleLeftMouse();
        cameraInteraction.HandleMouseScrollwheel();
        HandleRightMouse();
    }

    private void UpdateCursorHighlight()
    {
        Vector3 cursorPos = CameraInteraction.GetPlanePositionAtScreenpoint(Input.mousePosition);
        Shader.SetGlobalVector("_DistToCursor", cursorPos);
    }

    private void HandleRightMouse()
    {
        if(Input.GetMouseButton(1))
        {
            if(Input.GetMouseButtonDown(1))
            {
                cameraInteraction.StartPan();
            }
            cameraInteraction.ContinuePan();
        }
    }

    private Vector3 leftMouseDownPosition;
    private bool isDragging;

    private void HandleLeftMouse()
    {
        if (Input.GetMouseButton(0))
        {
            if(isDragging)
            {
                meshInteraction.HideSelectionMesh();
                cameraInteraction.ContinueOrbit();
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    leftMouseDownPosition = Input.mousePosition;
                    cameraInteraction.StartOrbit();
                }
                else
                {
                    isDragging = GetIsDragging();
                }
            }
        }
        else
        {
            if (Input.GetMouseButtonUp(0) && !isDragging)
            {
                meshInteraction.PlaceMesh();
            }
            isDragging = false;
            meshInteraction.ShowSelectionMesh();
        }
    }

    private bool GetIsDragging()
    {
        return (leftMouseDownPosition - Input.mousePosition).magnitude > dragStartDistance; 
    }
}
