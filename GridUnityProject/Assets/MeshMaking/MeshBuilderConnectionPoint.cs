using GameGrid;
using System;
using UnityEngine;

namespace MeshMaking
{
    internal class MeshBuilderConnectionPoint : IMeshBuilderPoint
    {
        public VoxelCell CellA { get; }
        public VoxelCell CellB { get; }
        public string Key { get; }

        public Vector3 Position { get; }

        public Vector2 Uv { get; } = Vector2.zero;

        public MeshBuilderConnectionPoint(VoxelCell cellA, VoxelCell cellB)
            : this(cellA, cellB, (cellA.CellPosition + cellB.CellPosition) / 2)
        { }
        public MeshBuilderConnectionPoint(VoxelCell cellA, VoxelCell cellB, Vector3 position)
        {
            bool cellAFirst = GetIsCellAFirst(cellA, cellB);
            CellA = cellAFirst ? cellA : cellB;
            CellB = cellAFirst ? cellB : cellA;
            Key = GetKey();
            Position = position;
        }

        private bool GetIsCellAFirst(VoxelCell cellA, VoxelCell cellB)
        {
            if (cellA.GroundPoint.Index == cellB.GroundPoint.Index)
            {
                if (cellA.Height == cellB.Height)
                {
                    throw new Exception("Cannot make MeshBuilderPoint from two cells at the same voxel location.");
                }
                return cellA.Height < cellB.Height;
            }
            return cellA.GroundPoint.Index < cellB.GroundPoint.Index;
        }

        private string GetKey()
        {
            return CellA.ToString() + " - " + CellB.ToString();
        }
        public override string ToString()
        {
            return Key;
        }
    }
}