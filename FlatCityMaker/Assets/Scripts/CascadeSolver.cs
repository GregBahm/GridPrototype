using Packages.Rider.Editor.PostProcessors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
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
        CascadingSolveState initialState = new CascadingSolveStateForFlat(this, grid);
        StateHistory = new List<CascadingSolveState>() { initialState };
        LastState = initialState;
        while(!LastState.IsEverythingSolved && StateHistory.Count < SolverLimit)
        {
            LastState = LastState.GetNextState();
            StateHistory.Add(LastState);
        }
    }
}

public abstract class CascadingSolverCellConnections
{
    public abstract IEnumerable<CascadingSolverCellConnection> Neighbors { get; }

    public CascadingSolverCellConnections()
    {
    }

    public bool AllConnectionsValid(CascadingSolveState state)
    {
        Tile myChoice = state.GetCellState(this).CurrentChoice;
        foreach (CascadingSolverCellConnection neighbor in Neighbors)
        {
            Tile theirChoice = state.GetCellState(neighbor.Cell).CurrentChoice;
            if (!neighbor.IsValid(myChoice, theirChoice))
            {
                return false;
            }
        }
        return true;
    }
}

public class CascadngSolverConnectionsFlat : CascadingSolverCellConnections // This won't be applicable to the grid
{
    public int X { get; }
    public int Y { get; }

    private IEnumerable<CascadingSolverCellConnection> neighbors;
    public override IEnumerable<CascadingSolverCellConnection> Neighbors => neighbors;

    public CascadngSolverConnectionsFlat(int x, int y)
        :base()
    {
        X = x;
        Y = y;
    }

    public void SetNeighbors(int cellX, int cellY, CascadingSolverCellConnections[,] cells, int gridWidth, int gridHeight)
    {
        neighbors = GetNeighbors(cellX, cellY, cells, gridWidth, gridHeight).ToList();
    }

    private IEnumerable<CascadingSolverCellConnection> GetNeighbors(int cellX, int cellY, CascadingSolverCellConnections[,] cells, int gridWidth, int gridHeight)
    {
        if (cellX > 0)
        {
            CascadingSolverCellConnections neighbor = cells[cellX - 1, cellY];
            yield return new RightNeighbor(neighbor);
        }
        if (cellX < gridWidth - 1)
        {
            CascadingSolverCellConnections neighbor = cells[cellX + 1, cellY];
            yield return new LeftNeighbor(neighbor);
        }
        if (cellY > 0)
        {
            CascadingSolverCellConnections neighbor = cells[cellX, cellY - 1];
            yield return new DownNeighbor(neighbor);
        }
        if (cellY < gridHeight - 1)
        {
            CascadingSolverCellConnections neighbor = cells[cellX, cellY + 1];
            yield return new UpNeighbor(neighbor);
        }
    }


    public class LeftNeighbor : CascadingSolverCellConnection
    {
        public LeftNeighbor(CascadingSolverCellConnections neighborCell)
            : base(neighborCell, Connects)
        { }

        private static bool Connects(Tile option, Tile neighborOption)
        {
            return option.Left == neighborOption.Right;
        }
    }

    public class RightNeighbor : CascadingSolverCellConnection
    {
        public RightNeighbor(CascadingSolverCellConnections neighborCell)
            : base(neighborCell, Connects)
        { }

        private static bool Connects(Tile option, Tile neighborOption)
        {
            return option.Right == neighborOption.Left;
        }
    }

    public class UpNeighbor : CascadingSolverCellConnection
    {
        public UpNeighbor(CascadingSolverCellConnections neighborCell)
            : base(neighborCell, Connects)
        { }

        private static bool Connects(Tile option, Tile neighborOption)
        {
            return option.Up == neighborOption.Down;
        }
    }

    public class DownNeighbor : CascadingSolverCellConnection
    {
        public DownNeighbor(CascadingSolverCellConnections neighborCell)
            : base(neighborCell, Connects)
        { }


        private static bool Connects(Tile option, Tile neighborOption)
        {
            return option.Down == neighborOption.Up;
        }
    }
}

public class CascadingSolverCellConnection
{
    private readonly Func<Tile, Tile, bool> comparisonFunction;
    public CascadingSolverCellConnections Cell { get; }
    public CascadingSolverCellConnection(CascadingSolverCellConnections cell, Func<Tile, Tile, bool> comparisonFunction)
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
    public CascadingSolverCellConnections Connections { get; }

