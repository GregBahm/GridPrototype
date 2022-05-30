using GameGrid;
using UnityEngine;

namespace VoxelVisuals
{
    public class GroundDesignationCell : IDesignationCell
    {
        public VoxelDesignation Designation => VoxelDesignation.Ground;

        public Vector3 Position { get; }

        public GroundPoint GroundPoint { get; }

        public GroundDesignationCell(GroundPoint groundPoint)
        {
            GroundPoint = groundPoint;
            Position = new Vector3(groundPoint.Position.x, -1f, groundPoint.Position.y);
        }
    }
}