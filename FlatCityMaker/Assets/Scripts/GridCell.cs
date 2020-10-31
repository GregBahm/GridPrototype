using System;
using System.Collections.Generic;
using System.Linq;
using TileDefinition;

public class GridCell
{
    private readonly MainGrid main;

    public int X { get; }
    public int Y { get; }

    public IReadOnlyList<Tile> OptionsFromDesignation { get; private set; }
    public IReadOnlyList<Tile> Options { get; private set; }
    public Tile FilledWith { get; set; }
    
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
            new OrthagonalNeighbor(this, cells, 1, 0, item => item.Left, item => item.Right,
                                                      item => item.UpLeft, item => item.UpRight, item => item.DownLeft, item => item.DownRight),
            new OrthagonalNeighbor(this, cells, -1, 0,item => item.Right, item => item.Left,
                                                      item => item.UpRight, item => item.UpLeft, item => item.DownRight, item => item.DownLeft),
            new OrthagonalNeighbor(this, cells, 0, -1,item => item.Up, item => item.Down,
                                                      item => item.UpLeft, item => item.DownLeft, item => item.UpRight, item => item.DownRight),
            new OrthagonalNeighbor(this, cells, 0, 1, item => item.Down, item => item.Up,
                                                      item => item.DownLeft, item => item.UpLeft, item => item.DownRight, item => item.UpRight),
            new Neighbor(this, cells, 1, -1, item => item.UpLeft, item => item.DownRight),
            new Neighbor(this, cells, -1, -1, item => item.UpRight, item => item.DownLeft),
            new Neighbor(this, cells, 1, 1, item => item.DownLeft, item => item.UpRight),
            new Neighbor(this, cells, -1, 1, item => item.DownRight, item => item.UpLeft)
        };
        this.neighbors = neighbors.Where(item => item.Cell != null).ToList();
    }

    internal void FillSelf()
    {
        Tile[] baseOptions = Options.Where(OptionIsValid).ToArray();
        if (!baseOptions.Any())
        {
            PropogateReset();
        }
        else
        {
            FilledWith = Options[0];
            main.EmptyCells.Remove(this);
            main.SolvedCells.Add(this);
        }
    }

    public void UpdateDesignationOptions()
    {
        // TODO: It should be possible to precompute these into a hash
        OptionsFromDesignation = main.AllOptions.Where(DesignationsAllowOption).ToArray();
    }

    public void Reset()
    {
        FilledWith = null;
        main.EmptyCells.Add(this);
        Options = OptionsFromDesignation.Where(OptionIsValid).ToArray();
        if (Options.Count == 0)
        {
            Options = baseOptions;
            PropogateReset();
        }
    }

    public void PropogateReset()
    {
        foreach (GridCell cell in neighbors.Select(item => item.Cell))
        {
            if(!main.EmptyCells.Contains(cell))
            {
                cell.Reset();
            }
        }
    }

    public void UpdateEmptyCell()
    {
        Options = Options.Where(OptionIsValid).ToArray();
        if (Options.Count == 0)
        {
            Options = main.AllOptions.Where(DesignationsAllowOption).ToArray();
            PropogateReset();
        }
        if (Options.Count == 1)
        {
            FillSelf();
        }
    }

    private bool DesignationsAllowOption(Tile option)
    {
        return main.Designations.IsOptionAllowed(X, Y, option);
    }

    private bool OptionIsValid(Tile blueprint)
    {
        return neighbors.All(item => item.DoesConnectTo(blueprint));
    }

    private class OrthagonalNeighbor : Neighbor
    {
        private readonly Func<Tile, TileConnectionType> selfCornerASelector;
        private readonly Func<Tile, TileConnectionType> neighborCornerASelector;

        private readonly Func<Tile, TileConnectionType> selfCornerBSelector;
        private readonly Func<Tile, TileConnectionType> neighborCornerBSelector;

        public OrthagonalNeighbor(GridCell source,
            GridCell[,] cells,
            int xOffset,
            int yOffset,
            Func<Tile, TileConnectionType> selfSelector,
            Func<Tile, TileConnectionType> neighborSelector,
            Func<Tile, TileConnectionType> selfCornerASelector, 
            Func<Tile, TileConnectionType> neighborCornerASelector, 
            Func<Tile, TileConnectionType> selfCornerBSelector, 
            Func<Tile, TileConnectionType> neighborCornerBSelector)
            :base(source, cells, xOffset, yOffset, selfSelector, neighborSelector)
        {
            this.selfCornerASelector = selfCornerASelector;
            this.neighborCornerASelector = neighborCornerASelector;
            this.selfCornerBSelector = selfCornerBSelector;
            this.neighborCornerBSelector = neighborCornerBSelector;
        }

        public override bool DoesConnectTo(Tile tile)
        {
            if(base.DoesConnectTo(tile))
            {
                return DoesConnectTo(tile, selfCornerASelector, neighborCornerASelector)
                    && DoesConnectTo(tile, selfCornerBSelector, neighborCornerBSelector);
            }
            return false;
        }
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

        protected bool DoesConnectTo(Tile tile, Func<Tile, TileConnectionType> sectorForSelf, Func<Tile, TileConnectionType> selectorForNeighbor)
        {
            if (Cell.FilledWith != null)
            {
                return sectorForSelf(Cell.FilledWith) == selectorForNeighbor(tile);
            }
            return Cell.Options.Any(item => sectorForSelf(item) == selectorForNeighbor(tile));
        }

        public virtual bool DoesConnectTo(Tile tile)
        {
            return DoesConnectTo(tile, selfSelector, neighborSelector);
        }
    }

    public override string ToString()
    {
        string ret = "(" + X + "," + Y + ")";
        if(FilledWith != null)
        {
            return ret + "filled with " + FilledWith.name;
        }
        ret += " Options: ";
        foreach (var option in Options)
        {
            ret += option.Sprite.name + " ";
        }
        return ret;
    }
}

