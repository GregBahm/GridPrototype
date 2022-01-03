using GameGrid;
using MeshMaking;
using VisualsSolving;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CityBuildingMain : MonoBehaviour
{
    public bool LoadLastSave;
    public bool TestSave;
    public bool TestLoad;

    [SerializeField]
    private TextAsset DefaultGridFile;

    public InteractionMesh InteractionMesh { get; private set; }

    [SerializeField]
    private GameObject InteractionMeshObject;
    [SerializeField]
    private GameObject BaseGridVisual;

    [SerializeField]
    private VoxelBlueprint[] VoxelBlueprints;

    [SerializeField]
    private Material VoxelDisplayMat;

    public MainGrid MainGrid { get; private set; }

    private VoxelVisualsManager visualsAssembler;
    private VisualOptionsByDesignation optionsSource;

    private VisualsSolver solver;

    private void Start()
    {
        VoxelBlueprints = VoxelBlueprint.GetAllBlueprints();

        MainGrid = LoadLastSave ? GroundLoader.Load() : GroundLoader.Load(DefaultGridFile.text);
        InteractionMesh = new InteractionMesh(new Mesh());
        UpdateInteractionGrid();
        InteractionMeshObject.GetComponent<MeshFilter>().mesh = InteractionMesh.Mesh;
        BaseGridVisual.GetComponent<MeshFilter>().mesh = CloneInteractionMesh();
        optionsSource = new VisualOptionsByDesignation(VoxelBlueprints);
        visualsAssembler = new VoxelVisualsManager(VoxelDisplayMat, optionsSource);
        solver = new VisualsSolver(MainGrid, optionsSource);
    }

    private Mesh CloneInteractionMesh()
    {
        Mesh ret = new Mesh();
        ret.vertices = InteractionMesh.Mesh.vertices;
        ret.triangles = InteractionMesh.Mesh.triangles;
        ret.uv = InteractionMesh.Mesh.uv;
        return ret;
    }

    private void Update()
    {
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
        HandleSolver();
        visualsAssembler.ConstantlyUpdateComponentTransforms();
    }

    private const double solverWaitTime = (double)1 / 30;

    private void HandleSolver()
    {
        if (!solver.SolveComplete)
        {
            double startTime = Time.realtimeSinceStartupAsDouble;
            bool keepGoing = true;
            while(keepGoing && !solver.SolveComplete)
            {
                double currentTime = Time.realtimeSinceStartupAsDouble;
                if (currentTime - startTime > solverWaitTime)
                {
                    keepGoing = false;
                }
                solver.StepForward();
            }
        }
        UpdateSolvedVoxelVisuals();
    }

    private void UpdateSolvedVoxelVisuals()
    {
        foreach (CellState item in solver.ReadyToDisplayVoxels)
        {
            item.Component.Contents = item.RemainingOptions[0];
            visualsAssembler.UpdateDebugObject(item.Component);
        }
        solver.ReadyToDisplayVoxels.Clear();
    }

    public void UpdateInteractionGrid()
    {
        InteractionMesh.UpdateMesh(MainGrid);
        InteractionMeshObject.GetComponent<MeshCollider>().sharedMesh = null; // Hack to force update
        InteractionMeshObject.GetComponent<MeshCollider>().sharedMesh = InteractionMesh.Mesh;
    }

    internal void UpdateVoxelVisuals(DesignationCell changedCell)
    {
        visualsAssembler.DoImmediateUpdate(changedCell);
        solver = new VisualsSolver(MainGrid, optionsSource);
    }
}
