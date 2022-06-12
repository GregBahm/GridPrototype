using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VoxelVisuals;

public class VoxelVisualComponentSet
{
    public VoxelConnectionType Up;
    public VoxelConnectionType Down;
    public VoxelVisualDesignation Designations;

    public ComponentInSet[] Components;

    internal IEnumerable<VisualCellOption> GetAllPermutations()
    {
        throw new NotImplementedException();
    }
}

[Serializable]
public class ComponentInSet
{
    public VoxelVisualComponent Component { get; }
    public bool Flipped { get; }
    public int Rotations { get; }
}

public class VoxelVisualComponent : ScriptableObject
{
    public Mesh ArtContent;
    public Material[] Materials;
}