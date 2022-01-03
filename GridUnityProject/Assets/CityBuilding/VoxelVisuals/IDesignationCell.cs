using GameGrid;
using UnityEngine;

public interface IDesignationCell
{
    VoxelDesignationType Designation { get; }
    Vector3 Position { get; }
    GroundPoint GroundPoint { get; }
}
