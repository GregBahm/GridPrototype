using MeshMaking;
using UnityEngine;


namespace Interaction
{
    public class ExteriorsInteractionManager : MonoBehaviour
    {

        private CityBuildingMain gameMain;
        private InteractionManager interactionMain;

        public VoxelDesignationType FillType;

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
            ConstructionCursor.CurorState state = Input.GetMouseButton(0) ? ConstructionCursor.CurorState.LeftClickDown
                : (Input.GetMouseButton(1) ? ConstructionCursor.CurorState.RightClickDown : ConstructionCursor.CurorState.Hovering);

            cursor.UpdateCursor(potentialMeshInteraction, state);
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
}