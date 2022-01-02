using System.Collections.Generic;
using UnityEngine;

public class VoxelVisualOption
{
    public Mesh Mesh { get; }
    private readonly VoxelDesignationType[,,] designation;

    public int Priority { get; }

    public bool Flipped { get; }
    public int Rotations { get; }

    public VoxelVisualConnections Connections { get; }

    public VoxelVisualOption(Mesh mesh, VoxelDesignationType[,,] designation, bool flipped, int rotations, int priority, VoxelVisualConnections connections)
    {
        Mesh = mesh;
        this.designation = designation;
        Flipped = flipped;
        Rotations = rotations;
        Priority = priority;
        Connections = connections;
    }

    public string GetDesignationKey()
    {
        return VoxelDesignation.GetDesignationKey(designation);
    }

    public override string ToString()
    {
        return Mesh?.name;
    }
}
