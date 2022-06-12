using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VoxelDesignationDisplay : MonoBehaviour
{
    public MeshRenderer Content;

    public void UpdateDisplayContent(Designation slotType)
    {
        Content.enabled = slotType != Designation.Empty;
        Color color = VoxelVisualBaseAssets.Instance.GetColorFor(slotType);
        Content.material.SetColor("_Color", color);
    }
}
