using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VoxelDesignationDisplay : MonoBehaviour
{
    public MeshRenderer Content;

    public void UpdateDisplayContent(VoxelDesignationType slotType)
    {
        Content.enabled = slotType != VoxelDesignationType.Empty;
        Color color = VoxelVisualViewer.Instance.Colors.GetColorFor(slotType);
        Content.material.SetColor("_BaseColor", color);
    }
}
