using GameGrid;
using MeshMaking;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

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

    private void Start()
    {
        MainGrid = LoadLastSave ? GroundLoader.Load() : GroundLoader.Load(DefaultGridFile.text);
        InteractionMesh = new InteractionMesh(new Mesh());
        UpdateInteractionGrid();
        InteractionMeshObject.GetComponent<MeshFilter>().mesh = InteractionMesh.Mesh;
        BaseGridVisual.GetComponent<MeshFilter>().mesh = CloneInteractionMesh();
        visualsAssembler = new VoxelVisualsManager(VoxelBlueprints, MainGrid, VoxelDisplayMat);
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
        visualsAssembler.ConstantlyUpdateComponentTransforms();
    }

    public void UpdateInteractionGrid()
    {
        InteractionMesh.UpdateMesh(MainGrid);
        InteractionMeshObject.GetComponent<MeshCollider>().sharedMesh = null; // Hack to force update
        InteractionMeshObject.GetComponent<MeshCollider>().sharedMesh = InteractionMesh.Mesh;
    }

    internal void UpdateVoxelVisuals(VoxelCell targetCell)
    {
        // Later we will want to do the whole wave collapse thing.
        // For now, we just want to tell this cell and all neighboring cells to update their display visuals based on the designation
        visualsAssembler.UpdateVoxels(targetCell);
    }
}
