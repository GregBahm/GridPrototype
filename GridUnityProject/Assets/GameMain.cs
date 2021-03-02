using Cysharp.Threading.Tasks;
using GameGrid;
using MeshMaking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;
using VisualsSolving;

public class GameMain : MonoBehaviour
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
    private OptionsByDesignation optionsSource;

    private VisualsSolvingManager solver;

    private void Start()
    {
        MainGrid = LoadLastSave ? GroundLoader.Load() : GroundLoader.Load(DefaultGridFile.text);
        InteractionMesh = new InteractionMesh(new Mesh());
        UpdateInteractionGrid();
        InteractionMeshObject.GetComponent<MeshFilter>().mesh = InteractionMesh.Mesh;
        BaseGridVisual.GetComponent<MeshFilter>().mesh = CloneInteractionMesh();
        optionsSource = new OptionsByDesignation(VoxelBlueprints);
        visualsAssembler = new VoxelVisualsManager(optionsSource, MainGrid, VoxelDisplayMat);
        solver = new VisualsSolvingManager(MainGrid, optionsSource);
    }

    private void OnDestroy()
    {
        solver.Destroy();
    }

    private void OnApplicationQuit()
    {
        solver.Destroy();
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
        if(solver.ChangedVoxels != null)
        {
            Debug.Log("got a solve");
            UpdateChangedVoxels();
        }
        visualsAssembler.ConstantlyUpdateComponentTransforms();
    }

    private void UpdateChangedVoxels()
    {
        IEnumerable<KeyValuePair<VoxelVisualComponent, VoxelVisualOption>> changedVoxels = solver.ChangedVoxels;
        solver.ChangedVoxels = null;
        foreach (KeyValuePair<VoxelVisualComponent, VoxelVisualOption> item in changedVoxels)
        {
            item.Key.Contents = item.Value;
            visualsAssembler.UpdateDebugObject(item.Key);
        }
    }

    public void UpdateInteractionGrid()
    {
        InteractionMesh.UpdateMesh(MainGrid);
        InteractionMeshObject.GetComponent<MeshCollider>().sharedMesh = null; // Hack to force update
        InteractionMeshObject.GetComponent<MeshCollider>().sharedMesh = InteractionMesh.Mesh;
    }

    internal void UpdateVoxelVisuals(VoxelCell changedCell)
    {
        solver.RegisterChangedVoxel(changedCell);
    }
}

public class VisualsSolvingManager
{
    private bool continueLooping = true;
    private Thread thread;

    public volatile IEnumerable<KeyValuePair<VoxelVisualComponent, VoxelVisualOption>> ChangedVoxels;

    private VoxelCell changedVoxel;
    private VisualsSolver currentSolver;
    private SolutionState lastSolution;

    public VisualsSolvingManager(MainGrid grid, OptionsByDesignation optionsSource)
    {
        currentSolver = new VisualsSolver(grid, optionsSource);
        lastSolution = currentSolver.FirstState;
        thread = new Thread(MainLoop);
        thread.Start();
    }

    public void RegisterChangedVoxel(VoxelCell changedVoxel)
    {
        this.changedVoxel = changedVoxel;
    }

    public void MainLoop()
    {
        while (continueLooping)
        {
            if(changedVoxel != null)
            {
                ResetSolver();
            }
            if(currentSolver.Status == VisualsSolver.SolverStatus.Solving)
            {
                UpdateVisualsSolver();
            }
        }
    }

    private void UpdateVisualsSolver()
    {
        Debug.Log("Advancing");
        currentSolver.AdvanceOneStep();
        if(currentSolver.Status != VisualsSolver.SolverStatus.Solving)
        {
            Debug.Log("This is a solve here");
            ChangedVoxels = GetChangedVoxels();
        }
    }

    private IEnumerable<KeyValuePair<VoxelVisualComponent, VoxelVisualOption>> GetChangedVoxels()
    {
        Dictionary<VoxelVisualComponent, VoxelVisualOption> lastState = lastSolution.GetDictionary();
        Dictionary<VoxelVisualComponent, VoxelVisualOption> currentState = currentSolver.LastState.GetDictionary();

        List<KeyValuePair<VoxelVisualComponent, VoxelVisualOption>> ret = new List<KeyValuePair<VoxelVisualComponent, VoxelVisualOption>>();
        foreach (KeyValuePair<VoxelVisualComponent, VoxelVisualOption> entry in currentState)
        {
            if(lastState[entry.Key] != currentState[entry.Key])
            {
                ret.Add(entry);
            }
        }
        return ret;
    }

    private void ResetSolver()
    {
        VoxelCell changed = changedVoxel;
        currentSolver.UpdateForChangedVoxel(changed);
        changedVoxel = null;
    }

    public void Destroy()
    {
        continueLooping = false;
        thread.Abort();
    }
}