using System;

namespace VisualsSolver
{
    public class CellConnection
    {
        private readonly Func<VoxelVisualOption, VoxelVisualOption, bool> comparisonFunction;
        public VoxelVisualComponent Cell { get; }
        public CellConnection(VoxelVisualComponent cell, Func<VoxelVisualOption, VoxelVisualOption, bool> comparisonFunction)
        {
            Cell = cell;
            this.comparisonFunction = comparisonFunction;
        }

        internal bool IsValid(VoxelVisualOption myChoice, VoxelVisualOption theirChoice)
        {
            return comparisonFunction(myChoice, theirChoice);
        }
    }
}