using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class NeighborComponents
{
    public VoxelVisualComponent Up { get; }
    public VoxelVisualComponent Down { get; }
    public VoxelVisualComponent Forward { get; }
    public VoxelVisualComponent Back { get; }
    public VoxelVisualComponent Left { get; }
    public VoxelVisualComponent Right { get; }

    public NeighborComponents(
        VoxelVisualComponent up,
        VoxelVisualComponent down,
        VoxelVisualComponent forward,
        VoxelVisualComponent back,
        VoxelVisualComponent left,
        VoxelVisualComponent right)
    {
        Up = up;
        Down = down;
        Forward = forward;
        Back = back;
        Left = left;
        Right = right;
    }
}