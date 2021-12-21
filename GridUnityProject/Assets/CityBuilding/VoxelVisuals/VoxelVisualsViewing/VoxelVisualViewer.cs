using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VoxelVisualViewer : MonoBehaviour
{
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

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
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
