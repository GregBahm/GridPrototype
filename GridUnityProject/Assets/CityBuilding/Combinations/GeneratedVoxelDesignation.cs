﻿public class GeneratedVoxelDesignation : VoxelVisualDesignation
{
    public bool WasFlipped { get; }
    public int Rotations { get; }

    public GeneratedVoxelDesignation(bool flipped, int rotations)
        :base()
    {
        WasFlipped = flipped;
        Rotations = rotations;
    }
}
