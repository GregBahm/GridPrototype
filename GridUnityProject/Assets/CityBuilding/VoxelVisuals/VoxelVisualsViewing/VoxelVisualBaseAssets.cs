using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class VoxelVisualBaseAssets : MonoBehaviour
{
    public static VoxelVisualBaseAssets Instance;

    public Color SquaredWalkableRoof;
    public Color SlantedRoof;
    public Color Platform;
    public Color Shell;

    public Material WallMat;
    public Material ShellMat;
    public Material StrutMat;
    public Material SlantedRoofMat;
    public Material PlatformMat;

    private Dictionary<Designation, Color> colorTable;

    public IEnumerable<Material> Materials
    {
        get
        {
            yield return WallMat;
            yield return StrutMat;
            yield return SlantedRoofMat;
            yield return PlatformMat;
        }
    }

    private Dictionary<string, Material> materialsTable;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        colorTable = new Dictionary<Designation, Color>()
        {
            {Designation.Empty, Color.clear },
            {Designation.Shell, Shell },
            {Designation.SquaredWalkableRoof, SquaredWalkableRoof },
            {Designation.SquaredSlantedRoof, SlantedRoof },
        };
        materialsTable = new Dictionary<string, Material>
        {
            {"WallMat", WallMat },
            {"StrutMat", StrutMat },
            {"SlantedRoofMat", SlantedRoofMat },
            {"PlatformMat", PlatformMat },
            {"ShellMat", ShellMat },
        };
    }

    public Color GetColorFor(Designation slotType)
    {
        return colorTable[slotType];
    }

    internal Material GetMaterialFor(Material material)
    {
        return materialsTable[material.name];
    }
}