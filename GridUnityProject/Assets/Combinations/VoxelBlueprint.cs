﻿using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "VoxelDefinition/VoxelBlueprint")]
public class VoxelBlueprint : ScriptableObject
{
    public Mesh ArtContent;
    public bool[] DesignationValues;
    public VoxelConnection Up;
    public VoxelConnection Down;
    public VoxelConnection PositiveX;
    public VoxelConnection NegativeX;
    public VoxelConnection PositiveZ;
    public VoxelConnection NegativeZ;

    public IEnumerable<VoxelVisualOption> GenerateVisualOptions()
    {
        VoxelDesignation baseDesignation = new VoxelDesignation(DesignationValues);
        IEnumerable<GeneratedVoxelDesignation> variants = baseDesignation.GetUniqueVariants();

        yield return new VoxelVisualOption(ArtContent, baseDesignation.Description, false, 0);
        foreach (GeneratedVoxelDesignation variant in variants)
        {
            yield return new VoxelVisualOption(ArtContent, variant.Description, variant.WasFlipped, variant.Rotations);
        }
    }
}