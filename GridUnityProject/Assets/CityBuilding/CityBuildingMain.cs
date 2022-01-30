using GameGrid;
using MeshMaking;
using VisualsSolving;
using UnityEngine;
using UnityEditor;
using System.Linq;

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

    public MainGrid MainGrid { get; private set; }

    private VoxelVisualsManager visualsManager;
    private VisualOptionsByDesignation optionsSource;

    private VisualsSolver solver;

    public VoxelBlueprint[] Blueprints;

    public static CityBuildingMain Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if(LoadLastSave)
        {
            GameSaveState state = GameSaveState.Load();
            MainGrid = new MainGrid(state.Ground.Points, state.Ground.Edges);
            Initialize();
            foreach (var item in state.Designations.DesignationStates)
            {
                DesignationCell cell = MainGrid.Points[item.GroundPointIndex].DesignationCells[item.Height];
                cell.Designation = item.Designation;
                visualsManager.DoImmediateUpdate(cell);
                solver = new VisualsSolver(MainGrid, optionsSource);
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
        visualsManager = new VoxelVisualsManager(optionsSource);
        solver = new VisualsSolver(MainGrid, optionsSource);
    }

    public void StubMissingBlueprint(VoxelDesignation designation)
    {
#if UNITY_EDITOR
        //GameObject gameObj = Instantiate(BlueprintViewerPrefab);
        //gameObj.transform.position = new Vector3(0, 5, 0);
        //BlueprintViewer viewer = gameObj.GetComponent<BlueprintViewer>();
        //VoxelBlueprint blueprint = ScriptableObject.CreateInstance<VoxelBlueprint>();
        //blueprint.Designations = DesignationGrid.FromDesignation(designation);
        //viewer.Blueprint = blueprint;

        //string path = VoxelBlueprint.GetBlueprintAssetPath(blueprint);
        //AssetDatabase.CreateAsset(blueprint, path);
        //AssetDatabase.Refresh();
#endif
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
            //MainGrid = GroundSaveState.Load();
            Debug.Log("Grid Loaded");
        }
        HandleSolver();
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
            visualsManager.UpdateDebugObject(item.Component);
        }
        solver.ReadyToDisplayVoxels.Clear();
    }

    public void UpdateBaseGrid()
    {
        InteractionMesh.UpdateGroundMesh(MainGrid);
        BaseGridVisual.GetComponent<MeshFilter>().mesh = InteractionMesh.BaseGridMesh;
        if(visualsManager != null)
        {
            visualsManager.UpdateForBaseGridModification();
        }
    }

    public void UpdateInteractionGrid()
    {
        InteractionMesh.UpdateMesh(MainGrid);
        InteractionMeshObject.GetComponent<MeshCollider>().sharedMesh = null; // Hack to force update
        InteractionMeshObject.GetComponent<MeshCollider>().sharedMesh = InteractionMesh.Mesh;
    }

    internal void UpdateVoxelVisuals(DesignationCell changedCell)
    {
        visualsManager.DoImmediateUpdate(changedCell);
        solver = new VisualsSolver(MainGrid, optionsSource);
    }
}
