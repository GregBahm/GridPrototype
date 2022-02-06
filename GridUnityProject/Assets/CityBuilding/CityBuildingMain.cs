using GameGrid;
using MeshMaking;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

public class CityBuildingMain : MonoBehaviour
{
    [SerializeField]
    private GameObject interactionMeshObject;
    [SerializeField]
    private GameObject baseGridVisual;
    [SerializeField]
    private LightingManager lightingManager;
    [SerializeField]
    public int newGridMaxHeight = 20;
    public UndoManager UndoManager { get; private set; }

    public bool LoadLastSave;
    public bool TestSave;
    public bool TestLoad;

    public InteractionMesh InteractionMesh { get; private set; }

    public MainGrid MainGrid { get; private set; }

    private VoxelVisualsManager visualsManager;
    private VisualOptionsByDesignation optionsSource;

    public VoxelBlueprint[] Blueprints;

    public static CityBuildingMain Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UndoManager = new UndoManager();
        if(LoadLastSave)
        {
            GameSaveState saveState = GameSaveState.Load();
            MainGrid = new MainGrid(newGridMaxHeight, saveState.Ground.Points, saveState.Ground.Edges);
            Initialize();
            HashSet<GroundQuad> columnsToUpdate = new HashSet<GroundQuad>();
            foreach (var item in saveState.Designations.DesignationStates)
            {
                DesignationCell cell = MainGrid.Points[item.GroundPointIndex].DesignationCells[item.Height];
                cell.Designation = item.Designation;

                foreach (GroundQuad column in MainGrid.GetConnectedQuads(cell.GroundPoint))
                {
                    columnsToUpdate.Add(column);
                }
            }
            foreach (GroundQuad quad in columnsToUpdate)
            {
                visualsManager.UpdateColumn(quad);
            }
            UpdateInteractionGrid();
        }
        else
        {
            MainGrid = GroundSaveState.LoadDefault();
            Initialize();
        }
    }

    private void Initialize()
    {
        InteractionMesh = new InteractionMesh();
        UpdateInteractionGrid();
        UpdateBaseGrid();
        optionsSource = new VisualOptionsByDesignation(Blueprints);
        visualsManager = new VoxelVisualsManager(this, optionsSource);
    }

    private void Update()
    {
        if(TestSave)
        {
            TestSave = false;
            GameSaveState state = new GameSaveState(this);
            state.Save();
            Debug.Log("Grid Saved");
        }
        if(TestLoad)
        {
            TestLoad = false;
            // TODO: loading
            //MainGrid = GroundSaveState.Load();
            Debug.Log("Grid Loaded");
        }
    }   

    public void UpdateVoxelVisuals(DesignationCell cell)
    {
        foreach(GroundQuad quad in MainGrid.GetConnectedQuads(cell.GroundPoint))
        {
            visualsManager.UpdateColumn(quad);
        }
    }

    public void UpdateBaseGrid()
    {
        InteractionMesh.UpdateGroundMesh(MainGrid);
        baseGridVisual.GetComponent<MeshFilter>().mesh = InteractionMesh.BaseGridMesh;
        if (visualsManager != null)
        {
            visualsManager.UpdateForBaseGridModification();
        }
        lightingManager.UpdatePostion(MainGrid);
    }
    public void UpdateBaseGrid(GridExpander expander)
    {
        MainGrid.AddToMesh(expander.Points, expander.Edges);
        UpdateBaseGrid();
    }

    public void UpdateInteractionGrid()
    {
        InteractionMesh.UpdateMesh(MainGrid);
        interactionMeshObject.GetComponent<MeshCollider>().sharedMesh = null; // Hack to force update
        interactionMeshObject.GetComponent<MeshCollider>().sharedMesh = InteractionMesh.Mesh;
    }
}
