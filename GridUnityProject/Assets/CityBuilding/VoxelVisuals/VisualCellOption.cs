using System.Collections.Generic;
using UnityEngine;

namespace VoxelVisuals
{
    public class VisualCellOption
    {
        public VisualCellOption(
            ComponentInSet[] components,
            VoxelVisualDesignation designation,
            VoxelConnectionType up,
            VoxelConnectionType down)
        {
            this.Components = components;
            this.Designation = designation;
            this.Up = up;
            this.Down = down;
        }

        public ComponentInSet[] Components { get; }

        public VoxelVisualDesignation Designation { get; }

        public VoxelConnectionType Up { get; }

        public VoxelConnectionType Down { get; }
    }
}