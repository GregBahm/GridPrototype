using System.Collections.Generic;
using UnityEngine;

public class VoxelVisualOption
{
    public Mesh Mesh { get; }
    private readonly SlotType[,,] designation;

    public int Priority { get; }

    public bool Flipped { get; }
    public int Rotations { get; }

    public VoxelVisualConnections Connections { get; }

    public VoxelVisualOption(Mesh mesh, SlotType[,,] designation, bool flipped, int rotations, int priority, VoxelVisualConnections connections)
    {
        Mesh = mesh;
        this.designation = designation;
        Flipped = flipped;
        Rotations = rotations;
        Priority = priority;
        Connections = connections;
    }

    public IEnumerable<string> GetDesignationKeys()
    {
        IEnumerable<SlotType[,,]> keyDescriptions = GetAllPossibleDesignationKeys(designation);
        foreach (SlotType[,,] description in keyDescriptions)
        {
            yield return VoxelDesignation.GetDesignationKey(description);
        }
    }

    // For every "AnyFilled" slot, produce a version that is Slanted and version that is Flat  
    private static IEnumerable<SlotType[,,]> GetAllPossibleDesignationKeys(SlotType[,,] currentDesignation)
    {
        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                for (int z = 0; z < 2; z++)
                {
                    if (currentDesignation[x, y, z] == SlotType.AnyFilled)
                    {
                        SlotType[,,] newDesignationA = currentDesignation.Clone() as SlotType[,,];
                        SlotType[,,] newDesignationB = currentDesignation.Clone() as SlotType[,,];
                        newDesignationA[x, y, z] = SlotType.WalkableRoof;
                        newDesignationB[x, y, z] = SlotType.SlantedRoof;
                        foreach (SlotType[,,] item in GetAllPossibleDesignationKeys(newDesignationA))
                        {
                            yield return item;
                        }
                        foreach (SlotType[,,] item in GetAllPossibleDesignationKeys(newDesignationB))
                        {
                            yield return item;
                        }
                        yield break;
                    }
                }
            }
        }
        yield return currentDesignation;
    }

    public override string ToString()
    {
        return Mesh?.name;
    }
}
