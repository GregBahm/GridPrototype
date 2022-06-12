using GameGrid;
using UnityEngine;

namespace VoxelVisuals
{
    public class ShellDesignationCell : IDesignationCell
    {
        public Designation Designation => Designation.Shell;

        public Vector3 Position { get; }

        public GroundPoint GroundPoint { get; }

        public ShellDesignationCell(GroundPoint groundPoint)
        {
            GroundPoint = groundPoint;
            Position = new Vector3(groundPoint.Position.x, -1f, groundPoint.Position.y);
        }
    }
}