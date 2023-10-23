using MeshMaking;
using UnityEngine;
using VoxelVisuals;

namespace Interaction
{
    public class ExteriorsInteractionManager : MonoBehaviour
    {

        private CityBuildingMain gameMain;
        private InteractionManager interactionMain;

        public Designation FillWith;

        [SerializeField]
        private ConstructionCursor cursor;

        private void Start()
        {
            gameMain = GetComponent<CityBuildingMain>();
            interactionMain = GetComponent<InteractionManager>();
        }

        public void ProceedWithUpdate(bool wasDragging, bool uiHovered)
        {
            if (uiHovered || interactionMain.SelectedTab != InteractionManager.UiTab.Exteriors)
            {
                UpdateCursor(null);
                return;
            }
            MeshHitTarget potentialMeshInteraction = GetPotentialMeshInteraction();
            UpdateCursor(potentialMeshInteraction);
            if (!wasDragging)
            {
                HandleRightMeshClicks(potentialMeshInteraction);
                HandleLeftMeshClicks(potentialMeshInteraction);
            }
        }

        public void SetFillToWalkableRoof() { FillWith = Designation.SquaredWalkableRoof; }
        public void SetFillToSlantedRoof() { FillWith = Designation.SquaredSlantedRoof; }
        public void SetFillToShell() { FillWith = Designation.Shell; }

        public void SetDesignation(DesignationCell cell, Designation type)
        {
            cell.Designation = type;
            gameMain.InteractionMesh.UpdateMesh(cell);
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
            if (gameMain.InteractionMesh.Collider.Raycast(ray, out hit, float.MaxValue))
            {
                return gameMain.InteractionMesh.GetHitTarget(hit.triangleIndex);
            }
            return null;
        }

        private void UpdateCursor(MeshHitTarget potentialMeshInteraction)
        {
            ConstructionCursor.CurorState state = Input.GetMouseButton(0) ? ConstructionCursor.CurorState.LeftClickDown
                : (Input.GetMouseButton(1) ? ConstructionCursor.CurorState.RightClickDown : ConstructionCursor.CurorState.Hovering);

            cursor.UpdateCursor(potentialMeshInteraction, state);
        }


        private void HandleLeftMeshClicks(MeshHitTarget hitInfo)
        {
            if (Input.GetMouseButtonUp(0)
                && hitInfo != null
                && hitInfo.CellAboveCursor != null
                && !hitInfo.CellAboveCursor.GroundPoint.IsBorder)
            {
                RegisterDesignationUndo(hitInfo.CellAboveCursor);
                SetDesignation(hitInfo.CellAboveCursor, FillWith);
            }
        }

        private void HandleRightMeshClicks(MeshHitTarget hitInfo)
        {
            if (Input.GetMouseButtonUp(1)
                && hitInfo != null
                && hitInfo.CellUnderCursor != null)
            {
                RegisterDesignationUndo(hitInfo.CellUnderCursor);
                SetDesignation(hitInfo.CellUnderCursor, Designation.Empty);
            }
        }
    }
}