﻿using GameGrid;
using MeshMaking;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Interaction;
using System.IO;

public class CityBuildingMain : MonoBehaviour
{
    public TextAsset DefaultSave;
    [SerializeField]
    private GameObject interactionMeshObject;
    [SerializeField]
    private GameObject groundMesh;
    [SerializeField]
    private LightingManager lightingManager;
    [SerializeField]
    public int newGridMaxHeight = 20;
    public UndoManager UndoManager { get; private set; }

    public bool LoadLastSave;
    public bool TestSave;
    public bool TestLoad;

    public InteractionMesh InteractionMesh { get; private set; }
    public GroundMesh GroundMesh { get; private set; }

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
            GameSaveState saveState = GameSaveState.Load(DefaultSave.ToString());
            MainGrid = new MainGrid(newGridMaxHeight, saveState.Ground.Points, saveState.Ground.Edges);
            Initialize();
            HashSet<GroundQuad> columnsToUpdate = new HashSet<GroundQuad>();
            foreach (var item in saveState.Designations.DesignationStates) // Turn back on after working out the grid
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
        GroundMesh = new GroundMesh();
        UpdateInteractionGrid();
        UpdateGroundMesh();
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

    public void UpdateGroundMesh()
    {
        GroundMesh.UpdateGroundMesh(MainGrid);
        groundMesh.GetComponent<MeshFilter>().mesh = GroundMesh.BaseGridMesh;
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

    public void UpdateInteractionGrid()
    {
        InteractionMesh.UpdateMesh(MainGrid);
        interactionMeshObject.GetComponent<MeshCollider>().sharedMesh = null; // Hack to force update
        interactionMeshObject.GetComponent<MeshCollider>().sharedMesh = InteractionMesh.Mesh;
    }
}
