using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

    public CascadeSolver()
    {
        CascadingSolveState initialState = new CascadingSolveState();
        StateHistory = new List<CascadingSolveState>() { initialState };
        LastState = initialState;
        while(!LastState.IsEverythingSolved && StateHistory.Count < SolverLimit)
        {
            LastState = LastState.GetNextState();
            StateHistory.Add(LastState);
        }
    }

    internal CascadingSolverCellState GetStateOf(CascadingSolverCell cell)
    {
        return LastState.GetCellState(cell);
    }
}

public class CascadingSolverCell
{
    private readonly CascadeSolver solver;
    public IEnumerable<CascadingSolverCellNeighbor> Neighbors { get; }

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

public class CascadingSolverCellNeighbor
{
    private readonly Func<bool, Tile, Tile> comparisonFunction;
    public CascadingSolverCell Cell { get; }
    public CascadingSolverCellNeighbor(CascadingSolverCell cell, Func<bool, Tile, Tile> comparisonFunction)
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

public class OptionsByDesignation
{
    private readonly Dictionary<string, Tile[]> optionsByDesignationKey;
    public OptionsByDesignation(Tile[] blueprints)
    {
        Tile[] allOptions = GetAllOptions(blueprints).ToArray();
        optionsByDesignationKey = GetOptionsByDesignationKey(allOptions);
    }

    public Tile[] GetOptions(VoxelDesignation designation)
    {
        return optionsByDesignationKey[designation.Key];
    }

    private IEnumerable<Tile> GetAllOptions(VoxelBlueprint[] blueprints)
    {
        foreach (VoxelBlueprint blueprint in blueprints)
        {
            IEnumerable<Tile> options = blueprint.GenerateVisualOptions();
            foreach (Tile option in options)
            {
                yield return option;
            }
        }
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
            cellStateLookup.Add(cell, new CascadingSolverCellState())
        }
        //TODO: Gonna need to feed this what it needs to make the cells and their states...
    }

    public CascadingSolveState GetNextState()
    {

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