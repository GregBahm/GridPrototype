using System;
using System.Collections.Generic;
using System.Linq;
using TileDefinition;

public class GridCell
{
    private readonly MainGrid main;

    public int X { get; }
    public int Y { get; }

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

    public void Reset()
    {
        FilledWith = null;
        Options = main.AllOptions.Where(DesignationsAllowOption).ToArray();
        if (Options.Count == 0)
        {
            throw new Exception("Impossible from the drop!");
        }
        main.DirtyCells.Add(this);
        main.EmptyCells.Add(this);
    }

    public void UpdateOptions()
    {
        int optionsCount = Options.Count;
        Tile[] newOptions = Options.Where(OptionIsValid).ToArray();
        if (newOptions.Length == 0)
        {
            throw new Exception("Impossible!");
        }
        Options = newOptions;
        main.DirtyCells.Remove(this);
        if(newOptions.Length != optionsCount)
        {
            foreach (Neighbor neighbor in neighbors)
            {
                neighbor.Cell.SetDirty();
            }
        }
    }

    private bool DesignationsAllowOption(Tile option)
    {
        return main.Designations.IsOptionAllowed(X, Y, option);
    }

    private void SetDirty()
    {
        if(FilledWith == null)
        {
            main.DirtyCells.Add(this);
        }
    }

    private bool OptionIsValid(Tile blueprint)
    {
        return neighbors.All(item => item.DoesConnectTo(blueprint));
    }

    internal void FillSelfWithFirstOption()
    {
        FilledWith = Options.First();
        main.EmptyCells.Remove(this);
        main.DirtyCells.Remove(this);
        foreach (Neighbor neighbor in neighbors)
        {
            neighbor.Cell.SetDirty();
        }
    }

    internal void FillSelfWithRandomOption()
    {
        Tile[] optionsArray = Options.ToArray();
        int rand = UnityEngine.Random.Range(0, optionsArray.Length);
        FilledWith = optionsArray[rand];
        main.EmptyCells.Remove(this);
        main.DirtyCells.Remove(this);
        foreach (Neighbor neighbor in neighbors)
        {
            neighbor.Cell.SetDirty();
        }
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
}

