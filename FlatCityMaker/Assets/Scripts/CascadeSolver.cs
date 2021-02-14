using Packages.Rider.Editor.PostProcessors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using TileDefinition;

// The logic is:
//  - Each cell starts with by getting their options from the designation table
//  - Of the cells that have remaining options...
//  - Each cell checks to see whether their current choice is valid
//  - If the answer is no, checks whether it's invalid neighbor has more options
//  - If the answer is no, the cell eliminates their current option from the set of available options
//  - If the answer is yes ???

public class CascadeSolver
{
    private const int SolverLimit = 100000;
    public List<CascadingSolveState> StateHistory { get; }
    public CascadingSolveState LastState { get; private set; }

    public CascadeSolver(MainGrid grid)
    {
        CascadingSolveState initialState = GetInitialState(grid.Cells);
        StateHistory = new List<CascadingSolveState>() { initialState };
        LastState = initialState;
        while(!LastState.IsEverythingSolved && StateHistory.Count < SolverLimit)
        {
            LastState = LastState.GetNextState();
            StateHistory.Add(LastState);
        }
    }

    private CascadingSolveState GetInitialState(GridCell[,] cells) // TODO: Redo this method for the real thing
    {
        int gridWidth = cells.GetLength(0);
        int gridHeight = cells.GetLength(1);
        List<CascadingSolverCell> solverCells = new List<CascadingSolverCell>();
        CascadngSolverCellForFlat[,] asGrid = new CascadngSolverCellForFlat[gridWidth, gridHeight];
        foreach (GridCell item in cells)
        {
            CascadngSolverCellForFlat solverCell = new CascadngSolverCellForFlat(this);
            asGrid[item.X, item.Y] = solverCell;
            solverCells.Add(solverCell);
        }
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                asGrid[x, y].SetNeighbors(x, y, asGrid, gridWidth, gridHeight);
            }
        }
        return new CascadingSolveState(solverCells);
    }

    internal CascadingSolverCellState GetStateOf(CascadingSolverCell cell)
    {
        return LastState.GetCellState(cell);
    }
}

public abstract class CascadingSolverCell
{
    private readonly CascadeSolver solver;
    public abstract IEnumerable<CascadingSolverCellNeighbor> Neighbors { get; }

    public CascadingSolverCell(CascadeSolver solver)
    {
        this.solver = solver;
    }

    public bool AllConnectionsValid()
    {
        Tile myChoice = solver.GetStateOf(this).CurrentChoice;
        foreach (CascadingSolverCellNeighbor neighbor in Neighbors)
        {
            Tile theirChoice = solver.GetStateOf(neighbor.Cell).CurrentChoice;
            if (!neighbor.IsValid(myChoice, theirChoice))
            {
                return false;
            }
        }
        return true;
    }
}

public class CascadngSolverCellForFlat : CascadingSolverCell // This won't be applicable to the grid
{
    private IEnumerable<CascadingSolverCellNeighbor> neighbors;
    public override IEnumerable<CascadingSolverCellNeighbor> Neighbors => neighbors;

    public CascadngSolverCellForFlat(CascadeSolver solver)
        :base(solver)
    { }

    public void SetNeighbors(int cellX, int cellY, CascadingSolverCell[,] cells, int gridWidth, int gridHeight)
    {
        neighbors = GetNeighbors(cellX, cellY, cells, gridWidth, gridHeight).ToList();
    }

    private IEnumerable<CascadingSolverCellNeighbor> GetNeighbors(int cellX, int cellY, CascadingSolverCell[,] cells, int gridWidth, int gridHeight)
    {
        if (cellX > 0)
        {
            CascadingSolverCell neighbor = cells[cellX - 1, cellY];
            yield return new RightNeighbor(neighbor);
        }
        if (cellX < gridWidth - 1)
        {
            CascadingSolverCell neighbor = cells[cellX + 1, cellY];
            yield return new LeftNeighbor(neighbor);
        }
        if (cellY > 0)
        {
            CascadingSolverCell neighbor = cells[cellX, cellY - 1];
            yield return new DownNeighbor(neighbor);
        }
        if (cellY < gridHeight - 1)
        {
            CascadingSolverCell neighbor = cells[cellX, cellY + 1];
            yield return new UpNeighbor(neighbor);
        }
    }


    public class LeftNeighbor : CascadingSolverCellNeighbor
    {
        public LeftNeighbor(CascadingSolverCell neighborCell)
            : base(neighborCell, Connects)
        { }

        private static bool Connects(Tile option, Tile neighborOption)
        {
            return option.Left == neighborOption.Right;
        }
    }

    public class RightNeighbor : CascadingSolverCellNeighbor
    {
        public RightNeighbor(CascadingSolverCell neighborCell)
            : base(neighborCell, Connects)
        { }

