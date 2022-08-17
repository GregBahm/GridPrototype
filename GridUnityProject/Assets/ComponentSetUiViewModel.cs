using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ComponentSetUiViewModel : MonoBehaviour
{
    private VoxelVisualComponentSet model;

    [SerializeField]
    private TextMeshProUGUI label;

    public void Initialize(VoxelVisualComponentSet model)
    {
        this.model = model;
        label.text = GetLabel(model.Designation);
    }

    private string GetLabel(SerializableVisualDesignation designation)
    {
        return designation.ToDesignation().ToString();
    }
}
