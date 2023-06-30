using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VoxelDesignationDisplay : MonoBehaviour
{
    public MeshRenderer XInterior;
    public MeshRenderer YInterior;
    public MeshRenderer ZInterior;

    public MeshRenderer XExterior;
    public MeshRenderer YExterior;
    public MeshRenderer ZExterior;

    public void UpdateDisplayContent(Designation slotType, 
        Designation adjacentX, 
        Designation adjacentY, 
        Designation adjacentZ)
    {
        Color color = VoxelVisualBaseAssets.Instance.GetColorFor(slotType);
        XInterior.material.SetColor("_Color", color);
        YInterior.material.SetColor("_Color", color);
        ZInterior.material.SetColor("_Color", color);

        XInterior.gameObject.SetActive(adjacentX == Designation.Empty);
        YInterior.gameObject.SetActive(adjacentY == Designation.Empty);
        ZInterior.gameObject.SetActive(adjacentZ == Designation.Empty);

        SetExterior(XExterior, slotType, adjacentY, adjacentZ);
        SetExterior(YExterior, slotType, adjacentX, adjacentZ);
        SetExterior(ZExterior, slotType, adjacentY, adjacentX);
    }

    private void SetExterior(MeshRenderer renderer,
        Designation self,
        Designation adjacentA, 
        Designation adjacentB)
    {
        Color colorA = GetColor(self,adjacentA);
        Color colorB = GetColor(self, adjacentB);
        renderer.material.SetColor("_ColorA", colorA);
        renderer.material.SetColor("_ColorB", colorB);
    }

    private Color GetColor(Designation self, Designation adjacent)
    {
        if(adjacent != Designation.Empty)
        {
            return Color.clear;
        }
        return VoxelVisualBaseAssets.Instance.GetColorFor(self);
    }
}
