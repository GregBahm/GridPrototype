using GameGrid;
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

    private InteractionMesh interactionMesh;

    [SerializeField]
    private GameObject InteractionMeshObject;

    [SerializeField]
    private Transform debugCube;

    public MainGrid MainGrid { get; private set; }

    private void Start()
    {
        MainGrid = LoadLastSave ? GroundLoader.Load() : GroundLoader.LoadDefault();
        interactionMesh = new InteractionMesh(new Mesh());
        UpdateInteractionGrid();
        InteractionMeshObject.GetComponent<MeshFilter>().sharedMesh = interactionMesh.Mesh;
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
        TestCollisionMesh();
    }

    public void UpdateInteractionGrid()
    {
        interactionMesh.UpdateMesh(MainGrid);
        InteractionMeshObject.GetComponent<MeshCollider>().sharedMesh = null;
        InteractionMeshObject.GetComponent<MeshCollider>().sharedMesh = interactionMesh.Mesh;
    }

    private void TestCollisionMesh()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            IHitTarget hitInfo = interactionMesh.GetHitTarget(hit.triangleIndex);
            debugCube.position = hitInfo.TargetCell.CellPosition;
            if(Input.GetMouseButtonDown(0))
            {
                hitInfo.TargetCell.Filled = !hitInfo.TargetCell.Filled;
                UpdateInteractionGrid();
            }
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
