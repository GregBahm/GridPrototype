using GameGrid;
using UnityEngine;

namespace VoxelVisuals
{

    public interface IDesignationCell
    {
        VoxelDesignationType Designation { get; }
        Vector3 Position { get; }
        GroundPoint GroundPoint { get; }
    }
}