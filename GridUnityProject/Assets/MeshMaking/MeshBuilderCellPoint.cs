using GameGrid;
using UnityEngine;

namespace MeshMaking
{
    class MeshBuilderCellPoint : IMeshBuilderPoint
    {
        public VoxelCell Cell { get; }
        public string Key { get; }
        public Vector3 Position { get; }

        public MeshBuilderCellPoint(VoxelCell cell)
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