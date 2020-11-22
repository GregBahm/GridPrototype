using GameGrid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VisualsAssembler
{
    private readonly VisualVoxelOption[] allOptions;
    

    public VisualsAssembler(VoxelBlueprint[] blueprints)
    {
        allOptions = GetAllOptions(blueprints).ToArray();

    }

    private IEnumerable<VisualVoxelOption> GetAllOptions(VoxelBlueprint[] blueprints)
    {
        foreach (VoxelBlueprint blueprint in blueprints)
        {
            IEnumerable<VisualVoxelOption> options = blueprint.GenerateVisualOptions();
            foreach (VisualVoxelOption option in options)
            {
                yield return option;
            }
        }
    }

    internal void UpdateVoxels(VoxelCell targetCell)
    {
    }
}


public class VoxelVisualComponent
{
    public VoxelCell[,,] Designations;

    public VisualVoxelOption Contents { get; }
}

public class VisualVoxelOption
{
    public Mesh Mesh { get; }
    private readonly bool[,,] designation;

    public bool Flipped { get; }
    public int Rotations { get; }

    public VisualVoxelOption(Mesh mesh, bool[,,] designation, bool flipped, int rotations)
    {
        Mesh = mesh;
        this.designation = designation;
        Flipped = flipped;
        Rotations = rotations;
    }

    public bool GetDesignation(int x, int y, int z)
    {
        return designation[x, y, z];
    }

    // TODO: Connections


}