using GameGrid;
using System;
using UnityEngine;
using VoxelVisuals;

namespace MeshMaking
{
    internal class MeshBuilderConnectionPoint : IMeshBuilderPoint
    {
        public DesignationCell CellA { get; }
        public DesignationCell CellB { get; }
        public string Key { get; }

        public Vector3 Position { get; }

        public Vector2 Uv { get; }

        public bool IsCellPoint { get; } = false;

        public MeshBuilderConnectionPoint(DesignationCell cellA, DesignationCell cellB)
            : this(cellA, cellB, (cellA.Position + cellB.Position) / 2)
        {
            Uv = Vector2.up;
        }
        public MeshBuilderConnectionPoint(DesignationCell cellA, DesignationCell cellB, Vector3 position)
        {
            bool cellAFirst = GetIsCellAFirst(cellA, cellB);
            CellA = cellAFirst ? cellA : cellB;
            CellB = cellAFirst ? cellB : cellA;
            Key = GetKey();
            Position = position;
            Uv = Vector2.one;
        }

        private bool GetIsCellAFirst(DesignationCell cellA, DesignationCell cellB)
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