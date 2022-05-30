using GameGrid;
using UnityEngine;

namespace VoxelVisuals
{
    public interface IDesignationCell
    {
        VoxelDesignation Designation { get; }
        Vector3 Position { get; }
        GroundPoint GroundPoint { get; }
    }
}