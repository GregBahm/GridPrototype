using GameGrid;
using MeshMaking;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Interaction;
using System.IO;
using Interiors;
using VoxelVisuals;

public class CityBuildingMain : MonoBehaviour
{
    public TextAsset DefaultSave;
    [SerializeField]
    private GameObject interactionMeshObject;
    [SerializeField]
    private GameObject interiorMeshPrefab;
    [SerializeField]
    private GameObject groundMesh;
    [SerializeField]
    private LightingManager lightingManager;
    [SerializeField]
    public int newGridMaxHeight = 30;
    public UndoManager UndoManager { get; private set; }

    public bool LoadLastSave;
    public bool TestSave;
    public bool TestLoad;

    public ExteriorsInteractionMesh InteractionMesh { get; private set; }
    public GroundMesh GroundMesh { get; private set; }

    public MainGrid MainGrid { get; private set; }

    private VoxelVisualsManager visualsManager;
    public InteriorsManager Interiors { get; private set; }
    
    private VisualOptionsByDesignation optionsSource;

    [SerializeField]
    private MasterVisualSetup visualSetup;
    public MasterVisualSetup VisualSetup => visualSetup;

    private void Start()
    {
        UndoManager = new UndoManager();
        if(LoadLastSave)
        {
            GameSaveState saveState = GameSaveState.Load(DefaultSave);
            MainGrid = new MainGrid(newGridMaxHeight, saveState.Ground.Points, saveState.Ground.Edges);
            Initialize();
            HashSet<GroundQuad> columnsToUpdate = new HashSet<GroundQuad>();
            foreach (var item in saveState.Designations.DesignationStates
                .Where(item => item.Height < newGridMaxHeight - 1))
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
        }
        else
        {
            MainGrid = GroundSaveState.LoadDefault();
            Initialize();
        }
        InteractionMesh.RebuildMesh();
    }

    private void Initialize()
    {
        InteractionMesh = new ExteriorsInteractionMesh(MainGrid, interactionMeshObject);
        GroundMesh = new GroundMesh(MainGrid);
        UpdateGroundMesh();
        optionsSource = new VisualOptionsByDesignation(visualSetup.ComponentSets);
        visualsManager = new VoxelVisualsManager(this, optionsSource);
        Interiors = new InteriorsManager(MainGrid, interiorMeshPrefab);
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
        visualsManager.Update();
    }   

    public void UpdateVoxelVisuals(DesignationCell cell)
    {
        foreach(GroundQuad quad in MainGrid.GetConnectedQuads(cell.GroundPoint))
        {
            visualsManager.UpdateColumn(quad);
        }
    }

    public void UpdateAllVisuals()
    {
        foreach (GroundQuad quad in MainGrid.Quads)
        {
            visualsManager.UpdateColumn(quad);
        }
    }

    public void UpdateGroundMesh()
    {
        GroundMesh.UpdateMesh();
        groundMesh.GetComponent<MeshFilter>().mesh = GroundMesh.Mesh;
        if (visualsManager != null)
        {
            visualsManager.UpdateForBaseGridModification();
        }
        lightingManager.UpdatePostion(MainGrid);
    }

    public void UpdateMainGrid(GridExpander expander)
    {
        MainGrid.AddToMesh(expander.Points, expander.Edges);
        UpdateGroundMesh();
    }

    private void OnDestroy()
    {
        if(visualsManager != null)
        {
            visualsManager.Dispose();
            visualsManager = null;
        }
    }
}
