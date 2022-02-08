using GameGrid;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Interaction
{
    public class FoundationInteractionManager : MonoBehaviour
    {
        private CityBuildingMain gameMain;
        private InteractionManager interactionMain;

        [SerializeField]
        private int gridExpansions;

        [SerializeField]
        private float gridExpansionDistance = 1;

        [SerializeField]
        private Toggle expandButton;
        [SerializeField]
        private Button smoothButton;
        [SerializeField]
        private Slider ExpansionsSlider;

        [SerializeField]
        private ExpansionCursor expansionCursor;

        private void Start()
        {
            gameMain = GetComponent<CityBuildingMain>();
            interactionMain = GetComponent<InteractionManager>();
        }

        public void ProceedWithUpdate(bool wasDragging, bool uiHovered)
        {
            if (uiHovered)
            {
                expansionCursor.gameObject.SetActive(false);
                if (IsSmoothPressed())
                {
                    DoSmoothing();
                }
                return;
            }
            bool isFoundation = (interactionMain.SelectedTab == InteractionManager.UiTab.Foundation);
            expansionCursor.gameObject.SetActive(isFoundation);
            if (!wasDragging && isFoundation)
                HandleExpansion();
        }

        private bool IsSmoothPressed()
        {
            return Input.GetMouseButton(0) &&
                UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == smoothButton.gameObject;
        }

        private void HandleExpansion()
        {
            gridExpansions = (int)(ExpansionsSlider.value * gameMain.MainGrid.BorderEdges.Count);
            GridExpander expander = new GridExpander(gameMain.MainGrid, gridExpansions, gridExpansionDistance);
            expander.Update(GetGridSpaceCursorPosition());
            if (!Input.GetMouseButton(0))
            {
                expansionCursor.PreviewExpansion(expander);
            }
            if (Input.GetMouseButtonUp(0))
            {
                // TODO: Handle expansion undo
                gameMain.UpdateMainGrid(expander);
                gameMain.UpdateInteractionGrid();
            }
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

        private void DoSmoothing()
        {
            //TODO: handle smoothing undo
            gameMain.MainGrid.DoEase();
            gameMain.UpdateGroundMesh();
            gameMain.UpdateInteractionGrid();
        }
    }
}