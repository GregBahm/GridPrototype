using Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(MeshInteraction))]
[RequireComponent(typeof(CameraInteraction))]
public class InteractionManager : MonoBehaviour
{
    [SerializeField]
    private float dragStartDistance;

    private MeshInteraction meshInteraction;
    private CameraInteraction cameraInteraction;


    private Vector3 leftMouseDownPosition;
    private bool isDragging;
    private bool isPanning;

    private void Start()
    {
        meshInteraction = GetComponent<MeshInteraction>();
        cameraInteraction = GetComponent<CameraInteraction>();
    }

    private void Update()
    {
        UpdateCursorHighlight();
        HandleOrbit();
        HandlePan();
        HandleShowSelection();
        cameraInteraction.HandleMouseScrollwheel();
    }

    private void HandleShowSelection()
    {
        if(isDragging || isPanning)
        {
            meshInteraction.HideSelectionMesh();
        }
        else
        {
            meshInteraction.ShowSelectionMesh();
        }
    }

    private void UpdateCursorHighlight()
    {
        Vector3 cursorPos = CameraInteraction.GetPlanePositionAtScreenpoint(Input.mousePosition);
        Shader.SetGlobalVector("_DistToCursor", cursorPos);
    }

    private void HandlePan()
    {
        if(Input.GetMouseButton(1))
        {
            if(Input.GetMouseButtonDown(1))
            {
                cameraInteraction.StartPan();
            }
            cameraInteraction.ContinuePan();
        }
        isPanning = Input.GetMouseButton(1);
    }

    private void HandleOrbit()
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
            if (Input.GetMouseButtonUp(0) &&  !isDragging)
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
