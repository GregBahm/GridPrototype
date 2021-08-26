using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class NeighborComponents
{
    public VoxelVisualComponent Up { get; }
    public VoxelVisualComponent Down { get; }
    public VoxelVisualComponent Forward { get; }
    public VoxelVisualComponent Backward { get; }
    public VoxelVisualComponent Left { get; }
    public VoxelVisualComponent Right { get; }

    public NeighborComponents(
        VoxelVisualComponent up,
        VoxelVisualComponent down,
        VoxelVisualComponent forward,
        VoxelVisualComponent backward,
        VoxelVisualComponent left,
        VoxelVisualComponent right)
    {
        Up = up;
        Down = down;
        Forward = forward;
        Backward = backward;
        Left = left;
        Right = right;
    }
}