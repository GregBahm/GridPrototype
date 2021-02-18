using GameGrid;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VisualsSolver
{
    public class SolutionState
    {
        public IEnumerable<CellState> Cells { get { return cellStateLookup.Values; } }
        public bool IsEverythingSolved { get; }

        private IReadOnlyDictionary<VoxelVisualComponent, CellState> cellStateLookup;

        public SolutionState(MainGrid grid, OptionsByDesignation optionsSource)
        {
            cellStateLookup = GetInitialDictionary(grid, optionsSource);
            IsEverythingSolved = false;
        }

        private Dictionary<VoxelVisualComponent, CellState> GetInitialDictionary(MainGrid grid, OptionsByDesignation optionsSource)
        {
            Dictionary<VoxelVisualComponent, CellState> ret = new Dictionary<VoxelVisualComponent, CellState>();
            foreach(VoxelVisualComponent component in grid.Voxels.SelectMany(item => item.Visuals.Components))
            {
                VoxelVisualOption[] options = optionsSource.GetOptions(component.GetCurrentDesignation());
                CellState state = new CellState(options, component);
                ret.Add(component, state);
            }
            return ret;
        }

        private SolutionState(IReadOnlyDictionary<VoxelVisualComponent, CellState> cellStateLookup)
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
            Dictionary<VoxelVisualComponent, CellState> newState = cellStateLookup.ToDictionary(item => item.Key, item => item.Value);
            // Each cell with multiple options checks to see whether their current choice is invalid
            // If it is, they check to see whether their choice of tile is higher in priority than their invalid neighbors
            // If it is, then they drop their current available option and the process starts over 
            foreach (CellState cellState in Cells.Where(item => item.Status == CellStatus.InvalidAndCanDrop))
            {
                if (ShouldFallToNextOption(cellState))
                {
                    newState[cellState.Component] = cellState.FallToNextOption();
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

        internal CellState GetCellState(VoxelVisualComponent component)
        {
            if(component == null)
            {
                throw new ArgumentNullException("component");
            }
            return cellStateLookup[component];
        }
    }
}