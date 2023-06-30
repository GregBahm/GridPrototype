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
        Designation adjacentZ,
        Designation diagonalXY,
        Designation diagonalXZ,
        Designation diagonalYZ)
    {
        Color color = VoxelVisualBaseAssets.Instance.GetColorFor(slotType);
        XInterior.material.SetColor("_Color", color);
        YInterior.material.SetColor("_Color", color);
        ZInterior.material.SetColor("_Color", color);

        XInterior.gameObject.SetActive(adjacentX == Designation.Empty);
        YInterior.gameObject.SetActive(adjacentY == Designation.Empty);
        ZInterior.gameObject.SetActive(adjacentZ == Designation.Empty);

        SetExterior(XExterior, slotType, adjacentY, adjacentZ, diagonalYZ);
        SetExterior(YExterior, slotType, adjacentX, adjacentZ, diagonalXZ);
        SetExterior(ZExterior, slotType, adjacentY, adjacentX, diagonalXY);
    }

    private void SetExterior(MeshRenderer renderer,
        Designation self,
        Designation adjacentA, 
        Designation adjacentB,
        Designation diagonal)
    {
        Color colorA = GetColor(self,adjacentA);
        Color colorB = GetColor(self, adjacentB);
        Color colorC = GetColor(self, diagonal);
        renderer.material.SetColor("_AdjacentColorA", colorA);
        renderer.material.SetColor("_AdjacentColorB", colorB);
        renderer.material.SetColor("_DiagonalColor", colorC);
    }

    private Color GetColor(Designation self, Designation other)
    {
        if(other != Designation.Empty)
        {
            return Color.clear;
        }
        return VoxelVisualBaseAssets.Instance.GetColorFor(self);
    }
}
