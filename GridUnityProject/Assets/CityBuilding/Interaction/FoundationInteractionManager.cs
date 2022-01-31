using Assets.GameGrid;
using UnityEngine;

public class FoundationInteractionManager : MonoBehaviour
{
    public int GridExpansions;
    public float GridExpansionDistance = 1;

    private CityBuildingMain gameMain;

    private void Start()
    {
        gameMain = GetComponent<CityBuildingMain>();
    }

    public void ProceedWithUpdate()
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

    private Vector2 GetGridSpaceCursorPosition()
    {
        Plane plane = new Plane(Vector3.up, 0);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distance;
        plane.Raycast(ray, out distance);
        Vector3 planePosition = ray.GetPoint(distance);
        return new Vector2(planePosition.x, planePosition.z);
    }

    private void HandleEasing()
    {
        if (Input.GetMouseButton(1)) // Holding right mouse button
        {
            gameMain.MainGrid.DoEase();
            gameMain.UpdateBaseGrid();
            gameMain.UpdateInteractionGrid();
        }
    }
}