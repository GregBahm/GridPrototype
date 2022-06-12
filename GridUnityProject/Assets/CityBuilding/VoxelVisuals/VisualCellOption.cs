using System.Collections.Generic;
using UnityEngine;

namespace VoxelVisuals
{
    public class VisualCellOption
    {
        public Mesh Mesh { get; }
        public Material[] Materials { get; }
        private readonly Designation[,,] designation;

        public int Priority { get; }

        public bool Flipped { get; }
        public int Rotations { get; }

        public VisualCellConnections Connections { get; }

        public VisualCellOption(Mesh mesh,
            Material[] materials,
            Designation[,,] designation,
            bool flipped,
            int rotations,
            int priority,
            VisualCellConnections connections)
        {
            Mesh = mesh;
            Materials = materials;
            this.designation = designation;
            Flipped = flipped;
            Rotations = rotations;
            Priority = priority;
            Connections = connections;
        }

        public string GetDesignationKey()
        {
            return VoxelVisualDesignation.GetDesignationKey(designation);
        }

        public override string ToString()
        {
            return Mesh?.name;
        }
    }
}