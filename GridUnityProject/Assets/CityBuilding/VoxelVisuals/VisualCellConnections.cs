using System;

public class VisualCellConnections
{
    public VoxelConnectionType Up { get; }
    public VoxelConnectionType Down { get; }

    public VisualCellConnections(VoxelConnectionType up,
        VoxelConnectionType down)
    {
        Up = up;
        Down = down;
    }
}