using GameGrid;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VisualsSolving
{
    public class VisualsSolver
    {
        private Dictionary<VisualCell, CellState> cellStateLookup;
        private readonly HashSet<CellState> unsolvedCells;
        private HashSet<CellState> dirtyCells = new HashSet<CellState>();
        public List<CellState> ReadyToDisplayVoxels { get; } = new List<CellState>();

        public bool SolveComplete
        {
            get
            {
                return unsolvedCells.Count == 0 && dirtyCells.Count == 0;
            }
        }

        public CellState this[VisualCell component]
        {
            get { return cellStateLookup[component]; }
        }

        public VisualsSolver(MainGrid grid, VisualOptionsByDesignation optionsSource)
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
                if(cleanCell.RemainingOptions.Count == 0)
                {
                    throw new Exception("No options remaining for cell!");
                }
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

        private Dictionary<VisualCell, CellState> GetInitialDictionary(MainGrid grid, VisualOptionsByDesignation optionsSource)
        {
            Dictionary<VisualCell, CellState> ret = new Dictionary<VisualCell, CellState>();
            IEnumerable<DesignationCell> voxelsToSolve = GetVoxelsToSolve(grid); 
            foreach (VisualCell visualCell in voxelsToSolve.SelectMany(item => item.Visuals))
            {
                if(!ret.ContainsKey(visualCell))
                {
                    VisualCellOption[] options = optionsSource.GetOptions(visualCell.GetCurrentDesignation());
                    CellState state = new CellState(this, options, visualCell);
                    ret.Add(visualCell, state);
                }
            }
            return ret;
        }
        
        // There's no point in solving every empty voxel
        // So we only solve voxels with a designation or somewhere below a designation
        private IEnumerable<DesignationCell> GetVoxelsToSolve(MainGrid grid)
        {
            HashSet<DesignationCell> ret = new HashSet<DesignationCell>();
            foreach (GroundPoint point in grid.Points)
            {
                bool takeColumn = false;
                for (int i = MainGrid.MaxHeight - 1; i >= 0; i--)
                {
                    DesignationCell voxel = point.DesignationCells[i];
                    takeColumn = takeColumn || voxel.Visuals.Any(item => item.Contents != null && item.Contents.Mesh != null);
                    if (takeColumn)
                        ret.Add(voxel);
                }
            }
            return ret;
        }

        public bool HasConnection(VisualCell neighbor)
        {
            return neighbor != null && cellStateLookup.ContainsKey(neighbor);
        }

        private bool IsValid(VisualCell neighbor, VoxelConnectionType connectionType, CellState.ConnectionDirection direction)
        {
            if (neighbor != null)
            {
                if (cellStateLookup.ContainsKey(neighbor))
                {
                    if (!cellStateLookup[neighbor].Connects(connectionType, direction))
                    {
                        return false;
                    }
                }
                else if (connectionType != VoxelConnectionType.None)
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsValid(VisualCellOption option, VisualCell component)
        {
            if(!IsValid(component.Neighbors.Up, option.Connections.Up, CellState.ConnectionDirection.Up))
                return false;

            if (!IsValid(component.Neighbors.Down, option.Connections.Down, CellState.ConnectionDirection.Down))
                return false;

            return true;
        }
    }
}