using System;
using UnityEngine;

[Serializable]
public class VoxelVisualColors : MonoBehaviour
{
    public static VoxelVisualColors Instance;

    public Color AnyFilled;
    public Color WalkableRoof;
    public Color SlantedRoof;
    public Color Platform;
    public Color Ground;

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