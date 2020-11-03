using System;
using System.Collections.Generic;
using System.Linq;
using TileDefinition;

public class GridCell
{
    private readonly MainGrid main;

    public int X { get; }
    public int Y { get; }

    public Tile FilledWith 
    { 
        get 
        { 
            if(Options.Any())
            {
                return Options[0];
            }
            return null;
        } 
    }

    public IReadOnlyList<Tile> OptionsFromDesignation { get; private set; }
    public IReadOnlyList<Tile> Options { get; private set; }

    private IReadOnlyCollection<Neighbor> neighbors;

    public GridCell(MainGrid main, int x, int y)
    {
        this.main = main;
        X = x;
        Y = y;
    }

    public void EstablishNeighbors(GridCell[,] cells)
    {
        Neighbor[] neighbors = new Neighbor[]
        {
            new Neighbor(this, cells, 1, 0, item => item.Left, item => item.Right),
            new Neighbor(this, cells, -1, 0,item => item.Right, item => item.Left),
            new Neighbor(this, cells, 0, -1,item => item.Up, item => item.Down),
            new Neighbor(this, cells, 0, 1, item => item.Down, item => item.Up),
        };
        this.neighbors = neighbors.Where(item => item.Cell != null).ToList();
    }

    public void ResetDesignationOptions()
    {
        // TODO: It should be possible to precompute these into a hash
        OptionsFromDesignation = main.AllOptions.Where(DesignationsAllowOption).ToArray();
    }

    private bool DesignationsAllowOption(Tile option)
    {
        return main.Designations.IsOptionAllowed(X, Y, option);
    }

    private bool FillIsValid()
    {
        return neighbors.All(item => item.DoesConnectTo(FilledWith));
    }

    private bool OptionIsValid(Tile blueprint)
    {
        return neighbors.All(item => item.CanConnectTo(blueprint));
    }

    private class Neighbor
    {
        public GridCell Cell { get; }
        private readonly Func<Tile, TileConnectionType> selfSelector;
        private readonly Func<Tile, TileConnectionType> neighborSelector;

        public Neighbor(GridCell source,
            GridCell[,] cells, 
            int xOffset, 
            int yOffset,
            Func<Tile, TileConnectionType> selfSelector,
            Func<Tile, TileConnectionType> neighborSelector)
        {
            int xIndex = source.X - xOffset;
            int yIndex = source.Y + yOffset;

            this.selfSelector = selfSelector;
            this.neighborSelector = neighborSelector;

            int width = cells.GetLength(0);
            int height = cells.GetLength(1);

            bool spaceLeft = xIndex >= 0;
            bool spaceRight = xIndex < width;
            bool spaceDown = yIndex >= 0;
            bool spaceUp = yIndex < height;
            if(spaceLeft && spaceRight && spaceDown && spaceUp)
            {
                Cell = cells[xIndex, yIndex];
            }
        }

        public bool DoesConnectTo(Tile tile)
        {
            return selfSelector(Cell.FilledWith) == neighborSelector(tile);
        }

        protected bool CanConnectTo(Tile tile, Func<Tile, TileConnectionType> sectorForSelf, Func<Tile, TileConnectionType> selectorForNeighbor)
        {
            return Cell.Options.Any(item => sectorForSelf(item) == selectorForNeighbor(tile));
        }

        public virtual bool CanConnectTo(Tile tile)
        {
            if(tile == null)
            {
                throw new Exception("You may ask yourself, how did I get here?");
            }
            return CanConnectTo(tile, selfSelector, neighborSelector);
        }
    }
}

public class GridSolver
{
    // Pick a cell and pick it's first option
    // Go to neighbor A. Pick first option that connects to solved neighbors. Then march.
    
}

public class ResolvingCell
{
    public IReadOnlyList<Tile> Options { get; }
    private IReadOnlyCollection<SolvedNeighbor> solvedNeighbors;
    private IReadOnlyCollection<ResolvingCell> unsolvedNeighbors;

    public 

    public ResolvingCell(List<Tile> options)
    {
        Options = options;
    }

    public bool Process()
    {
        if(FirstOptionIsValid())
        {
            return 
        }
        else
        {
            if(Options.Count > 1) // Try the next option
            {
                UnsolvedCell next = new UnsolvedCell(Options.Skip(1).ToList());

            }
            else
            {
                return null; // This cell can't be solved on the current grid
            }
        }
    }

    private bool FirstOptionIsValid()
    {
        return solvedNeighbors.All(neighbor => neighbor.ConnectionIsValid(Options[0]));
    }
}

public class SolvedNeighbor
{
    public Tile Choice;

    public bool ConnectionIsValid(Tile tile)
    {
        throw new NotImplementedException();
    }
}