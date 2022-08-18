using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelVisualSetViewer : MonoBehaviour
{
    private VoxelVisualComponentSet model;

    [SerializeField]
    private VoxelDesignationDisplay X0Y0Z0Display;
    [SerializeField]
    private VoxelDesignationDisplay X0Y0Z1Display;
    [SerializeField]
    private VoxelDesignationDisplay X0Y1Z0Display;
    [SerializeField]
    private VoxelDesignationDisplay X0Y1Z1Display;
    [SerializeField]
    private VoxelDesignationDisplay X1Y0Z0Display;
    [SerializeField]
    private VoxelDesignationDisplay X1Y0Z1Display;
    [SerializeField]
    private VoxelDesignationDisplay X1Y1Z0Display;
    [SerializeField]
    private VoxelDesignationDisplay X1Y1Z1Display;

    [SerializeField]
    private Transform contents;

    public void Initialize(VoxelVisualComponentSet model)
    {
        this.model = model;
        SetDesignationDisplay();
    }

    private void UpdateVisuals()
    {
        ClearContents();
        foreach (ComponentInSet item in model.Components)
        {
            AddComponent(item);
        }
    }

    private void AddComponent(ComponentInSet item)
    {
        throw new NotImplementedException();
    }

    private void ClearContents()
    {
        throw new NotImplementedException();
    }

    private void SetDesignationDisplay()
    {
        VoxelVisualDesignation designation = model.Designation.ToDesignation();
        X0Y0Z0Display.UpdateDisplayContent(designation.X0Y0Z0);
        X0Y0Z1Display.UpdateDisplayContent(designation.X0Y0Z1);
        X0Y1Z0Display.UpdateDisplayContent(designation.X0Y1Z0);
        X0Y1Z1Display.UpdateDisplayContent(designation.X0Y1Z1);
        X1Y0Z0Display.UpdateDisplayContent(designation.X1Y0Z0);
        X1Y0Z1Display.UpdateDisplayContent(designation.X1Y0Z1);
        X1Y1Z0Display.UpdateDisplayContent(designation.X1Y1Z0);
        X1Y1Z1Display.UpdateDisplayContent(designation.X1Y1Z1);
    }
}
