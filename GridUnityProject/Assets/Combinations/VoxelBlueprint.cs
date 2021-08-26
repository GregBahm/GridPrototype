using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "VoxelDefinition/VoxelBlueprint")]
public class VoxelBlueprint : ScriptableObject
{
    public Mesh ArtContent;
    public bool[] DesignationValues;
    public VoxelConnectionType Up;
    public VoxelConnectionType Down;
    public VoxelConnectionType PositiveX;
    public VoxelConnectionType NegativeX;
    public VoxelConnectionType PositiveZ;
    public VoxelConnectionType NegativeZ;

    public IEnumerable<VoxelVisualOption> GenerateVisualOptions(VoxelConnectionType groundType)
    {
        VoxelVisualConnections baseConnections = new VoxelVisualConnections(Up, Down, PositiveX, NegativeX, PositiveZ, NegativeZ);

        VoxelDesignation baseDesignation = new VoxelDesignation(DesignationValues);
        IEnumerable<GeneratedVoxelDesignation> variants = baseDesignation.GetUniqueVariants();
        int priority = 0;
        bool isGround = Down == groundType;
        yield return new VoxelVisualOption(ArtContent, baseDesignation.Description, false, 0, priority, baseConnections, isGround);
        foreach (GeneratedVoxelDesignation variant in variants)
        {
            priority++;
            VoxelVisualConnections connectionsVariant = baseConnections.GetVarient(variant.WasFlipped, variant.Rotations);
            yield return new VoxelVisualOption(ArtContent, variant.Description, variant.WasFlipped, variant.Rotations, priority, connectionsVariant, isGround);
        }
    }
}
