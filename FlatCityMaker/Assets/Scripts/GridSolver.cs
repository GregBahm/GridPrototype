using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TileDefinition;

namespace GridSolver
{
    public class Solver
    {
        public SolverCell[,] cells;

        public void Solve()
        {
            GridState state = CreateInitialState();
            GridState solved = RecursivelySolve(state);
        }

        private GridState CreateInitialState()
        {
            throw new NotImplementedException();
        }

        private GridState RecursivelySolve(GridState gridState)
        {
            GridCellState unsolvedCell = gridState.UnsolvedStates.First();
            foreach (Tile choice in unsolvedCell.AvailableOptions)
            {
                GridState state = gridState.GetWithChoiceApplied(choice);
                if(state != null) // A grid cell was left with no valid options
                {
                    if (!state.UnsolvedStates.Any())
                    {
                        return gridState;
                    }
                    return RecursivelySolve(state);
                }
            }
            throw new Exception("This grid appears to be unsolveable");
        }
    }

    public class SolverCell
    {
        public IEnumerable<Tile> BaseOptions { get; }

        public IEnumerable<SolverCellNeighbor> Neighbors { get; }
        
        public class SolverCellNeighbor
        { }
    }

    public class GridState
    {
        public GridCellState[,] grid;
        public IReadOnlyCollection<GridCellState> UnsolvedStates { get; }

        public GridState(GridCellState[,] grid, IReadOnlyCollection<GridCellState> unsolvedStates)
        {
            this.grid = grid;
            UnsolvedStates = unsolvedStates;
        }
        
        public GridState GetWithChoiceApplied(Tile choice)
        {
            GridCellState[,] newGrid = (GridCellState[,])grid.Clone();
            GridCellState first = UnsolvedStates.First();
            GridCellState newState = new GridCellState(first.X, first.Y, choice);
            newGrid[newState.X, newState.Y] = newState;

            List<GridCellState> newUnsolved = new List<GridCellState>();
            foreach (GridCellState unsolvedCell in UnsolvedStates.Skip(1))
            {
                // TODO: Optimize to only update neighbors
                GridCellState updatedCell = GetUpdatedCell(unsolvedCell, newGrid);
                if(updatedCell.Status == GridCellState.StateStatus.Unsolveable)
                {
                    return null;
                }

                newGrid[updatedCell.X, updatedCell.Y] = updatedCell;

                if(updatedCell.Status == GridCellState.StateStatus.Unsolved)
                {
                    newUnsolved.Add(updatedCell);
                }
            }
            return new GridState(newGrid, newUnsolved);
        }

        private GridCellState GetUpdatedCell(GridCellState unsolvedCell, GridCellState[,] newGrid)
        {
            List<Tile> newValidOptions = unsolvedCell.AvailableOptions.Where(option => OptionIsValid(unsolvedCell.X, unsolvedCell.Y, option, newGrid)).ToList();
            if(newValidOptions.Count == 1)
            {
                return new GridCellState(unsolvedCell.X, unsolvedCell.Y, newValidOptions[0]);
            }
            return new GridCellState(unsolvedCell.X, unsolvedCell.Y, newValidOptions);
        }

        private bool OptionIsValid(int x, int y, Tile option, GridCellState[,] newGrid)
        {
            throw new NotImplementedException();
        }
    }

    public class GridCellState
    {
        public int X { get; }
        public int Y { get; }
        public Tile Choice { get; }
        public IEnumerable<Tile> AvailableOptions { get; }
        public StateStatus Status { get; }

        public GridCellState(int x, int y, IEnumerable<Tile> options)
        {
            X = x;
            Y = y;
            AvailableOptions = options;
            Status = AvailableOptions.Any() ? StateStatus.Unsolved : StateStatus.Unsolveable;
        }

        public GridCellState(int x, int y, Tile choice)
        {
            X = x;
            Y = y;
            Choice = choice;
            Status = StateStatus.Solved;
        }

        public enum StateStatus
        {
            Solved,
            Unsolved,
            Unsolveable
        }
    }
}