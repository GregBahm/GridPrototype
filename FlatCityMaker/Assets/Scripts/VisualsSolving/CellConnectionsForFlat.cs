using System.Collections.Generic;
using System.Linq;
using TileDefinition;

namespace VisualsSolver
{
    public class CellConnectionsForFlat : CellConnections // This won't be applicable to the grid
    {
        public int X { get; }
        public int Y { get; }

        private IEnumerable<CellConnection> neighbors;

        public CellConnectionsForFlat(int x, int y)
            : base()
        {
            X = x;
            Y = y;
        }

        public void SetNeighbors(int cellX, int cellY, CellConnections[,] cells, int gridWidth, int gridHeight)
        {
            neighbors = GetNeighbors(cellX, cellY, cells, gridWidth, gridHeight).ToList();
        }

        private IEnumerable<CellConnection> GetNeighbors(int cellX, int cellY, CellConnections[,] cells, int gridWidth, int gridHeight)
        {
            if (cellX > 0)
            {
                CellConnections neighbor = cells[cellX - 1, cellY];
                yield return new RightNeighbor(neighbor);
            }
            if (cellX < gridWidth - 1)
            {
                CellConnections neighbor = cells[cellX + 1, cellY];
                yield return new LeftNeighbor(neighbor);
            }
            if (cellY > 0)
            {
                CellConnections neighbor = cells[cellX, cellY - 1];
                yield return new DownNeighbor(neighbor);
            }
            if (cellY < gridHeight - 1)
            {
                CellConnections neighbor = cells[cellX, cellY + 1];
                yield return new UpNeighbor(neighbor);
            }
        }

        public override IEnumerator<CellConnection> GetEnumerator()
        {
            return neighbors.GetEnumerator();
        }

        public class LeftNeighbor : CellConnection
        {
            public LeftNeighbor(CellConnections neighborCell)
                : base(neighborCell, Connects)
            { }

            private static bool Connects(Tile option, Tile neighborOption)
            {
                return option.Left == neighborOption.Right;
            }
        }

        public class RightNeighbor : CellConnection
        {
            public RightNeighbor(CellConnections neighborCell)
                : base(neighborCell, Connects)
            { }

            private static bool Connects(Tile option, Tile neighborOption)
            {
                return option.Right == neighborOption.Left;
            }
        }

        public class UpNeighbor : CellConnection
        {
            public UpNeighbor(CellConnections neighborCell)
                : base(neighborCell, Connects)
            { }

            private static bool Connects(Tile option, Tile neighborOption)
            {
                return option.Up == neighborOption.Down;
            }
        }

        public class DownNeighbor : CellConnection
        {
            public DownNeighbor(CellConnections neighborCell)
                : base(neighborCell, Connects)
            { }


            private static bool Connects(Tile option, Tile neighborOption)
            {
                return option.Down == neighborOption.Up;
            }
        }
    }
}