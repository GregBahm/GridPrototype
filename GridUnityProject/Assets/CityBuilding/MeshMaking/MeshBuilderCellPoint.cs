using GameGrid;
using UnityEngine;
using VoxelVisuals;

namespace MeshMaking
{
    class MeshBuilderCellPoint : IMeshBuilderPoint
    {
        public DesignationCell Cell { get; }
        public string Key { get; }
        public Vector3 Position { get; }

        public Vector2 Uv { get; } = Vector2.zero;

        public bool IsCellPoint { get; } = true;

        public MeshBuilderCellPoint(DesignationCell cell)
        {
            Cell = cell;
            Key = cell.ToString();
            Position = new Vector3(cell.GroundPoint.Position.x, cell.Height, cell.GroundPoint.Position.y);
        }
        public override string ToString()
        {
            return Key;
        }
    }
}