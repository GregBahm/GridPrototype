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

    public bool IsDirty { get { return main.DirtyCells.Contains(this); } }

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

    public void ResetDesignationOptions()
    {
        // TODO: It should be possible to precompute these into a hash
        OptionsFromDesignation = main.AllOptions.Where(DesignationsAllowOption).ToArray();
        Options = OptionsFromDesignation;
        if (!OptionsFromDesignation.Any())
        {
            throw new Exception("Zero options from designation table");
        }
        SetAsDirty();
        DirtyNeighbors();
    }

    private void DirtyNeighbors()
    {
        foreach (Neighbor neighbor in neighbors)
        {
            neighbor.Cell.SetAsDirty();
        }
    }

    private void SetAsDirty()
    {
        main.DirtyCells.Add(this);
        main.UnsolvedCells.Add(this);
        if(!main.RefreshedCells.Contains(this))
        {
            Options = OptionsFromDesignation;
            main.RefreshedCells.Add(this);
        }
    }

    public void UpdateOptions()
    {
        if(Options.Count > 1)
        {
            int oldOptionsCount = Options.Count;
            List<Tile> newOptions = Options.Where(OptionIsValid).ToList();
            if(!newOptions.Any())
            {
                Options = new List<Tile>() { FilledWith };
                main.DirtyCells.Remove(this);
            }
            else
            {
                Options = newOptions;
            }
            if(Options.Count != oldOptionsCount)
            {
                DirtyNeighbors();
            }
        }
        main.DirtyCells.Remove(this);
    }

    public void UpdateUnsolvedCell()
    {
        if(Options.Count == 1 || FillIsValid())
        {
            main.UnsolvedCells.Remove(this);
        }
        else
        {
            Options = Options.Skip(1).ToList();
        }
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

        public override bool CanConnectTo(Tile tile)
        {
            if(base.CanConnectTo(tile))
            {
                return CanConnectTo(tile, selfCornerASelector, neighborCornerASelector)
                    && CanConnectTo(tile, selfCornerBSelector, neighborCornerBSelector);
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

