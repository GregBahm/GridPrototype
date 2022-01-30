﻿using Assets.GameGrid;
using Interaction;
using MeshMaking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CityBuildingMain))]
[RequireComponent(typeof(CameraInteraction))]
public class InteractionManager : MonoBehaviour
{
    private static readonly Plane groundPlane = new Plane(Vector3.up, 0);

    [SerializeField]
    private float dragStartDistance = 2;

    [SerializeField]
    private ConstructionCursor cursor;

    private CameraInteraction cameraInteraction;
    private CityBuildingMain gameMain;

    private DragDetector leftDragDetector;
    private DragDetector rightDragDetector;

    public bool GroundModificationMode;

    public int GridExpansions;
    public float GridExpansionDistance = 1;

    public VoxelDesignationType FillType;

    private readonly UndoManager undoManager;

    public Button UndoButton;
    public InteractionManager()
    {
        undoManager = new UndoManager(this);
    }

    private void Start()
    {
        gameMain = GetComponent<CityBuildingMain>();
        cameraInteraction = GetComponent<CameraInteraction>();
        leftDragDetector = new DragDetector(dragStartDistance);
        rightDragDetector = new DragDetector(dragStartDistance);
    }

    private void Update()
    {
        if (GroundModificationMode)
        {
            HandleEasing();
            GridExpander expander = new GridExpander(gameMain.MainGrid, GridExpansions, GridExpansionDistance);
            expander.Update(GetGridSpaceCursorPosition());
            expander.PreviewExpansion();
            if (Input.GetMouseButtonUp(0))
            {
                gameMain.MainGrid.AddToMesh(expander.Points, expander.Edges);
                gameMain.UpdateBaseGrid();
                gameMain.UpdateInteractionGrid();
            }
        }
        else
        {
            MeshHitTarget potentialMeshInteraction = GetPotentialMeshInteraction();
            UpdateCursor(potentialMeshInteraction);
            HandleRightMeshClicks(potentialMeshInteraction);
            HandleLeftMeshClicks(potentialMeshInteraction);
        }
        UpdateCursorHighlight();
        HandleOrbit();
        HandlePan();
        cameraInteraction.HandleMouseScrollwheel();
        UndoButton.interactable = undoManager.CanUndo;
        UndoButton.gameObject.SetActive(undoManager.CanUndo);
    }

    private Vector2 GetGridSpaceCursorPosition()
    {
        Plane plane = new Plane(Vector3.up, 0);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distance;
        plane.Raycast(ray, out distance);
        Vector3 planePosition = ray.GetPoint(distance);
        return new Vector2(planePosition.x, planePosition.z);
    }

    public void SetFillToWalkableRoof()
    {
        FillType = VoxelDesignationType.WalkableRoof;
    }

    public void SetFillToSlantedRoof()
    {
        FillType = VoxelDesignationType.SlantedRoof;
    }

    public void SetFillToPlatform()
    {
        FillType = VoxelDesignationType.Platform;
    }

    public void Undo()
    {
        undoManager.Undo();
    }

    private void UpdateCursor(MeshHitTarget potentialMeshInteraction)
    {
        ConstructionCursor.MouseState state = Input.GetMouseButton(0) ? ConstructionCursor.MouseState.LeftClickDown
            : (Input.GetMouseButton(1) ? ConstructionCursor.MouseState.RightClickDown : ConstructionCursor.MouseState.Hovering);

        cursor.UpdateCursor(potentialMeshInteraction, state);
    }

    private void HandleEasing()
    {
        if(Input.GetMouseButton(1)) // Holding right mouse button
        {
            gameMain.MainGrid.DoEase();
            gameMain.UpdateBaseGrid();
            gameMain.UpdateInteractionGrid();
        }
    }

    private void UpdateCursorHighlight()
    {
        Vector3 cursorPos = GetGroundPositionAtScreenpoint(Input.mousePosition);
        Shader.SetGlobalVector("_DistToCursor", cursorPos);
    }

    private void HandlePan()
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
            rightDragDetector.IsDragging = false;
        }
    }

    private void HandleOrbit()
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
            leftDragDetector.IsDragging = false;
        }
    }

    private MeshHitTarget GetPotentialMeshInteraction()
    {
        if(rightDragDetector.IsDragging ||
            leftDragDetector.IsDragging)
        {
            return null;
        }

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            return gameMain.InteractionMesh.GetHitTarget(hit.triangleIndex);
        }
        return null;
    }

    private void HandleRightMeshClicks(MeshHitTarget hitInfo)
    {
        if (Input.GetMouseButtonUp(1) && hitInfo != null && hitInfo.SourceCell != null)
        {
            RegisterDesignationUndo(hitInfo.SourceCell);
            SetDesignation(hitInfo.SourceCell, VoxelDesignationType.Empty);
        }
    }

    private void RegisterDesignationUndo(DesignationCell cell)
    {
        undoManager.RegisterDesignationPlacement(cell);
    }

    private void HandleLeftMeshClicks(MeshHitTarget hitInfo)
    {
        if (Input.GetMouseButtonUp(0) 
            && hitInfo != null 
            && hitInfo.TargetCell != null
            && !hitInfo.TargetCell.GroundPoint.IsBorder)
        {
            RegisterDesignationUndo(hitInfo.TargetCell);
            SetDesignation(hitInfo.TargetCell, FillType);
        }
    }

    public void SetDesignation(DesignationCell cell, VoxelDesignationType type)
    {
        cell.Designation = type;
        gameMain.UpdateInteractionGrid();
        gameMain.UpdateVoxelVisuals(cell);
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
