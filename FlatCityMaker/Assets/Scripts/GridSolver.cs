using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TileDefinition;
using UnityEngine;

namespace GridSolver
{
    public class Solver
    {
        private readonly SolverCell[,] cells;

        public List<GridState> SolverHistory { get; } = new List<GridState>();

        public Solver(int width, int height)
        {
            cells = CreateSolverCells(width, height);
        }

        private SolverCell[,] CreateSolverCells(int width, int height)
        {
            SolverCell[,] ret = new SolverCell[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    ret[x, y] = new SolverCell(x, y, width, height);
                }
            }
            return ret;
        }

        public SolverCell GetCell(int x, int y)
        {
            return cells[x, y];
        }

        public GridState GetSolved(MainGrid mainGrid)
        {
            SolverHistory.Clear();
            GridState state = CreateInitialState(mainGrid);
            return RecursivelySolve(state);
        }

        private GridState CreateInitialState(MainGrid mainGrid)
        {
            GridCellState[,] cells = new GridCellState[mainGrid.Width, mainGrid.Height];
            List<GridCellState> cellsToSolve = new List<GridCellState>();
            for (int x = 0; x < mainGrid.Width; x++)
            {
                for (int y = 0; y < mainGrid.Height; y++)
                {
                    IEnumerable<Tile> options = mainGrid.Cells[x, y].OptionsFromDesignation;
                    GridCellState state = new GridCellState(x, y, options);
                    cells[x, y] = state;
                    if(state.Status == GridCellState.StateStatus.Unsolved)
                    {
                        cellsToSolve.Add(state);
                    }
                }
            }
            return new GridState(cells, cellsToSolve, this);
        }

