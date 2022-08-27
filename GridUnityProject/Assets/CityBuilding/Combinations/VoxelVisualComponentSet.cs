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

    public IEnumerable<VisualCellOption> GetAllPermutations()
    {
        VoxelVisualDesignation realDesignation = designation.ToDesignation();
        IEnumerable<GeneratedVoxelDesignation> variants = realDesignation.GetUniqueVariants(true);
        foreach (var variant in variants)
        {
            ComponentInSet[] variantComponents = GetVariantComponents(variant.Rotations, variant.WasFlipped).ToArray();
            yield return new VisualCellOption(variantComponents, variant, up, down);
        }
    }

    public IEnumerable<ComponentInSet> GetVariantComponents(int designationRot, bool designationFlipped)
    {
        foreach (ComponentInSet component in components)
        {
            bool newFlipped = component.Flipped != designationFlipped;

            int compRot = component.Rotations;
            if (designationFlipped)
                compRot = 4 - compRot;
            int newRotations = (compRot + designationRot) % 4;
            yield return new ComponentInSet(component.Component, newFlipped, newRotations);
        }
    }
}