    public Tile CurrentChoice { get; }
    public ReadOnlyCollection<Tile> RemainingOptions { get; }

    public CellStatus Status { get; private set; }

    public CascadingSolverCellState(IEnumerable<Tile> remainingOptions, CascadingSolverCellConnections connections)
    {
        Connections = connections;
        RemainingOptions = remainingOptions.ToList().AsReadOnly();
        CurrentChoice = RemainingOptions[0];
    }

    public CascadingSolverCellState FallToNextOption()
    {
        IEnumerable<Tile> newOptions = RemainingOptions.Skip(1);
        return new CascadingSolverCellState(newOptions, Connections);
    }

    public void SetStatus(CascadingSolveState state)
    {
        if(RemainingOptions.Count == 1)
        {
            Status = CellStatus.OnLastOption;
        }
        else
        {
            Status = Connections.AllConnectionsValid(state) ? CellStatus.Valid : CellStatus.Invalid;
        }
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

public class CascadingSolveStateForFlat : CascadingSolveState
{
    public CascadingSolveStateForFlat(CascadeSolver solver, MainGrid grid)
        :base()
    {
        Dictionary<CascadingSolverCellConnections, CascadingSolverCellState> initialLookup = new Dictionary<CascadingSolverCellConnections, CascadingSolverCellState>();

        CascadngSolverConnectionsFlat[,] asGrid = new CascadngSolverConnectionsFlat[grid.Width, grid.Height];

        for (int x = 0; x < grid.Width; x++)
        {
            for (int y = 0; y < grid.Height; y++)
            {
                GridCell item = grid.Cells[x, y];
                CascadngSolverConnectionsFlat solverCell = new CascadngSolverConnectionsFlat(x, y);
                asGrid[item.X, item.Y] = solverCell;
            }
        }
        for (int x = 0; x < grid.Width; x++)
        {
            for (int y = 0; y < grid.Height; y++)
            {
                CascadngSolverConnectionsFlat connections = asGrid[x, y];
                connections.SetNeighbors(x, y, asGrid, grid.Width, grid.Height);
                IEnumerable<Tile> options = grid.Cells[x, y].OptionsFromDesignation;
                CascadingSolverCellState cellState = new CascadingSolverCellState(options, connections);
                initialLookup.Add(connections, cellState);
            }
        }
        this.cellStateLookup = initialLookup;
        foreach (CascadingSolverCellState cellState in Cells)
        {
            cellState.SetStatus(this);
        }
    }
}

public class CascadingSolveState
{
    public IEnumerable<CascadingSolverCellState> Cells { get { return cellStateLookup.Values; } }
    public bool IsEverythingSolved { get; }

    protected IReadOnlyDictionary<CascadingSolverCellConnections, CascadingSolverCellState> cellStateLookup;

    protected CascadingSolveState()
    {
        IsEverythingSolved = false;
    }

    private CascadingSolveState(IReadOnlyDictionary<CascadingSolverCellConnections, CascadingSolverCellState> cellStateLookup)
    {
        this.cellStateLookup = cellStateLookup;
        foreach (CascadingSolverCellState item in Cells)
        {
            item.SetStatus(this);
        }
        IsEverythingSolved = Cells.All(item => item.Status != CellStatus.Invalid);
    }

    public CascadingSolveState GetNextState()
    {
        Dictionary<CascadingSolverCellConnections, CascadingSolverCellState> newState = cellStateLookup.ToDictionary(item => item.Key, item => item.Value);
        //  - Each cell checks to see whether their current choice is valid
        //  - If the answer is no, checks whether it's invalid neighbor has more options
        //  - If the answer is no, the cell eliminates their current option from the set of available options
        //  - If the answer is yes ???
        foreach (CascadingSolverCellState item in Cells.Where(item => item.Status == CellStatus.Invalid))
        {
            newState[item.Connections] = item.FallToNextOption();
        }
        return new CascadingSolveState(newState);

    }

    internal CascadingSolverCellState GetCellState(CascadingSolverCellConnections connections)
    {
        return cellStateLookup[connections];
    }
}

public enum CellStatus
{
    Valid,
    Invalid,
    OnLastOption,
}