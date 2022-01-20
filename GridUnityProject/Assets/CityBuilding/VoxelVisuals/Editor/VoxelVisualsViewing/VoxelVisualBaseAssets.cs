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

    public Color GetColorFor(VoxelDesignationType slotType)
    {
        switch (slotType)
        {
            case VoxelDesignationType.AnyFilled:
                return AnyFilled;
            case VoxelDesignationType.SlantedRoof:
                return SlantedRoof;
            case VoxelDesignationType.WalkableRoof:
                return WalkableRoof;
            case VoxelDesignationType.Platform:
                return Platform;
            case VoxelDesignationType.Empty:
            case VoxelDesignationType.Ground:
            default:
                return Ground;
        }
    }
}