using Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(GridModification))]
[RequireComponent(typeof(CameraInteraction))]
public class InteractionManager : MonoBehaviour
{
    private GridModification gridModification;
    private static readonly Plane groundPlane = new Plane(Vector3.up, 0);

    [SerializeField]
    private float dragStartDistance = 2;

    //private MeshInteraction meshInteraction;
    private CameraInteraction cameraInteraction;


    private Vector3 leftMouseDownPosition;
    private bool isDragging;
    private bool isPanning;

    private void Start()
    {
        //meshInteraction = GetComponent<MeshInteraction>();
        gridModification = GetComponent<GridModification>();
        cameraInteraction = GetComponent<CameraInteraction>();
    }

    private void Update()
    {
        gridModification.DoGridModification();
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
            //meshInteraction.HideSelectionMesh();
        }
        else
        {
            //meshInteraction.ShowSelectionMesh();
        }
    }

    private void UpdateCursorHighlight()
    {
        Vector3 cursorPos = GetGroundPositionAtScreenpoint(Input.mousePosition);
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
                //meshInteraction.HideSelectionMesh();
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
                //meshInteraction.PlaceMesh();
            }
            isDragging = false;
            //meshInteraction.ShowSelectionMesh();
        }
    }

    private bool GetIsDragging()
    {
        return (leftMouseDownPosition - Input.mousePosition).magnitude > dragStartDistance; 
    }

    public static Vector3 GetGroundPositionAtScreenpoint(Vector3 screenPoint)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        float enter;
        groundPlane.Raycast(ray, out enter);
        return ray.GetPoint(enter);
    }
}
