using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VoxelVisualBaseAssets : MonoBehaviour
{
    public static VoxelVisualBaseAssets Instance;

    public Color SquaredWalkableRoof;
    public Color SquaredSlantedRoof;
    public Color SlantedRoof;
    public Color Platform;
    public Color Shell;
    public Color Aquaduct;

    public Material WallMat;
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

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        colorTable = new Dictionary<Designation, Color>()
        {
            {Designation.Empty, Color.white },
            {Designation.Shell, Shell },
            {Designation.SquaredWalkableRoof, SquaredWalkableRoof },
            {Designation.SquaredSlantedRoof, SlantedRoof },
            {Designation.Platform, Platform },
            {Designation.Aquaduct, Aquaduct }
        };
    }

    public Color GetColorFor(Designation slotType)
    {
        return colorTable[slotType];
    }
}