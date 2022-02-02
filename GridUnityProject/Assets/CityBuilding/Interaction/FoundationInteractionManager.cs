using Assets.GameGrid;
using System;
using UnityEngine;
using UnityEngine.UI;

public class FoundationInteractionManager : MonoBehaviour
{
    [SerializeField]
    private int gridExpansions;

    [SerializeField]
    private float gridExpansionDistance = 1;

    [SerializeField]
    private Tool selectedTool;

    [SerializeField]
    private Toggle expandButton;
    [SerializeField]
    private Toggle smoothButton;
    [SerializeField]
    private Slider ExpansionsSlider;

    [SerializeField]
    private ExpansionCursor expansionCursor;

    private CityBuildingMain gameMain;

    public enum Tool
    {
        ExpandTool,
        SmoothTool
    }

    private void Start()
    {
        gameMain = GetComponent<CityBuildingMain>();
        expandButton.onValueChanged.AddListener(val => ToolToggleValueChanged(val, Tool.ExpandTool));
        smoothButton.onValueChanged.AddListener(val => ToolToggleValueChanged(val, Tool.SmoothTool));
    }

    private void ToolToggleValueChanged(bool value, Tool tool)
    {
        if (value)
            selectedTool = tool;
    }

    public void ProceedWithUpdate(bool wasDragging)
    {
        if (wasDragging)
        {
            expansionCursor.gameObject.SetActive(false);
            return;
        }
        if(selectedTool == Tool.SmoothTool)
        {
            expansionCursor.gameObject.SetActive(false);
            HandleSmoothing();
            return;
        }
        if(selectedTool == Tool.ExpandTool)
        {
            expansionCursor.gameObject.SetActive(true);
            HandleExpansion();
        }
    }

    private void HandleExpansion()
    {
        gridExpansions = (int)(ExpansionsSlider.value * gameMain.MainGrid.BorderEdges.Count);
        GridExpander expander = new GridExpander(gameMain.MainGrid, gridExpansions, gridExpansionDistance);
        expander.Update(GetGridSpaceCursorPosition());
        if(!Input.GetMouseButton(0))
        {
            expansionCursor.PreviewExpansion(expander);
        }
        if (Input.GetMouseButtonUp(0))
        {
            // TODO: Handle expansion undo
            gameMain.MainGrid.AddToMesh(expander.Points, expander.Edges);
            gameMain.UpdateBaseGrid();
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

    private void HandleSmoothing()
    {
        if (Input.GetMouseButton(0)) 
        {
            //TODO: handle smoothing undo
            gameMain.MainGrid.DoEase();
            gameMain.UpdateBaseGrid();
            gameMain.UpdateInteractionGrid();
        }
    }
}
