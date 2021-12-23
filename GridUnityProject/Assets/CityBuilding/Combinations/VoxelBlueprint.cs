using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "VoxelDefinition/VoxelBlueprint")]
public class VoxelBlueprint : ScriptableObject
{
    public Mesh ArtContent;
    public VoxelConnectionType Up;
    public VoxelConnectionType Down;
    public VoxelConnectionType PositiveX;
    public VoxelConnectionType NegativeX;
    public VoxelConnectionType PositiveZ;
    public VoxelConnectionType NegativeZ;
    public DesignationGrid Designations;

    public IEnumerable<VoxelVisualOption> GenerateVisualOptions()
    {
        VoxelVisualConnections baseConnections = new VoxelVisualConnections(Up, Down, PositiveX, NegativeX, PositiveZ, NegativeZ);

        VoxelDesignation baseDesignation = new VoxelDesignation(Designations.ToFlatArray());
        int priority = 0;
        yield return new VoxelVisualOption(ArtContent, baseDesignation.Description, false, 0, priority, baseConnections);
        IEnumerable<GeneratedVoxelDesignation> variants = baseDesignation.GetUniqueVariants().ToArray();
        foreach (GeneratedVoxelDesignation variant in variants)
        {
            priority++;
            VoxelVisualConnections connectionsVariant = baseConnections.GetVariant(variant.WasFlipped, variant.Rotations);
            yield return new VoxelVisualOption(ArtContent, variant.Description, variant.WasFlipped, variant.Rotations, priority, connectionsVariant);
        }
    }
}

[Serializable]
public class DesignationGrid
{
    public SlotType X0Y0Z0;
    public SlotType X0Y0Z1;
    public SlotType X0Y1Z0;
    public SlotType X0Y1Z1;
    public SlotType X1Y0Z0;
    public SlotType X1Y0Z1;
    public SlotType X1Y1Z0;
    public SlotType X1Y1Z1;

    public SlotType[] ToFlatArray()
    {
        return new SlotType[]
        {
            X0Y0Z0,
            X0Y0Z1,
            X0Y1Z0,
            X0Y1Z1,
            X1Y0Z0,
            X1Y0Z1,
            X1Y1Z0,
            X1Y1Z1,
        };
    }

    public SlotType[,,] ToCubedArray()
    {
        SlotType[,,] ret = new SlotType[2, 2, 2];
        ret[0, 0, 0] = X0Y0Z0;
        ret[0, 0, 1] = X0Y0Z1;
        ret[0, 1, 0] = X0Y1Z0;
        ret[0, 1, 1] = X0Y1Z1;
        ret[1, 0, 0] = X1Y0Z0;
        ret[1, 0, 1] = X1Y0Z1;
        ret[1, 1, 0] = X1Y1Z0;
        ret[1, 1, 1] = X1Y1Z1;
        return ret;
    }
}

public enum SlotType
{
    Empty,
    AnyFilled,
    SlantedRoof,
    FlatRoof
}