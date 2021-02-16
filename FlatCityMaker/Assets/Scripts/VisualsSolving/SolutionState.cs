using System.Collections.Generic;
using System.Linq;

namespace VisualsSolver
{
    public class SolutionState
    {
        public IEnumerable<CellState> Cells { get { return cellStateLookup.Values; } }
        public bool IsEverythingSolved { get; }

        protected IReadOnlyDictionary<CellConnections, CellState> cellStateLookup;

        protected SolutionState()
        {
            IsEverythingSolved = false;
        }

        private SolutionState(IReadOnlyDictionary<CellConnections, CellState> cellStateLookup)
        {
            this.cellStateLookup = cellStateLookup;
            foreach (CellState item in Cells)
            {
                item.SetStatus(this);
            }
            IsEverythingSolved = Cells.All(item => item.Status != CellStatus.InvalidAndCanDrop);
        }

        public SolutionState GetNextState()
        {
            Dictionary<CellConnections, CellState> newState = cellStateLookup.ToDictionary(item => item.Key, item => item.Value);
            // Each cell with multiple options checks to see whether their current choice is invalid
            // If it is, they check to see whether their choice of tile is higher in priority than their invalid neighbors
            // If it is, then they drop their current available option and the process starts over 
            foreach (CellState cellState in Cells.Where(item => item.Status == CellStatus.InvalidAndCanDrop))
            {
                if (ShouldFallToNextOption(cellState))
                {
                    newState[cellState.Connections] = cellState.FallToNextOption();
                }
            }
            return new SolutionState(newState);

        }

        private bool ShouldFallToNextOption(CellState cellState)
        {
            if (cellState.InvalidNeighborConnections.Any(state => state.Status == CellStatus.InvalidAndCanDrop))
            {
                int highestNeighborPriority = cellState.InvalidNeighborConnections.Max(item => item.CurrentChoice.Priority);
                return cellState.CurrentChoice.Priority <= highestNeighborPriority;
            }
            return true;
        }

        internal CellState GetCellState(CellConnections connections)
        {
            return cellStateLookup[connections];
        }
    }
}