using GameGrid;
using UnityEngine;

public class GameMain : MonoBehaviour
{
    public bool ShowGrid;
    public bool TestSave;
    public bool TestLoad;
    private MasterGrid masterGrid;

    private void Start()
    {
        masterGrid = GridLoader.LoadGrid();
    }

    private void Update()
    {
        if(ShowGrid)
        {
            DoShowGrid();
        }
        if(TestSave)
        {
            TestSave = false;
            GridLoader.SaveGrid(masterGrid);
        }
        if(TestLoad)
        {
            masterGrid = GridLoader.LoadGrid();
        }
    }

    private void DoShowGrid()
    {
        foreach (GridEdge edge in masterGrid.Edges)
        {
            Vector3 pointA = new Vector3(edge.PointA.Position.x, 0, edge.PointA.Position.y);
            Vector3 pointB = new Vector3(edge.PointB.Position.x, 0, edge.PointB.Position.y);
            Debug.DrawLine(pointA, pointB);
        }
    }
}
