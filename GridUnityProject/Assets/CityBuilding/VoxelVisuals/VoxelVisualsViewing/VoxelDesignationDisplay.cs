using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VoxelDesignationDisplay : MonoBehaviour
{
    public MeshRenderer Content;
    public TextMeshPro Label;
    private string baseLabel;

    private void Start()
    {
        baseLabel = Label.text;
    }

    public void UpdateDisplayContent(VoxelDesignationType slotType)
    {
        Content.enabled = slotType != VoxelDesignationType.Empty;
        Color color = VoxelVisualViewer.Instance.Colors.GetColorFor(slotType);
        Content.material.SetColor("_Color", color);
        Label.text = GetLabelText(slotType);
    }

    private string GetLabelText(VoxelDesignationType slotType)
    {
        if (slotType == VoxelDesignationType.Empty)
            return "";
        string ret = baseLabel + " ";
        ret += slotType.ToString();
        return ret;
    }
}
