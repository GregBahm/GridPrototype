using GameGrid;
using MeshMaking;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class GameMain : MonoBehaviour
{
    public bool ShowGrid;
    public bool LoadLastSave;
    public bool TestSave;
    public bool TestLoad;

    public InteractionMesh InteractionMesh { get; private set; }

    [SerializeField]
    private GameObject InteractionMeshObject;

    [SerializeField]
    private Transform debugCube;

    public MainGrid MainGrid { get; private set; }

    private void Start()
    {
        MainGrid = LoadLastSave ? GroundLoader.Load() : GroundLoader.LoadDefault();
        InteractionMesh = new InteractionMesh(new Mesh());
        UpdateInteractionGrid();
        InteractionMeshObject.GetComponent<MeshFilter>().sharedMesh = InteractionMesh.Mesh;
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

    public void UpdateInteractionGrid()
    {
        InteractionMesh.UpdateMesh(MainGrid);
        InteractionMeshObject.GetComponent<MeshCollider>().sharedMesh = null; // Hack to force update
        InteractionMeshObject.GetComponent<MeshCollider>().sharedMesh = InteractionMesh.Mesh;
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
