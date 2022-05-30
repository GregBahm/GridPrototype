using Interiors;
using MeshMaking;
using System;
using UnityEngine;
using UnityEngine.UI;
using VoxelVisuals;

namespace Interaction
{
    public class InteriorsInteractionManager : MonoBehaviour
    {
        private CityBuildingMain gameMain;

        [SerializeField]
        private ConstructionCursor cursor;
        [SerializeField]
        private Toggle addButton;

        private Interior selectedInterior;

        private void Start()
        {
            gameMain = GetComponent<CityBuildingMain>();
        }

        public void ProceedWithUpdate(bool wasDragging, bool uiHovered)
        {
            if(!wasDragging && !uiHovered)
            {
                if(addButton.isOn)
                {
                    if (selectedInterior == null)
                        HandleAddRoom();
                    else
                        HandleExpandSelectedRoom();
                }
                else
                {
                    HandleSelectRoom();
                }
            }
            else
            {
                UpdateCursor(null);
            }
        }

        private void HandleExpandSelectedRoom()
        {
            MeshHitTarget meshHitTarget = GetExpandedInteriorTarget();
            bool canExpandRoom = CanExpandRoom(meshHitTarget);
            if (canExpandRoom)
            {
                UpdateCursor(meshHitTarget);
                if (Input.GetMouseButtonUp(0))
                {
                    DoExpandRoom(meshHitTarget.CellAboveCursor);
                }
            }
            else
                UpdateCursor(null);
        }
        private MeshHitTarget GetNewRoomTarget()
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (gameMain.InteractionMesh.Collider.Raycast(ray, out hit, float.MaxValue))
            {
                return gameMain.InteractionMesh.GetHitTarget(hit.triangleIndex);
            }
            return null;
        }

        private MeshHitTarget GetExpandedInteriorTarget()
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            InteriorInteractionMesh mesh = gameMain.Interiors.GetMeshFor(selectedInterior);
            if (mesh.Collider.Raycast(ray, out hit, float.MaxValue))
            {
                return mesh.GetHitTarget(hit.triangleIndex);
            }
            return null;
        }

        private void UpdateCursor(MeshHitTarget potentialMeshInteraction)
        {
            ConstructionCursor.CurorState state = Input.GetMouseButton(0) ? ConstructionCursor.CurorState.LeftClickDown
                : (Input.GetMouseButton(1) ? ConstructionCursor.CurorState.RightClickDown : ConstructionCursor.CurorState.Hovering);

            cursor.UpdateCursor(potentialMeshInteraction, state);
        }

        private void HandleAddRoom()
        {
            // In this case, filled designation cells without interiors can be clicked on to start a new room.
            // Right clicking cells with interiors clears the interior
            MeshHitTarget meshHitTarget = GetNewRoomTarget();
            bool canAddNewRoom = CanAddNewRoom(meshHitTarget);
            if (canAddNewRoom)
            {
                UpdateCursor(meshHitTarget);
                if(Input.GetMouseButtonUp(0))
                {
                    DoAddNewRoom(meshHitTarget.CellUnderCursor);
                }
            }
            else
                UpdateCursor(null);
        }

        private void DoAddNewRoom(DesignationCell cell)
        {
            Interior newInterior = new Interior(gameMain.MainGrid.Interiors);
            cell.AssignedInterior = newInterior;
            gameMain.Interiors.UpdateInteractionMesh(newInterior);
            selectedInterior = newInterior;
        }

        private void DoExpandRoom(DesignationCell sourceCell)
        {
            sourceCell.AssignedInterior = selectedInterior;
            gameMain.Interiors.UpdateInteractionMesh(selectedInterior);
        }

        bool CanExpandRoom(MeshHitTarget meshHitTarget)
        {
            DesignationCell cell = meshHitTarget?.CellAboveCursor;
            if (cell != null)
            {
                bool isInteriorable = cell.Designation == VoxelDesignation.SlantedRoof ||
                    cell.Designation == VoxelDesignation.WalkableRoof;
                return isInteriorable && cell.AssignedInterior == null;
            }
            return false;
        }

        bool CanAddNewRoom(MeshHitTarget meshHitTarget)
        {
            DesignationCell cell = meshHitTarget?.CellUnderCursor;
            if(cell != null)
            {
                bool isInteriorable = cell.Designation == VoxelDesignation.SlantedRoof ||
                    cell.Designation == VoxelDesignation.WalkableRoof;
                return isInteriorable && cell.AssignedInterior == null;
            }
            return false;
        }

        private void HandleSelectRoom()
        {
            // In this case, hovering an interior highlights it, and clicking it makes it selected
            // So once they select a room, they can edit it. The "+" button becomes a "check" for done
            // Once they're done editing, they can click the check, which sets selectedInterior back to null
        }
    }
}