using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VoxelDesignationDisplay : MonoBehaviour
{
    public MeshRenderer Content;

    public void UpdateDisplayContent(VoxelDesignation slotType)
    {
        Content.enabled = slotType != VoxelDesignation.Empty;
        Color color = VoxelVisualBaseAssets.Instance.GetColorFor(slotType);
        Content.material.SetColor("_Color", color);
    }
}
