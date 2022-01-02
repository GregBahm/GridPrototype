using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

public class VoxelVisualViewer : MonoBehaviour
{
    public static VoxelVisualViewer Instance { get; private set; }

    public VoxelBlueprint CurrentBlueprint;
    public Mesh CurrentMesh;
    public int CurrentBlueprintIndex;
    public bool Next;
    public bool Previous;

    public string GeneratedName;
    public string GeneratedMeshName;
    public bool MakeStubFromBlueprint;
    public bool SetCorrectBlueprintName;
    public bool FindArtContentForBlueprint;

    private List<VoxelBlueprint> allBlueprints;

    public VoxelDesignationDisplay X0Y0Z0Display;
    public VoxelDesignationDisplay X0Y0Z1Display;
    public VoxelDesignationDisplay X0Y1Z0Display;
    public VoxelDesignationDisplay X0Y1Z1Display;
    public VoxelDesignationDisplay X1Y0Z0Display;
    public VoxelDesignationDisplay X1Y0Z1Display;
    public VoxelDesignationDisplay X1Y1Z0Display;
    public VoxelDesignationDisplay X1Y1Z1Display;

    public ConnectionLabel[] ConnectionLabels;
    private MeshFilter meshFilter;

    public VoxelVisualColors Colors;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        allBlueprints = VoxelBlueprint.GetAllBlueprints().ToList();
    }

    private void CorrectCurrentBlueprintName()
    {
        string originalPath = AssetDatabase.GetAssetPath(CurrentBlueprint);
        AssetDatabase.RenameAsset(originalPath, GeneratedName);
    }

    private void Update()
    {
        UpdateBlueprintIndex();
        CurrentBlueprint = allBlueprints[CurrentBlueprintIndex];
        CurrentMesh = CurrentBlueprint.ArtContent;

        meshFilter.mesh = CurrentBlueprint.ArtContent;
        SetDesignationDisplay();

        HandleCommands();
    }

    private void HandleCommands()
    {
        GeneratedName = VoxelBlueprint.DeriveCorrectAssetName(CurrentBlueprint);
        GeneratedMeshName = GeneratedName.Replace(' ', '_');
        if (SetCorrectBlueprintName)
        {
            SetCorrectBlueprintName = false;
            CorrectCurrentBlueprintName();
        }
        if (MakeStubFromBlueprint)
        {
            MakeStubFromBlueprint = false;
            StubBlueprintFromCurrent();
        }
        if(FindArtContentForBlueprint)
        {
            FindArtContentForBlueprint = false;
            FindArtContent();
        }
    }

    private void FindArtContent()
    {
        string[] assets = AssetDatabase.FindAssets(GeneratedMeshName, new[] { "Assets/ArtContent/VoxelVisuals/" });
        if(assets.Length == 1)
        {
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(AssetDatabase.GUIDToAssetPath(assets[0]));
            CurrentBlueprint.ArtContent = mesh;
            EditorUtility.SetDirty(CurrentBlueprint);
        }
    }

    private void StubBlueprintFromCurrent()
    {
        VoxelBlueprint blueprint = ScriptableObject.CreateInstance<VoxelBlueprint>();

        blueprint.PositiveX = CurrentBlueprint.PositiveX;
        blueprint.Up = CurrentBlueprint.Up;
        blueprint.PositiveZ = CurrentBlueprint.PositiveZ;
        blueprint.NegativeX = CurrentBlueprint.NegativeX;
        blueprint.Down = CurrentBlueprint.Down;
        blueprint.NegativeZ = CurrentBlueprint.NegativeZ;

        blueprint.Designations = new DesignationGrid();
        blueprint.Designations.X0Y0Z0 = CurrentBlueprint.Designations.X0Y0Z0;
        blueprint.Designations.X0Y0Z1 = CurrentBlueprint.Designations.X0Y0Z1;
        blueprint.Designations.X0Y1Z0 = CurrentBlueprint.Designations.X0Y1Z0;
        blueprint.Designations.X0Y1Z1 = CurrentBlueprint.Designations.X0Y1Z1;
        blueprint.Designations.X1Y0Z0 = CurrentBlueprint.Designations.X1Y0Z0;
        blueprint.Designations.X1Y0Z1 = CurrentBlueprint.Designations.X1Y0Z1;
        blueprint.Designations.X1Y1Z0 = CurrentBlueprint.Designations.X1Y1Z0;
        blueprint.Designations.X1Y1Z1 = CurrentBlueprint.Designations.X1Y1Z1;

        blueprint.name = "stub";
        string path = VoxelBlueprint.BlueprintsFolderPath + "stub.asset";
        AssetDatabase.CreateAsset(blueprint, path);
        allBlueprints.Add(blueprint);
        CurrentBlueprintIndex = allBlueprints.Count - 1;
    }

    private void UpdateBlueprintIndex()
    {
        if (Next)
        {
            CurrentBlueprintIndex++;
            Next = false;
        }
        if (Previous)
        {
            CurrentBlueprintIndex--;
            Previous = false;
        }
        CurrentBlueprintIndex %= (allBlueprints.Count);
        if (CurrentBlueprintIndex < 0)
        {
            CurrentBlueprintIndex = allBlueprints.Count + CurrentBlueprintIndex;
        }
    }

    private void SetDesignationDisplay()
    {
        X0Y0Z0Display.UpdateDisplayContent(CurrentBlueprint.Designations.X0Y0Z0);
        X0Y0Z1Display.UpdateDisplayContent(CurrentBlueprint.Designations.X0Y0Z1);
        X0Y1Z0Display.UpdateDisplayContent(CurrentBlueprint.Designations.X0Y1Z0);
        X0Y1Z1Display.UpdateDisplayContent(CurrentBlueprint.Designations.X0Y1Z1);
        X1Y0Z0Display.UpdateDisplayContent(CurrentBlueprint.Designations.X1Y0Z0);
        X1Y0Z1Display.UpdateDisplayContent(CurrentBlueprint.Designations.X1Y0Z1);
        X1Y1Z0Display.UpdateDisplayContent(CurrentBlueprint.Designations.X1Y1Z0);
        X1Y1Z1Display.UpdateDisplayContent(CurrentBlueprint.Designations.X1Y1Z1);
    }
}

[Serializable]
public class VoxelVisualColors
{
    public Color AnyFilled;
    public Color WalkableRoof;
    public Color SlantedRoof;
    public Color Platform;
    public Color Ground;

    public Color GetColorFor(VoxelDesignationType slotType)
    {
        switch (slotType)
        {
            case VoxelDesignationType.AnyFilled:
                return AnyFilled;
            case VoxelDesignationType.SlantedRoof:
                return SlantedRoof;
            case VoxelDesignationType.WalkableRoof:
                return WalkableRoof;
            case VoxelDesignationType.Platform:
                return Platform;
            case VoxelDesignationType.Empty:
            case VoxelDesignationType.Ground:
            default:
                return Ground;
        }
    }
}