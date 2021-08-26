using GameGrid;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VisualsSolving
{
    public class VisualsSolver
    {
        private Dictionary<VoxelVisualComponent, CellState> cellStateLookup;
        public readonly HashSet<CellState> unsolvedCells;
        public HashSet<CellState> currentDirtyCells = new HashSet<CellState>();
        private HashSet<CellState> nextDirtyCells = new HashSet<CellState>();
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
                    currentDirtyCells.Add(item);
                }
            }
            unsolvedCells = new HashSet<CellState>(currentDirtyCells);
        }

        public void StepForward()
        {
            if(!currentDirtyCells.Any() && nextDirtyCells.Any())
            {
                currentDirtyCells = nextDirtyCells;
                nextDirtyCells = new HashSet<CellState>();
            }
            if(currentDirtyCells.Any())
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
            CellState dirtyCell = currentDirtyCells.First();
            currentDirtyCells.Remove(dirtyCell);
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
                nextDirtyCells.Add(cell);
            }
        }

        // If we've sudokued way every invalid option, we collapse an uncollapsed cell to its first choice
        // We then dirty its neighbors if necessary
        private void CollapseACell()
        {
            CellState toCollapse = unsolvedCells.First();
            unsolvedCells.Remove(toCollapse);
            currentDirtyCells.Remove(toCollapse);
            CellState collapsed = toCollapse.GetCollapsed();
            cellStateLookup[collapsed.Component] = collapsed;
            ReadyToDisplayVoxels.Add(collapsed);
            foreach (CellState cell in collapsed.GetNewDirtyCells(toCollapse))
            {
                currentDirtyCells.Add(cell);
            }
        }

        private Dictionary<VoxelVisualComponent, CellState> GetInitialDictionary(MainGrid grid, OptionsByDesignation optionsSource)
        {
            Dictionary<VoxelVisualComponent, CellState> ret = new Dictionary<VoxelVisualComponent, CellState>();
            IEnumerable<VoxelCell> voxelsToSolve = GetVoxelsToSolve(grid); 
            foreach (VoxelVisualComponent component in voxelsToSolve.SelectMany(item => item.Visuals.Components))
            {
                VoxelVisualOption[] options = optionsSource.GetOptions(component.GetCurrentDesignation());
                CellState state = new CellState(this, options, component);
                ret.Add(component, state);
            }
            return ret;
        }
        
        // There's no point in solving every empty voxel
        // So we only solve voxels with a designation or somewhere below a designation
        private IEnumerable<VoxelCell> GetVoxelsToSolve(MainGrid grid)
        {
            foreach (GroundPoint point in grid.Points)
            {
                bool takeColumn = false;
                for (int i = MainGrid.VoxelHeight - 1; i >= 0; i--)
                {
                    VoxelCell voxel = point.Voxels[i];
                    takeColumn = takeColumn || voxel.Filled;
                    if (takeColumn)
                        yield return voxel;
                }
            }
        }

        public bool HasConnection(VoxelVisualComponent neighbor)
        {
            return neighbor != null && cellStateLookup.ContainsKey(neighbor);
        }

        public bool IsValid(VoxelVisualOption option, VoxelVisualComponent component)
        {
            return (!HasConnection(component.Neighbors.Up) ||
                    cellStateLookup[component.Neighbors.Up].ConnectsDown(option.Connections.Up))
                && (!HasConnection(component.Neighbors.Down) ||
                    cellStateLookup[component.Neighbors.Down].ConnectsUp(option.Connections.Down))
                && (!HasConnection(component.Neighbors.Left) ||
                    cellStateLookup[component.Neighbors.Left].ConnectsRight(option.Connections.Left))
                && (!HasConnection(component.Neighbors.Right) ||
                    cellStateLookup[component.Neighbors.Right].ConnectsLeft(option.Connections.Right))
                && (!HasConnection(component.Neighbors.Forward) ||
                    cellStateLookup[component.Neighbors.Forward].ConnectsBack(option.Connections.Forward))
                && (!HasConnection(component.Neighbors.Backward) ||
                    cellStateLookup[component.Neighbors.Backward].ConnectsForward(option.Connections.Back));
        }
    }
}