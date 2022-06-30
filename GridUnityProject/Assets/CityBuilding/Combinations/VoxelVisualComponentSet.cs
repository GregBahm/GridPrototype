using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VoxelVisuals;

[Serializable]
public class VoxelVisualComponentSet
{
    [SerializeField]
    private VoxelConnectionType up;
    [SerializeField]
    private VoxelConnectionType down;
    [SerializeField]
    private VoxelVisualDesignation designation;

    [SerializeField]
    private ComponentInSet[] components;

    public VoxelConnectionType Up { get => up; set => up = value; }
    public VoxelConnectionType Down { get => down; set => down = value; }
    public VoxelVisualDesignation Designation { get => designation; set => designation = value; }
    public ComponentInSet[] Components { get => components; set => components = value; }

    public VoxelVisualComponentSet(VoxelConnectionType up, 
        VoxelConnectionType down, 
        VoxelVisualDesignation designation, 
        ComponentInSet[] components)
    {
        this.up = up;
        this.down = down;
        this.designation = designation;
        this.components = components;
    }

    internal IEnumerable<VisualCellOption> GetAllPermutations()
    {
        var variants = designation.GetUniqueVariants();
        foreach (var variant in variants)
        {
            ComponentInSet[] variantComponents = GetVariantComponents(variant.Rotations, variant.WasFlipped).ToArray();
            yield return new VisualCellOption(variantComponents, designation, up, down);
        }
    }

    private IEnumerable<ComponentInSet> GetVariantComponents(int rotations, bool flipped)
    {
        foreach (ComponentInSet componentInSet in components)
        {
            bool newFlipped = componentInSet.Flipped != flipped;
            int newRotations = (rotations + componentInSet.Rotations) % 4;
            yield return new ComponentInSet(componentInSet.Component, newFlipped, newRotations);
        }
    }
}

[Serializable]
public class ComponentInSet
{
    [SerializeField]
    private VoxelVisualComponent component;
    public VoxelVisualComponent Component => component;
    [SerializeField]
    private bool flipped;
    public bool Flipped => flipped;
    [SerializeField]
    private int rotations;
    public int Rotations => rotations;

    public ComponentInSet(VoxelVisualComponent component, 
        bool flipped, 
        int rotations)
    {
        this.component = component;
        this.flipped = flipped;
        this.rotations = rotations;
    }
}

[CreateAssetMenu(menuName = "VoxelVisualComponent")]
public class VoxelVisualComponent : ScriptableObject
{
    [SerializeField]
    private Mesh mesh;
    [SerializeField]
    private Material[] materials;

    public Mesh Mesh { get => mesh; set => mesh = value; }
    public Material[] Materials { get => materials; set => materials = value; }
}