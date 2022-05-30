using MeshMaking;
using UnityEngine;
using VoxelVisuals;

namespace Interaction
{
    public class ExteriorsInteractionManager : MonoBehaviour
    {

        private CityBuildingMain gameMain;
        private InteractionManager interactionMain;

        public VoxelDesignation FillType;

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

        public void SetFillToWalkableRoof() { FillType = VoxelDesignation.WalkableRoof; }
        public void SetFillToSlantedRoof() { FillType = VoxelDesignation.SlantedRoof; }
        public void SetFillToPlatform() { FillType = VoxelDesignation.Platform; }


        public void SetDesignation(DesignationCell cell, VoxelDesignation type)
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
                SetDesignation(hitInfo.CellAboveCursor, FillType);
            }
        }

        private void HandleRightMeshClicks(MeshHitTarget hitInfo)
        {
            if (Input.GetMouseButtonUp(1)
                && hitInfo != null
                && hitInfo.CellUnderCursor != null)
            {
                RegisterDesignationUndo(hitInfo.CellUnderCursor);
                SetDesignation(hitInfo.CellUnderCursor, VoxelDesignation.Empty);
            }
        }
    }
}