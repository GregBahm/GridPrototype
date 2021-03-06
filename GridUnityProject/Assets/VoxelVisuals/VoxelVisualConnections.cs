﻿using System;

public class VoxelVisualConnections
{
    public VoxelConnectionType Up { get; }
    public VoxelConnectionType Down { get; }
    
    public VoxelConnectionType Left { get; }
    public VoxelConnectionType Right { get; }

    public VoxelConnectionType Forward { get; }
    public VoxelConnectionType Back { get; }

    public VoxelVisualConnections(VoxelConnectionType up,
        VoxelConnectionType down,
        VoxelConnectionType left,
        VoxelConnectionType right,
        VoxelConnectionType forward,
        VoxelConnectionType back)
    {
        Up = up;
        Down = down;
        Left = left;
        Right = right;
        Forward = forward;
        Back = back;
    }

    internal VoxelVisualConnections GetVarient(bool wasFlipped, int rotations)
    {
        VoxelConnectionType left = Left;
        VoxelConnectionType right = Right;
        VoxelConnectionType forward = Forward;
        VoxelConnectionType back = Back;

        if(wasFlipped)
        {
            left = Right;
            right = Left;
        }
        for (int i = 0; i < rotations; i++)
        {
            VoxelConnectionType oldLeft = left;
            VoxelConnectionType oldForward = forward;
            VoxelConnectionType oldRight = right;
            VoxelConnectionType oldBack = back;

            left = oldBack;
            forward = oldLeft;
            right = oldForward;
            back = oldRight;
        }

        return new VoxelVisualConnections(Up, Down, left, right, forward, back);
    }
}