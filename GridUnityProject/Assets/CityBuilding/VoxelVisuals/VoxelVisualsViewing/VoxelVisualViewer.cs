using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

public class VoxelVisualViewer : MonoBehaviour
{
    private const string BlueprintsFolderPath = "Assets/CityBuilding/Combinations/VoxelBlueprints";

    public static VoxelVisualViewer Instance { get; private set; }

    public VoxelBlueprint CurrentBlueprint;
    public int CurrentBlueprintIndex;
    public bool Next;
    public bool Previous;

    public bool ReportKeys;

    private VoxelBlueprint[] allBlueprints;

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
        allBlueprints = GetAllBlueprints();

    }

    private void CorrectAssetNames()
    {
        foreach (VoxelBlueprint item in allBlueprints)
        {
            item.name = DeriveCorrectAssetName(item);
            EditorUtility.SetDirty(item);
            if (item.ArtContent != null)
            {
                item.ArtContent.name = item.name;
                EditorUtility.SetDirty(item.ArtContent);
            }
        }
    }

    private static string DeriveCorrectAssetName(VoxelBlueprint blueprint)
    {
        return blueprint.Down.ToString() + " " +
            GetNameComponent(blueprint.Designations.X0Y0Z0) + " " +
            GetNameComponent(blueprint.Designations.X0Y0Z1) + " " +
            GetNameComponent(blueprint.Designations.X0Y1Z0) + " " +
            GetNameComponent(blueprint.Designations.X0Y1Z1) + " " +
            GetNameComponent(blueprint.Designations.X1Y0Z0) + " " +
            GetNameComponent(blueprint.Designations.X1Y0Z1) + " " +
            GetNameComponent(blueprint.Designations.X1Y1Z0) + " " +
            GetNameComponent(blueprint.Designations.X1Y1Z1) + " " +
                blueprint.Up.ToString();
    }

    private static string GetNameComponent(SlotType slotType)
    {
        switch (slotType)
        {
            case SlotType.Empty:
                return "E";
            case SlotType.AnyFilled:
                return "A";
            case SlotType.SlantedRoof:
                return "S";
            case SlotType.WalkableRoof:
                return "W";
            case SlotType.Platform:
                return "P";
            case SlotType.Ground:
            default:
                return "G";
        }
    }

    private VoxelBlueprint[] GetAllBlueprints()
    {
        string[] guids = AssetDatabase.FindAssets("t: VoxelBlueprint", new[] { BlueprintsFolderPath });
        return guids.Select(item => AssetDatabase.LoadAssetAtPath<VoxelBlueprint>(AssetDatabase.GUIDToAssetPath(item))).ToArray();
    }

    private void Update()
    {
        UpdateBlueprintIndex();
        CurrentBlueprint = allBlueprints[CurrentBlueprintIndex];

        meshFilter.mesh = CurrentBlueprint.ArtContent;
        SetDesignationDisplay();

        HandleKeyReport();
    }

    private void HandleKeyReport()
    {
        if(ReportKeys)
        {
            ReportKeys = false;
            VoxelVisualOption[] options = CurrentBlueprint.GenerateVisualOptions().ToArray();
            Dictionary<VoxelVisualOption, string[]> toInspect = options.ToDictionary(item => item, item => item.GetDesignationKeys().ToArray());
        }
    }

    private void UpdateBlueprintIndex()
    {
        if(Next)
        {
            CurrentBlueprintIndex++;
            Next = false;
        }
        if (Previous)
        {
            CurrentBlueprintIndex--;
            Previous = false;
        }
        CurrentBlueprintIndex %= (allBlueprints.Length);
        if (CurrentBlueprintIndex < 0)
        {
            CurrentBlueprintIndex = allBlueprints.Length + CurrentBlueprintIndex;
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

    public Color GetColorFor(SlotType slotType)
    {
        switch (slotType)
        {
            case SlotType.AnyFilled:
                return AnyFilled;
            case SlotType.SlantedRoof:
                return SlantedRoof;
            case SlotType.WalkableRoof:
                return WalkableRoof;
            case SlotType.Platform:
                return Platform;
            case SlotType.Empty:
            case SlotType.Ground:
            default:
                return Ground;
        }
    }
}