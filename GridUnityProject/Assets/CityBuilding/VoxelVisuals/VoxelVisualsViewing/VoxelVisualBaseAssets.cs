using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VoxelVisualBaseAssets : MonoBehaviour
{
    public static VoxelVisualBaseAssets Instance;

    public Color AnyFilled;
    public Color WalkableRoof;
    public Color SlantedRoof;
    public Color Platform;
    public Color Ground;

    public Material WallMat;
    public Material StrutMat;
    public Material SlantedRoofMat;
    public Material PlatformMat;

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

    public Color GetColorFor(VoxelDesignation slotType)
    {
        switch (slotType)
        {
            case VoxelDesignation.AnyFilled:
                return AnyFilled;
            case VoxelDesignation.SlantedRoof:
                return SlantedRoof;
            case VoxelDesignation.WalkableRoof:
                return WalkableRoof;
            case VoxelDesignation.Platform:
                return Platform;
            case VoxelDesignation.Empty:
            case VoxelDesignation.Ground:
            default:
                return Ground;
        }
    }
}