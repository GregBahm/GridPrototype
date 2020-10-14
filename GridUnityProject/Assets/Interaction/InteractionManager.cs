using Interaction;
using MeshMaking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(GameMain))]
[RequireComponent(typeof(GridModification))]
[RequireComponent(typeof(CameraInteraction))]
public class InteractionManager : MonoBehaviour
{
    private GridModification gridModification;
    private static readonly Plane groundPlane = new Plane(Vector3.up, 0);

    [SerializeField]
    private float dragStartDistance = 2;

    private CameraInteraction cameraInteraction;
    private GameMain gameMain;

    private DragDetector leftDragDetector;
    private DragDetector rightDragDetector;

    private void Start()
    {
        gameMain = GetComponent<GameMain>();
        gridModification = GetComponent<GridModification>();
        cameraInteraction = GetComponent<CameraInteraction>();
        leftDragDetector = new DragDetector(dragStartDistance);
        rightDragDetector = new DragDetector(dragStartDistance);
    }

    private void Update()
    {
        //DoEasing();
        //gridModification.DoGridModification();
        IHitTarget potentialMeshInteraction = GetPotentialMeshInteraction();
        UpdateCursorHighlight();
        HandleOrbit(potentialMeshInteraction);
        HandlePan(potentialMeshInteraction);
        cameraInteraction.HandleMouseScrollwheel();
    }

    private void DoEasing()
    {
        if(Input.GetMouseButton(1))
        {
            gameMain.MainGrid.DoEase();
        }
    }

    private void UpdateCursorHighlight()
    {
        Vector3 cursorPos = GetGroundPositionAtScreenpoint(Input.mousePosition);
        Shader.SetGlobalVector("_DistToCursor", cursorPos);
    }

    private void HandlePan(IHitTarget potentialMeshInteraction)
    {
        if (Input.GetMouseButton(1))
        {
            if (rightDragDetector.IsDragging)
            {
                cameraInteraction.ContinuePan();
            }
            else
            {
                if (Input.GetMouseButtonDown(1))
                {
                    rightDragDetector.DragStartPos = Input.mousePosition;
                    cameraInteraction.StartPan();
                }
                else
                {
                    rightDragDetector.UpdateIsDragging();
                }
            }
        }
        else
        {
            HandleRightMeshClicks(potentialMeshInteraction);
            rightDragDetector.IsDragging = false;
        }
    }

    private void HandleOrbit(IHitTarget potentialMeshInteraction)
    {
        if (Input.GetMouseButton(0))
        {
            if(leftDragDetector.IsDragging)
            {
                cameraInteraction.ContinueOrbit();
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    leftDragDetector.DragStartPos = Input.mousePosition;
                    cameraInteraction.StartOrbit();
                }
                else
                {
                    leftDragDetector.UpdateIsDragging();
                }
            }
        }
        else
        {
            HandleLeftMeshClicks(potentialMeshInteraction);
            leftDragDetector.IsDragging = false;
        }
    }

    private IHitTarget GetPotentialMeshInteraction()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            return gameMain.InteractionMesh.GetHitTarget(hit.triangleIndex);
        }
        return null;
    }

    private void HandleRightMeshClicks(IHitTarget hitInfo)
    {
        if (Input.GetMouseButtonUp(1) && !rightDragDetector.IsDragging && hitInfo != null && hitInfo.SourceCell != null)
        {
            hitInfo.SourceCell.Filled = false;
            gameMain.UpdateInteractionGrid();
        }
    }

    private void HandleLeftMeshClicks(IHitTarget hitInfo)
    {
        if (Input.GetMouseButtonUp(0) && !leftDragDetector.IsDragging && hitInfo != null && hitInfo.TargetCell != null)
        {
            hitInfo.TargetCell.Filled = true;
            gameMain.UpdateInteractionGrid();
        }
    }

    public static Vector3 GetGroundPositionAtScreenpoint(Vector3 screenPoint)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        float enter;
        groundPlane.Raycast(ray, out enter);
        return ray.GetPoint(enter);
    }

    private class DragDetector
    {
        public Vector3 DragStartPos { get; set; }
        public bool IsDragging { get; set; }
        private readonly float dragStartDistance;

        public DragDetector(float dragStartDistance)
        {
            this.dragStartDistance = dragStartDistance;
        }

        public void UpdateIsDragging()
        {
            IsDragging = (DragStartPos - Input.mousePosition).magnitude > dragStartDistance;
        }
    }
}
