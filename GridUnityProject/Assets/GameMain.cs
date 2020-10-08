using GameGrid;
using System.Linq;
using UnityEngine;

public class GameMain : MonoBehaviour
{
    public bool ShowGrid;
    public bool LoadLastSave;
    public bool TestSave;
    public bool TestLoad;
    public MainGrid MainGrid { get; private set; }

    private void Start()
    {
        MainGrid = LoadLastSave ? GroundLoader.Load() : GroundLoader.LoadDefault();
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
            GroundLoader.Save(MainGrid);
            Debug.Log("Grid Saved");
        }
        if(TestLoad)
        {
            TestLoad = false;
            MainGrid = GroundLoader.Load();
            Debug.Log("Grid Loaded");
        }
    }

    private void DoShowGrid()
    {
        foreach (GroundEdge edge in MainGrid.Edges)
        {
            Vector3 pointA = new Vector3(edge.PointA.Position.x, 0, edge.PointA.Position.y);
            Vector3 pointB = new Vector3(edge.PointB.Position.x, 0, edge.PointB.Position.y);
            Debug.DrawLine(pointA, pointB);
        }
    }
}
