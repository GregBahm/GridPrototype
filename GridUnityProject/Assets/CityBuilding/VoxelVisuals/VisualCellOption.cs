using System.Collections.Generic;
using UnityEngine;

public class VisualCellOption
{
    public Mesh Mesh { get; }
    public Material[] Materials {get;}
    private readonly VoxelDesignationType[,,] designation;

    public int Priority { get; }

    public bool Flipped { get; }
    public int Rotations { get; }

    public VisualCellConnections Connections { get; }

    public VisualCellOption(Mesh mesh, 
        Material[] materials,
        VoxelDesignationType[,,] designation, 
        bool flipped, 
        int rotations, 
        int priority, 
        VisualCellConnections connections)
    {
        Mesh = mesh;
        Materials = materials;
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
