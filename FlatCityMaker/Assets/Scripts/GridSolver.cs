using System;
using System.Collections.Generic;
using System.Linq;
using TileDefinition;

public class GridSolver
{
    private readonly MainGrid currentGrid;
    private ResolvedCell resolvedStartCell;

    public GridSolver(MainGrid currentGrid, GridCell solveStartCell)
    {
        this.currentGrid = currentGrid;
        this.resolvedStartCell = GetStartCellResolved(solveStartCell);
    }

    public void ApplySolve()
    {
        RecursivelyApplyToGrid(resolvedStartCell);
    }

    private void RecursivelyApplyToGrid(ResolvedCell cell)
    {
        cell.GridCell.FilledWith = cell.Choice;
        foreach (ResolvedCell child in cell.ResolvedNeighbors)
        {
            RecursivelyApplyToGrid(child);
        }
    }

    private ResolvedCell GetStartCellResolved(GridCell solveStartCell)
    {
        throw new NotImplementedException();
    }
}


public class ResolvingCell
{
    private IReadOnlyList<Tile> options;
    
    private IReadOnlyCollection<ResolvedCell> resolvedNeighbors;
    private IReadOnlyCollection<UnresolvedCell> unresolvedNeighbors;

    public bool ChoiceIsValid { get; }
    public bool HasUnsolvedNeighbors { get; }

    public bool Resolveable { get { return Resolved != null; } }
    public ResolvingCell Resolved { get; }

    public ResolvingCell(List<Tile> options,
        List<ResolvedCell> resolvedNeighbors,
        List<UnresolvedCell> unresolvedNeighbors)
    {
        this.options = options;
        this.resolvedNeighbors = resolvedNeighbors;
        this.unresolvedNeighbors = unresolvedNeighbors;

        ChoiceIsValid = resolvedNeighbors.All(neighbor => neighbor.ConnectionIsValid(Choice));

        HasUnsolvedNeighbors = unresolvedNeighbors.Any();

        Resolved = TryGetResolved();
    }

    public ResolvingCell TryGetResolved()
    {
        foreach (Tile tile in options)
        {
            ResolvingCell resolved = TryGetResolved(tile);
            if (resolved != null)
            {
                return resolved;
            }
        }
        return null;
    }

    private ResolvingCell TryGetResolved(Tile choice)
    {
        List<ResolvedCell> resolvedNeighbors = new List<ResolvedCell>();
        foreach (UnresolvedCell unresolvedCell in unresolvedNeighbors)
        {
            ResolvedCell resolved =
        }
        return
    }
}

public class UnresolvedCell
{
}

public class ResolvedCell
{
    public GridCell GridCell { get; }
    public Tile Choice { get; }
    public IEnumerable<ResolvedCell> ResolvedNeighbors { get; }
    
    public bool ConnectionIsValid(Tile tile)
    {
        throw new NotImplementedException();
    }
}