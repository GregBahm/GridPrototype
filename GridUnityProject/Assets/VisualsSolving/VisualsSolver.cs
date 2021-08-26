using GameGrid;
using System.Collections.Generic;
using System.Linq;

namespace VisualsSolving
{
    public class VisualsSolver
    {
        private Dictionary<VoxelVisualComponent, CellState> cellStateLookup;
        public readonly HashSet<CellState> unsolvedCells;
        public readonly HashSet<CellState> dirtyCells = new HashSet<CellState>();
        public List<CellState> ReadyToDisplayVoxels { get; } = new List<CellState>();

        public bool SolveComplete
        {
            get
            {
                return unsolvedCells.Count == 0;
            }
        }

        public CellState this[VoxelVisualComponent component]
        {
            get { return cellStateLookup[component]; }
        }

        public VisualsSolver(MainGrid grid, OptionsByDesignation optionsSource)
        {
            cellStateLookup = GetInitialDictionary(grid, optionsSource);
            foreach (CellState item in cellStateLookup.Values)
            {
                if(item.RemainingOptions.Count == 1)
                {
                    ReadyToDisplayVoxels.Add(item);
                }
                else
                {
                    dirtyCells.Add(item);
                }
            }
            unsolvedCells = new HashSet<CellState>(dirtyCells);
        }

        public void StepForward()
        {
            if(dirtyCells.Any())
            {
                SudokuACell();
            }
            else
            {
                CollapseACell();
            }
        }

        // Takes the first dirty cell
        // Removes each option from the cell that is no longe possible
        // If this changes the possibility of the cell, its neighbors are made dirty
        private void SudokuACell()
        {
            CellState dirtyCell = dirtyCells.First();
            dirtyCells.Remove(dirtyCell);
            unsolvedCells.Remove(dirtyCell);
            CellState cleanCell = dirtyCell.GetReducedOptions();
            cellStateLookup[cleanCell.Component] = cleanCell;
            if (cleanCell.RemainingOptions.Count > 1)
            {
                unsolvedCells.Add(cleanCell);
            }
            else
            {
                ReadyToDisplayVoxels.Add(cleanCell);
            }
            foreach (CellState cell in cleanCell.GetNewDirtyCells(dirtyCell))
            {
                dirtyCells.Add(cell);
            }
        }

        // If we've sudokued way every invalid option, we collapse an uncollapsed cell to its first choice
        // We then dirty its neighbors if necessary
        private void CollapseACell()
        {
            CellState toCollapse = unsolvedCells.First();
            unsolvedCells.Remove(toCollapse);
            dirtyCells.Remove(toCollapse);
            CellState collapsed = toCollapse.GetCollapsed();
            cellStateLookup[collapsed.Component] = collapsed;
            ReadyToDisplayVoxels.Add(collapsed);
            foreach (CellState cell in collapsed.GetNewDirtyCells(toCollapse))
            {
                dirtyCells.Add(cell);
            }
        }

        private Dictionary<VoxelVisualComponent, CellState> GetInitialDictionary(MainGrid grid, OptionsByDesignation optionsSource)
        {
            Dictionary<VoxelVisualComponent, CellState> ret = new Dictionary<VoxelVisualComponent, CellState>();
            foreach (VoxelVisualComponent component in grid.Voxels.SelectMany(item => item.Visuals.Components))
            {
                VoxelVisualOption[] options = optionsSource.GetOptions(component.GetCurrentDesignation());
                CellState state = new CellState(this, options, component);
                ret.Add(component, state);
            }
            return ret;
        }


        public bool IsValid(VoxelVisualOption option, VoxelVisualComponent component)
        {
            return (component.Neighbors.Up == null ||
                cellStateLookup[component.Neighbors.Up].ConnectsDown(option.Connections.Up))
                && (component.Neighbors.Down == null ||
                cellStateLookup[component.Neighbors.Down].ConnectsUp(option.Connections.Down))
                && (component.Neighbors.Left == null ||
                cellStateLookup[component.Neighbors.Left].ConnectsRight(option.Connections.Left))
                && (component.Neighbors.Right == null ||
                cellStateLookup[component.Neighbors.Right].ConnectsLeft(option.Connections.Right))
                && (component.Neighbors.Forward == null ||
                cellStateLookup[component.Neighbors.Forward].ConnectsBack(option.Connections.Forward))
                && (component.Neighbors.Backward == null ||
                cellStateLookup[component.Neighbors.Backward].ConnectsForward(option.Connections.Back));
        }
    }
}