using GameGrid;
using UnityEngine;

namespace VoxelVisuals
{
    public interface IDesignationCell
    {
        Designation Designation { get; }
        Vector3 Position { get; }
        GroundPoint GroundPoint { get; }
    }
}