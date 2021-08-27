using GameGrid;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VisualsSolving
{
    public class VisualsSolver
    {
        private readonly VoxelConnectionType groundConnection;
        private Dictionary<VoxelVisualComponent, CellState> cellStateLookup;
        private readonly HashSet<CellState> unsolvedCells;
        private HashSet<CellState> currentDirtyCells = new HashSet<CellState>();
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

        public VisualsSolver(MainGrid grid, OptionsByDesignation optionsSource, VoxelConnectionType groundConnection)
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
            this.groundConnection = groundConnection;
        }

        public void StepForward()
        {
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
                if(cleanCell.RemainingOptions.Count == 0)
                {
                    throw new Exception("No options remaining for cell!");
                }
                ReadyToDisplayVoxels.Add(cleanCell);
            }
            foreach (CellState cell in cleanCell.GetNewDirtyCells(dirtyCell))
            {
                currentDirtyCells.Add(cell);
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
                    takeColumn = takeColumn || voxel.Visuals.Components.Any(item => item.Contents != null);
                    if (takeColumn || i == 0)
                        yield return voxel;
                }
            }
        }

        public bool HasConnection(VoxelVisualComponent neighbor)
        {
            return neighbor != null && cellStateLookup.ContainsKey(neighbor);
        }

        private bool IsValid(VoxelVisualComponent neighbor, VoxelConnectionType connectionType, CellState.ConnectionDirection direction)
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
                else if (connectionType != null)
                {
                    return false;
                }
            }else if(direction == CellState.ConnectionDirection.Down)
            {
                return connectionType == groundConnection;
            }
            return true;
        }

        public bool IsValid(VoxelVisualOption option, VoxelVisualComponent component)
        {
            if(!IsValid(component.Neighbors.Up, option.Connections.Up, CellState.ConnectionDirection.Up))
                return false;

            if (!IsValid(component.Neighbors.Down, option.Connections.Down, CellState.ConnectionDirection.Down))
                return false;

            if (!IsValid(component.Neighbors.Left, option.Connections.Left, CellState.ConnectionDirection.Left))
                return false;

            if (!IsValid(component.Neighbors.Right, option.Connections.Right, CellState.ConnectionDirection.Right))
                return false;

            if (!IsValid(component.Neighbors.Forward, option.Connections.Forward, CellState.ConnectionDirection.Forward))
                return false;

            if (!IsValid(component.Neighbors.Back, option.Connections.Back, CellState.ConnectionDirection.Back))
                return false;

            return true;
        }
    }
}