        private static bool Connects(Tile option, Tile neighborOption)
        {
            return option.Right == neighborOption.Left;
        }
    }

    public class UpNeighbor : CascadingSolverCellNeighbor
    {
        public UpNeighbor(CascadingSolverCell neighborCell)
            : base(neighborCell, Connects)
        { }

        private static bool Connects(Tile option, Tile neighborOption)
        {
            return option.Up == neighborOption.Down;
        }
    }

    public class DownNeighbor : CascadingSolverCellNeighbor
    {
        public DownNeighbor(CascadingSolverCell neighborCell)
            : base(neighborCell, Connects)
        { }


        private static bool Connects(Tile option, Tile neighborOption)
        {
            return option.Down == neighborOption.Up;
        }
    }
}

public class CascadingSolverCellNeighbor
{
    private readonly Func<Tile, Tile, bool> comparisonFunction;
    public CascadingSolverCell Cell { get; }
    public CascadingSolverCellNeighbor(CascadingSolverCell cell, Func<Tile, Tile, bool> comparisonFunction)
    {
        Cell = cell;
        this.comparisonFunction = comparisonFunction;
    }

    internal bool IsValid(Tile myChoice, Tile theirChoice)
    {
        return comparisonFunction(myChoice, theirChoice);
    }
}

public class CascadingSolverCellState
{
    public CascadingSolverCell Cell { get; }

    public Tile CurrentChoice { get; }
    public ReadOnlyCollection<Tile> RemainingOptions { get; }
    public CellStatus State { get; }

    public CascadingSolverCellState(IEnumerable<Tile> remainingOptions, CascadingSolverCell cell)
    {
        Cell = cell;
        RemainingOptions = remainingOptions.ToList().AsReadOnly();
        CurrentChoice = RemainingOptions[0];
        State = GetStatus();
    }

    public CascadingSolverCellState FallToNextOption()
    {
        IEnumerable<Tile> newOptions = RemainingOptions.Skip(1);
        return new CascadingSolverCellState(newOptions, Cell);
    }

    private CellStatus GetStatus()
    {
        if(RemainingOptions.Count == 1)
        {
            return CellStatus.OnLastOption;
        }
        return Cell.AllConnectionsValid() ? CellStatus.Valid : CellStatus.Invalid;
    }
}

public class VoxelDesignation
{
    public bool UpLeftFilled { get; }
    public bool UpRightFilled { get; }
    public bool DownLeftFilled { get; }
    public bool DownRightFilled { get; }
    public string Key { get; }

    public VoxelDesignation(bool upLeft,
        bool upRight,
        bool downLeft,
        bool downRight)
    {
        UpLeftFilled = upLeft;
        UpRightFilled = upRight;
        DownLeftFilled = downLeft;
        DownRightFilled = downRight;
        Key = UpLeftFilled + " " 
            + UpRightFilled + " " 
            + DownLeftFilled + " " 
            + DownRightFilled;
    }
}

public class OptionsByDesignation
{
    private readonly Dictionary<string, Tile[]> optionsByDesignationKey;
    public OptionsByDesignation(Tile[] tiles)
    {
        optionsByDesignationKey = GetOptionsByDesignationKey(tiles);
    }

    public Tile[] GetOptions(VoxelDesignation designation)
    {
        return optionsByDesignationKey[designation.Key];
    }

    private Dictionary<string, Tile[]> GetOptionsByDesignationKey(Tile[] allOptions)
    {
        return allOptions.GroupBy(item => item.GetDesignationKey())
            .ToDictionary(item => item.Key, item => item.ToArray());
    }
}

public class CascadingSolveState
{
    public IEnumerable<CascadingSolverCellState> Cells { get { return cellStateLookup.Values; } }
    public bool IsEverythingSolved { get; }

    private readonly Dictionary<CascadingSolverCell, CascadingSolverCellState> cellStateLookup;

    public CascadingSolveState(IEnumerable<CascadingSolverCell> cells)
    {
        cellStateLookup = new Dictionary<CascadingSolverCell, CascadingSolverCellState>();
        foreach (CascadingSolverCell cell in cells)
        {
            //cellStateLookup.Add(cell, new CascadingSolverCellState()) TODO: Once Tile DesignationKey is in
        }
        //TODO: Gonna need to feed this what it needs to make the cells and their states...
    }

    public CascadingSolveState GetNextState()
    {
        throw new NotImplementedException();
    }

    internal CascadingSolverCellState GetCellState(CascadingSolverCell cell)
    {
        return cellStateLookup[cell];
    }
}

public enum CellStatus
{
    Valid,
    Invalid,
    OnLastOption,
}