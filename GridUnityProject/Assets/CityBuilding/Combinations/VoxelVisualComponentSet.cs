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
    private SerializableVisualDesignation designation;
    public SerializableVisualDesignation Designation => designation;

    [SerializeField]
    private ComponentInSet[] components;

    public VoxelConnectionType Up { get => up; set => up = value; }
    public VoxelConnectionType Down { get => down; set => down = value; }
    public ComponentInSet[] Components { get => components; set => components = value; }

    public VoxelVisualComponentSet(VoxelConnectionType up, 
        VoxelConnectionType down, 
        VoxelVisualDesignation designation, 
        ComponentInSet[] components)
    {
        this.up = up;
        this.down = down;
        this.designation = new SerializableVisualDesignation(designation);
        this.components = components;
    }

    internal IEnumerable<VisualCellOption> GetAllPermutations()
    {
        VoxelVisualDesignation realDesignation = designation.ToDesignation();
        var variants = realDesignation.GetUniqueVariants();
        foreach (var variant in variants)
        {
            ComponentInSet[] variantComponents = GetVariantComponents(variant.Rotations, variant.WasFlipped).ToArray();
            yield return new VisualCellOption(variantComponents, realDesignation, up, down);
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
