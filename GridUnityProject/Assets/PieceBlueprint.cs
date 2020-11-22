using GameGrid;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualPieceProfile : MonoBehaviour
{
    public VisualSet CoreSet { get; }
    public VisualSet ConectionUpward { get; }
    public VisualSet ConnectionDownward { get; }

    public VisualSet ConnectionX { get; }
    public VisualSet ConnectionZ { get; }
}

public class VoxelVisuals
{
    public VoxelCell Cell { get; }
    public VoxelVisuals(VoxelCell cell)
    {
        Cell = cell;
    }
}

public enum VisualSet
{
    Ground,
    Air,
    Structure,
    Walkway
}