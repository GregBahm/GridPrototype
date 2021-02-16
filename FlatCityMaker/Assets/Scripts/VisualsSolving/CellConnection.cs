using System;
using TileDefinition;

namespace VisualsSolver
{
    public class CellConnection
    {
        private readonly Func<Tile, Tile, bool> comparisonFunction;
        public CellConnections Cell { get; }
        public CellConnection(CellConnections cell, Func<Tile, Tile, bool> comparisonFunction)
        {
            Cell = cell;
            this.comparisonFunction = comparisonFunction;
        }

        internal bool IsValid(Tile myChoice, Tile theirChoice)
        {
            return comparisonFunction(myChoice, theirChoice);
        }
    }
}