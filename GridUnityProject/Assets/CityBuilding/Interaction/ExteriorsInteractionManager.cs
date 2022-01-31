using MeshMaking;
using UnityEngine;

public class ExteriorsInteractionManager : MonoBehaviour
{
    private static readonly Plane groundPlane = new Plane(Vector3.up, 0);

    private CityBuildingMain gameMain;

    public VoxelDesignationType FillType;

    [SerializeField]
    private ConstructionCursor cursor;

    private void Start()
    {
        gameMain = GetComponent<CityBuildingMain>();
    }

    public void ProceedWithUpdate(bool wasDragging)
    {
        MeshHitTarget potentialMeshInteraction = GetPotentialMeshInteraction();
        UpdateCursor(potentialMeshInteraction);
        if(!wasDragging)
        {
            HandleRightMeshClicks(potentialMeshInteraction);
            HandleLeftMeshClicks(potentialMeshInteraction);
        }
        UpdateCursorHighlight();
    }

    public void SetFillToWalkableRoof() { FillType = VoxelDesignationType.WalkableRoof; }
    public void SetFillToSlantedRoof() { FillType = VoxelDesignationType.SlantedRoof; }
    public void SetFillToPlatform() { FillType = VoxelDesignationType.Platform; }


    public void SetDesignation(DesignationCell cell, VoxelDesignationType type)
    {
        cell.Designation = type;
        gameMain.UpdateInteractionGrid();
        gameMain.UpdateVoxelVisuals(cell);
    }

    private void RegisterDesignationUndo(DesignationCell cell)
    {
        gameMain.UndoManager.RegisterDesignationPlacement(this, cell);
    }


    private MeshHitTarget GetPotentialMeshInteraction()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            return gameMain.InteractionMesh.GetHitTarget(hit.triangleIndex);
        }
        return null;
    }

    private void UpdateCursor(MeshHitTarget potentialMeshInteraction)
    {
        ConstructionCursor.MouseState state = Input.GetMouseButton(0) ? ConstructionCursor.MouseState.LeftClickDown
            : (Input.GetMouseButton(1) ? ConstructionCursor.MouseState.RightClickDown : ConstructionCursor.MouseState.Hovering);

        cursor.UpdateCursor(potentialMeshInteraction, state);
    }



    private void UpdateCursorHighlight()
    {
        Vector3 cursorPos = GetGroundPositionAtScreenpoint(Input.mousePosition);
        Shader.SetGlobalVector("_DistToCursor", cursorPos);
    }

    public static Vector3 GetGroundPositionAtScreenpoint(Vector3 screenPoint)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        float enter;
        groundPlane.Raycast(ray, out enter);
        return ray.GetPoint(enter);
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

    private void HandleRightMeshClicks(MeshHitTarget hitInfo)
    {
        if (Input.GetMouseButtonUp(1) 
            && hitInfo != null 
            && hitInfo.SourceCell != null)
        {
            RegisterDesignationUndo(hitInfo.SourceCell);
            SetDesignation(hitInfo.SourceCell, VoxelDesignationType.Empty);
        }
    }
}
