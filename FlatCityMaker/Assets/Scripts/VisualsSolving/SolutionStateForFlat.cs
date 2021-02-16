using Packages.Rider.Editor.PostProcessors;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Dynamic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using TileDefinition;
using UnityEngine.UIElements;

namespace VisualsSolver
{

    public class SolutionStateForFlat : SolutionState
    {
        public SolutionStateForFlat(VisualsSolution solver, MainGrid grid)
            : base()
        {
            Dictionary<CellConnections, CellState> initialLookup = new Dictionary<CellConnections, CellState>();

            CellConnectionsForFlat[,] asGrid = new CellConnectionsForFlat[grid.Width, grid.Height];

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    GridCell item = grid.Cells[x, y];
                    CellConnectionsForFlat solverCell = new CellConnectionsForFlat(x, y);
                    asGrid[item.X, item.Y] = solverCell;
                }
            }
            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    CellConnectionsForFlat connections = asGrid[x, y];
                    connections.SetNeighbors(x, y, asGrid, grid.Width, grid.Height);
                    IEnumerable<Tile> options = grid.Cells[x, y].OptionsFromDesignation;
                    CellState cellState = new CellState(options, connections);
                    initialLookup.Add(connections, cellState);
                }
            }
            this.cellStateLookup = initialLookup;
            foreach (CellState cellState in Cells)
            {
                cellState.SetStatus(this);
            }
        }
    }
}