        private GridState RecursivelySolve(GridState oldState)
        {
            if(!oldState.UnsolvedStates.Any())
            {
                return oldState;
            }
            GridCellState unsolvedCell = oldState.UnsolvedStates.First();
            foreach (Tile choice in unsolvedCell.AvailableOptions
                .Where(option => oldState.OptionIsValid(unsolvedCell.X, unsolvedCell.Y, option)))
            {
                GridState newState = oldState.GetWithChoiceApplied(choice);
                SolverHistory.Add(newState);
                if(newState != null) // A grid cell was left with no valid options
                {
                    if (!newState.UnsolvedStates.Any())
                    {
                        return newState;
                    }
                    return RecursivelySolve(newState);
                }
            }
            Debug.LogError("Error: Failed to solve the board");
            return null;
        }
    }

    public class SolverCell
    {
        public IEnumerable<SolverCellNeighbor> Neighbors { get; }

        public SolverCell(int x, int y, int gridWidth, int gridHeight)
        {
            List<SolverCellNeighbor> mappings = new List<SolverCellNeighbor>();
            if (x > 0)
                mappings.Add(new RightNeighbor(x - 1, y));
            if (x < gridWidth - 1)
                mappings.Add(new LeftNeighbor(x + 1, y));
            if (y > 0)
                mappings.Add(new DownNeighbor(x, y - 1));
            if (y < gridHeight - 1)
                mappings.Add(new UpNeighbor(x, y + 1));
            Neighbors = mappings;
        }

        public class LeftNeighbor : SolverCellNeighbor
        {
            public LeftNeighbor(int x, int y) : base(x, y) { }

            public override bool Connects(Tile option, Tile neighborOption)
            {
                return option.Left == neighborOption.Right;
            }
        }

        public class RightNeighbor : SolverCellNeighbor
        {
            public RightNeighbor(int x, int y) : base(x, y) { }

            public override bool Connects(Tile option, Tile neighborOption)
            {
                return option.Right == neighborOption.Left;
            }
        }

        public class UpNeighbor : SolverCellNeighbor
        {
            public UpNeighbor(int x, int y) : base(x, y) { }

            public override bool Connects(Tile option, Tile neighborOption)
            {
                return option.Up == neighborOption.Down;
            }
        }

        public class DownNeighbor : SolverCellNeighbor
        {
            public DownNeighbor(int x, int y) : base(x, y) { }

            public override bool Connects(Tile option, Tile neighborOption)
            {
                return option.Down == neighborOption.Up;
            }
        }
    }

    public abstract class SolverCellNeighbor
    {
        public int X { get; }
        public int Y { get; }

        public SolverCellNeighbor(int x, int y)
        {
            X = x;
            Y = y;
        }

        public abstract bool Connects(Tile option, Tile neighborOption);
    }

    public class GridState
    {
        private readonly Solver solver;
        public GridCellState[,] Cells { get; }
        public IReadOnlyCollection<GridCellState> UnsolvedStates { get; }

        public GridState(GridCellState[,] grid, IReadOnlyCollection<GridCellState> unsolvedStates, Solver solver)
        {
            this.solver = solver;
            this.Cells = grid;
            UnsolvedStates = unsolvedStates;
        }
        
        public GridState GetWithChoiceApplied(Tile choice)
        {
            GridCellState[,] newGrid = (GridCellState[,])Cells.Clone();
            GridCellState first = UnsolvedStates.First();
            GridCellState newState = new GridCellState(first.X, first.Y, choice);
            newGrid[newState.X, newState.Y] = newState;


            HashSet<GridCellState> cellsToUpdate = new HashSet<GridCellState>(GetUnsolvedNeighborsOf(newState, newGrid));
            while(cellsToUpdate.Any())
            {
                GridCellState cellToUpdate = cellsToUpdate.First();
                cellsToUpdate.Remove(cellToUpdate);
                GridCellState updatedCell = GetUpdatedCell(cellToUpdate, newGrid);
                if (updatedCell.Status == GridCellState.StateStatus.Unsolveable)
                {
                    return null;
                }
                newGrid[updatedCell.X, updatedCell.Y] = updatedCell;
                if (updatedCell.Status == GridCellState.StateStatus.Solved)
                {
                    IEnumerable<GridCellState> newCellsToUpdate = GetUnsolvedNeighborsOf(updatedCell, newGrid);
                    foreach (GridCellState item in newCellsToUpdate)
                    {
                        cellsToUpdate.Add(item);
                    }
                }
            }
            List<GridCellState> newUnsolved = GetNewUnsolved(newGrid).ToList();
            return new GridState(newGrid, newUnsolved, solver);
        }

        private IEnumerable<GridCellState> GetNewUnsolved(GridCellState[,] newGrid)
        {
            foreach (GridCellState oldState in UnsolvedStates)
            {
                GridCellState newState = newGrid[oldState.X, oldState.Y];
                if(newState.Status == GridCellState.StateStatus.Unsolved)
                {
                    yield return newState;
                }
            }
        }

        private IEnumerable<GridCellState> GetUnsolvedNeighborsOf(GridCellState newState, GridCellState[,] newGrid)
        {
            SolverCell cell = solver.GetCell(newState.X, newState.Y);
            foreach (SolverCellNeighbor neighbor in cell.Neighbors)
            {
                GridCellState state = newGrid[neighbor.X, neighbor.Y];
                if(state.Status == GridCellState.StateStatus.Unsolved)
                {
                    yield return state;
                }
            }
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
        public bool OptionIsValid(int x, int y, Tile option)
        {
            SolverCell solverCell = solver.GetCell(x, y);
            foreach (SolverCellNeighbor neighbor in solverCell.Neighbors)
            {
                GridCellState neighborState = Cells[neighbor.X, neighbor.Y];
                if (neighborState.Status == GridCellState.StateStatus.Solved)
                {
                    if (!neighbor.Connects(option, neighborState.Choice))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool OptionIsValid(int x, int y, Tile option, GridCellState[,] newGrid)
        {
            SolverCell solverCell = solver.GetCell(x, y);
            foreach (SolverCellNeighbor neighbor in solverCell.Neighbors)
            {
                GridCellState neighborState = newGrid[neighbor.X, neighbor.Y];
                if(neighborState.Status == GridCellState.StateStatus.Solved)
                {
                    if(!neighbor.Connects(option, neighborState.Choice))
                    {
                        return false;
                    }
                }
            }
            return true;
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
            if(options.Count() == 1)
            {
                Choice = options.First();
                Status = StateStatus.Solved;
            }
            else
            {
                AvailableOptions = options;
                Status = AvailableOptions.Any() ? StateStatus.Unsolved : StateStatus.Unsolveable;
            }
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

        public override string ToString()
        {
            if(Status == StateStatus.Solved)
            {
                return "Solved: " + Choice.Sprite.name;
            }
            if(Status == StateStatus.Unsolved)
            {
                return "Unsolved with " + AvailableOptions.Count() + " options";
            }
            return "Unsolveable";
        }
    }
}