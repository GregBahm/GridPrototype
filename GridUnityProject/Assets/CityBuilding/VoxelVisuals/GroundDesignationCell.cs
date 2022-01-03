using GameGrid;
using UnityEngine;

public class GroundDesignationCell : IDesignationCell
{
    public VoxelDesignationType Designation => VoxelDesignationType.Ground;

    public Vector3 Position { get; }

    public GroundPoint GroundPoint { get; }

    public GroundDesignationCell(GroundPoint groundPoint)
    {
        GroundPoint = groundPoint;
        Position = new Vector3(groundPoint.Position.x, -1f, groundPoint.Position.y);
    }
}