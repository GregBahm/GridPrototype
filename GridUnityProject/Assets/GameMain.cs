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
        visualsAssembler = new VoxelVisualsManager(VoxelDisplayMat);
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
            UpdateChangedVoxels();
        }
        //visualsAssembler.ConstantlyUpdateComponentTransforms();
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
