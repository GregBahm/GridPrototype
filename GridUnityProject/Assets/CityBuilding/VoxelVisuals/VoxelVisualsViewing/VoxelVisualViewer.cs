using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class VoxelVisualViewer : MonoBehaviour
{
    public static VoxelVisualViewer Instance { get; private set; }

    public VoxelBlueprint CurrentBlueprint;
    public int CurrentBlueprintIndex;
    public bool Next;
    public bool Previous;

    public VoxelBlueprint[] AllBlueprints;

    public VoxelDesignationDisplay X0Y0Z0Display;
    public VoxelDesignationDisplay X1Y0Z0Display;
    public VoxelDesignationDisplay X0Y1Z0Display;
    public VoxelDesignationDisplay X1Y1Z0Display;
    public VoxelDesignationDisplay X0Y0Z1Display;
    public VoxelDesignationDisplay X1Y0Z1Display;
    public VoxelDesignationDisplay X0Y1Z1Display;
    public VoxelDesignationDisplay X1Y1Z1Display;

    public ConnectionLabel[] ConnectionLabels;
    private MeshFilter meshFilter;

    public Color AnyFilledColor;
    public Color FlatRoofColor;
    public Color SlantedRoofColor;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        FixAll();
    }

    private void FixAll()
    {
        foreach (VoxelBlueprint item in AllBlueprints)
        {
            EditorUtility.SetDirty(item);
            //SlotType[,,] cube = item.Designations.ToCubedArray();
            //item.Designations.X0Y0Z0 = cube[1, 1, 1];
            //item.Designations.X1Y0Z0 = cube[0, 1, 1];
            //item.Designations.X0Y1Z0 = cube[1, 0, 1];
            //item.Designations.X1Y1Z0 = cube[0, 0, 1];
            //item.Designations.X0Y0Z1 = cube[1, 1, 0];
            //item.Designations.X1Y0Z1 = cube[0, 1, 0];
            //item.Designations.X0Y1Z1 = cube[1, 0, 0];
            //item.Designations.X1Y1Z1 = cube[0, 0, 0];
        }
    }

    private void Update()
    {
        UpdateBlueprintIndex();
        CurrentBlueprint = AllBlueprints[CurrentBlueprintIndex];

        meshFilter.mesh = CurrentBlueprint.ArtContent;
        SetDesignationDisplay();
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
        CurrentBlueprintIndex %= (AllBlueprints.Length);
        if (CurrentBlueprintIndex < 0)
        {
            CurrentBlueprintIndex = AllBlueprints.Length + CurrentBlueprintIndex;
        }
    }

    private void SetDesignationDisplay()
    {
        X0Y0Z0Display.UpdateDisplayContent(CurrentBlueprint.Designations.X0Y0Z0);
        X1Y0Z0Display.UpdateDisplayContent(CurrentBlueprint.Designations.X1Y0Z0);
        X0Y1Z0Display.UpdateDisplayContent(CurrentBlueprint.Designations.X0Y1Z0);
        X1Y1Z0Display.UpdateDisplayContent(CurrentBlueprint.Designations.X1Y1Z0);
        X0Y0Z1Display.UpdateDisplayContent(CurrentBlueprint.Designations.X0Y0Z1);
        X1Y0Z1Display.UpdateDisplayContent(CurrentBlueprint.Designations.X1Y0Z1);
        X0Y1Z1Display.UpdateDisplayContent(CurrentBlueprint.Designations.X0Y1Z1);
        X1Y1Z1Display.UpdateDisplayContent(CurrentBlueprint.Designations.X1Y1Z1);
    }
}
